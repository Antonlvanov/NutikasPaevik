using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using NutikasPaevik.Database;

namespace NutikasPaevik.Services
{
    public static class SyncService
    {
        private static readonly HttpClient _httpClient = App.HttpClient;

        public static async Task UploadEventsToServerAsync()
        {
            var dbContext = App.Services.GetService<AppDbContext>();

            try
            {
                var localEvents = await dbContext.Events.Where(e => e.UserID == UserService.Instance.UserId).ToListAsync();
                if (!localEvents.Any())
                {
                    Console.WriteLine("No local events to sync");
                    return;
                }
                if (string.IsNullOrEmpty(UserService.Instance.AuthToken))
                {
                    Console.WriteLine("Auth token missing");
                    throw new InvalidOperationException("No auth token");
                }
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserService.Instance.AuthToken);
                Console.WriteLine("Auth token set");

                var eventsToSend = localEvents.Select(ev => new
                {
                    syncId = ev.SyncId,
                    title = ev.Title,
                    description = ev.Description,
                    date = ev.Date.ToString("yyyy-MM-dd"),
                    startTime = ev.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    endTime = ev.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    type = ev.Type.ToString(),
                    creationTime = ev.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    modifyTime = ev.ModifyTime.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToList();

                var json = JsonSerializer.Serialize(eventsToSend);
                Console.WriteLine("Events JSON prepared");
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine("Sending events to server");
                var response = await _httpClient.PostAsync("http://paevik.antonivanov23.thkit.ee/events/sync", content);
                Console.WriteLine($"Server response: {response.StatusCode}");
                response.EnsureSuccessStatusCode();
                Console.WriteLine("Events uploaded successfully");

                App.CalendarViewModel.MarkForRefresh();
                App.HomePageViewModel.MarkForRefresh();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Event upload failed: {ex.Message}");
                throw new Exception("Event upload error", ex);
            }
        }

        public static async Task DownloadEventsFromServerAsync()
        {
            var dbContext = App.Services.GetService<AppDbContext>();
            try
            {
                var userId = UserService.Instance.UserId;
                var token = UserService.Instance.AuthToken;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var url = "http://paevik.antonivanov23.thkit.ee/events";
                var response = await _httpClient.GetAsync(url);
                Console.WriteLine($"Server response: {response.StatusCode}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var jsonArray = JsonSerializer.Deserialize<JsonElement>(content);
                var modifiedEvents = new List<JsonElement>();
                foreach (var evt in jsonArray.EnumerateArray())
                {
                    var evtObject = evt.GetRawText();
                    var evtDict = JsonSerializer.Deserialize<Dictionary<string, object>>(evtObject);

                    if (evtDict.ContainsKey("Date") && evtDict["Date"] != null)
                    {
                        string dateStr = evtDict["Date"].ToString();
                        if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                        {
                            evtDict["Date"] = $"{dateStr} 00:00:00";
                        }
                    }

                    var modifiedEvtJson = JsonSerializer.Serialize(evtDict);
                    modifiedEvents.Add(JsonSerializer.Deserialize<JsonElement>(modifiedEvtJson));
                }

                var modifiedContent = JsonSerializer.Serialize(modifiedEvents);
                Console.WriteLine("Fixed event JSON");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                options.Converters.Add(new CustomDateTimeConverter());
                options.Converters.Add(new CustomNullableDateTimeConverter());
                options.Converters.Add(new JsonStringEnumConverter());
                var serverEvents = JsonSerializer.Deserialize<List<Event>>(modifiedContent, options);
                Console.WriteLine($"Deserialized {serverEvents?.Count ?? 0} events");

                if (serverEvents == null) serverEvents = new List<Event>();

                var localEvents = await dbContext.Events.Where(e => e.UserID == userId).ToListAsync();
                Console.WriteLine($"Found {localEvents.Count} local events");

                foreach (var serverEvent in serverEvents)
                {
                    serverEvent.UserID = userId;
                    var localEvent = localEvents.FirstOrDefault(e => e.SyncId == serverEvent.SyncId);
                    if (localEvent != null)
                    {
                        if (serverEvent.Status == "deleted")
                        {
                            Console.WriteLine($"Deleting local event {serverEvent.SyncId}");
                            dbContext.Events.Remove(localEvent);
                        }
                        else
                        {
                            Console.WriteLine($"Updating local event {serverEvent.SyncId}");
                            localEvent.Title = serverEvent.Title;
                            localEvent.Description = serverEvent.Description;
                            localEvent.Date = serverEvent.Date;
                            localEvent.StartTime = serverEvent.StartTime;
                            localEvent.EndTime = serverEvent.EndTime;
                            localEvent.Type = serverEvent.Type;
                            localEvent.CreationTime = serverEvent.CreationTime;
                            localEvent.ModifyTime = serverEvent.ModifyTime;
                            localEvent.LastSyncTime = serverEvent.LastSyncTime;
                            localEvent.Status = serverEvent.Status;
                            dbContext.Events.Update(localEvent);
                        }
                    }
                    else if (serverEvent.Status != "deleted")
                    {
                        Console.WriteLine($"Adding new event {serverEvent.SyncId}");
                        dbContext.Events.Add(serverEvent);
                    }
                }

                var serverSyncIds = serverEvents.Select(e => e.SyncId).ToHashSet();
                var eventsToRemove = localEvents.Where(e => !serverSyncIds.Contains(e.SyncId)).ToList();
                if (eventsToRemove.Any())
                {
                    Console.WriteLine($"Removing {eventsToRemove.Count} outdated local events");
                    dbContext.Events.RemoveRange(eventsToRemove);
                }
                await dbContext.SaveChangesAsync();

                UserService.Instance.UpdateLastSyncTime(DateTime.Now);
                Console.WriteLine("Events synced successfully");

                App.CalendarViewModel.MarkForRefresh();
                App.HomePageViewModel.MarkForRefresh();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Event download failed: {ex.Message}");
                throw new Exception("Event download error", ex);
            }
        }

        public static async Task UploadNotesToServerAsync()
        {
            var dbContext = App.Services.GetService<AppDbContext>();
            try
            {
                var localNotes = await dbContext.Notes.Where(n => n.UserID == UserService.Instance.UserId).ToListAsync();
                if (!localNotes.Any())
                {
                    Console.WriteLine("No local notes to sync");
                    return;
                }
                if (string.IsNullOrEmpty(UserService.Instance.AuthToken))
                {
                    Console.WriteLine("Auth token missing");
                    throw new InvalidOperationException("No auth token");
                }
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserService.Instance.AuthToken);
                Console.WriteLine("Auth token set");

                var notesToSend = localNotes.Select(note => new
                {
                    syncId = note.SyncId,
                    title = note.Title,
                    content = note.Content,
                    noteColor = (int)note.NoteColor,
                    creationTime = note.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    notifyTime = note.NotifyTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                    modifyTime = note.ModifyTime?.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToList();

                var json = JsonSerializer.Serialize(notesToSend);
                Console.WriteLine("Notes JSON prepared");
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine("Sending notes to server");
                var response = await _httpClient.PostAsync("http://paevik.antonivanov23.thkit.ee/notes/sync", content);
                Console.WriteLine($"Server response: {response.StatusCode}");
                response.EnsureSuccessStatusCode();
                Console.WriteLine("Notes uploaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Note upload failed: {ex.Message}");
                throw new Exception("Note upload error", ex);
            }
        }

        public static async Task DownloadNotesFromServerAsync()
        {
            var dbContext = App.Services.GetService<AppDbContext>();
            try
            {
                var userId = UserService.Instance.UserId;
                var token = UserService.Instance.AuthToken;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("http://paevik.antonivanov23.thkit.ee/notes");
                Console.WriteLine($"Server response: {response.StatusCode}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine("Deserializing notes");
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                options.Converters.Add(new CustomDateTimeConverter());
                options.Converters.Add(new CustomNullableDateTimeConverter());
                Console.WriteLine("JSON options configured");

                List<Note> serverNotes;
                try
                {
                    serverNotes = JsonSerializer.Deserialize<List<Note>>(content, options);
                    Console.WriteLine($"Deserialized {serverNotes?.Count ?? 0} notes");
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"JSON deserialization failed: {jsonEx.Message}");
                    throw;
                }

                if (serverNotes == null)
                {
                    Console.WriteLine("No notes from server");
                    serverNotes = new List<Note>();
                }

                var localNotes = await dbContext.Notes.Where(n => n.UserID == userId).ToListAsync();
                Console.WriteLine($"Found {localNotes.Count} local notes");

                foreach (var serverNote in serverNotes)
                {
                    serverNote.UserID = userId;
                    var localNote = localNotes.FirstOrDefault(n => n.SyncId == serverNote.SyncId);
                    if (localNote != null)
                    {
                        if (serverNote.Status == "deleted")
                        {
                            Console.WriteLine($"Deleting local note {serverNote.SyncId}");
                            dbContext.Notes.Remove(localNote);
                        }
                        else
                        {
                            Console.WriteLine($"Updating local note {serverNote.SyncId}");
                            localNote.Title = serverNote.Title;
                            localNote.Content = serverNote.Content;
                            localNote.NoteColor = serverNote.NoteColor;
                            localNote.CreationTime = serverNote.CreationTime;
                            localNote.NotifyTime = serverNote.NotifyTime;
                            localNote.ModifyTime = serverNote.ModifyTime;
                            localNote.LastSyncTime = serverNote.LastSyncTime;
                            localNote.Status = serverNote.Status;
                            dbContext.Notes.Update(localNote);
                        }
                    }
                    else if (serverNote.Status != "deleted")
                    {
                        Console.WriteLine($"Adding new note {serverNote.SyncId}");
                        dbContext.Notes.Add(serverNote);
                    }
                }

                var serverSyncIds = serverNotes.Select(n => n.SyncId).ToHashSet();
                var notesToRemove = localNotes.Where(n => !serverSyncIds.Contains(n.SyncId)).ToList();
                if (notesToRemove.Any())
                {
                    Console.WriteLine($"Removing {notesToRemove.Count} outdated local notes");
                    dbContext.Notes.RemoveRange(notesToRemove);
                }

                await dbContext.SaveChangesAsync();
                UserService.Instance.UpdateLastSyncTime(DateTime.Now);
                App.DiaryViewModel.MarkForRefresh();
                App.HomePageViewModel.MarkForRefresh();
                Console.WriteLine("Notes synced successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Note download failed: {ex.Message}");
                throw new Exception("Note download error", ex);
            }
        }
        public static async Task DeleteEventAsync(string syncId)
        {
            var token = UserService.Instance.AuthToken;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            Console.WriteLine($"Deleting event {syncId}");
            var response = await _httpClient.DeleteAsync($"http://paevik.antonivanov23.thkit.ee/events/{syncId}");
            Console.WriteLine($"Server response: {response.StatusCode}");
            response.EnsureSuccessStatusCode();
            Console.WriteLine("Event deleted from server");

            App.HomePageViewModel.MarkForRefresh();
            App.CalendarViewModel.MarkForRefresh();
        }

        public static async Task DeleteNoteAsync(string syncId)
        {
            var token = UserService.Instance.AuthToken;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            Console.WriteLine($"Deleting note {syncId}");
            var response = await _httpClient.DeleteAsync($"http://paevik.antonivanov23.thkit.ee/notes/{syncId}");
            Console.WriteLine($"Server response: {response.StatusCode}");
            response.EnsureSuccessStatusCode();
            Console.WriteLine("Note deleted from server");

            App.HomePageViewModel.MarkForRefresh();
        }

        public static async Task ClearLocalDatabaseAsync()
        {
            var dbContext = App.Services.GetService<AppDbContext>();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            UserService.Instance.UpdateLastSyncTime(default);

            App.DiaryViewModel.MarkForRefresh();
            App.HomePageViewModel.MarkForRefresh();
            App.CalendarViewModel.MarkForRefresh();
        }

        public static async Task ClearLocalEventsForCurrentUserAsync()
        {
            var dbContext = App.Services.GetService<AppDbContext>();
            var userEvents = await dbContext.Events.Where(n => n.UserID == UserService.Instance.UserId).ToListAsync();
            dbContext.Events.RemoveRange(userEvents);
            await dbContext.SaveChangesAsync();

            UserService.Instance.UpdateLastSyncTime(default);
            App.HomePageViewModel.MarkForRefresh();
            App.CalendarViewModel.MarkForRefresh();
        }

        public static async Task ClearLocalNotesForCurrentUserAsync()
        {
            var dbContext = App.Services.GetService<AppDbContext>();
            var userNotes = await dbContext.Notes.Where(n => n.UserID == UserService.Instance.UserId).ToListAsync();
            dbContext.Notes.RemoveRange(userNotes);
            await dbContext.SaveChangesAsync();

            UserService.Instance.UpdateLastSyncTime(default);
            App.DiaryViewModel.MarkForRefresh();
            App.HomePageViewModel.MarkForRefresh();
        }
    }
}
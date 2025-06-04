using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using NutikasPaevik.Database;
using NutikasPaevik.Services;

namespace NutikasPaevik.Services
{
    public static class SyncService
    {
        private static readonly HttpClient _httpClient = App.HttpClient;

        public static async Task SynchronizeNotesAsync()
        {
            var dbContext = App.Services.GetService<AppDbContext>();
            if (dbContext == null) throw new InvalidOperationException("AppDbContext is not available.");

            try
            {
                await UploadNotesToServerAsync();
                await DownloadNotesFromServerAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка синхронизации заметок: {ex.Message}");
                throw new Exception("Ошибка синхронизации заметок", ex);
            }
        }

        public static async Task UploadEventsToServerAsync()
        {
            var dbContext = App.Services.GetService<AppDbContext>();
            if (dbContext == null) throw new InvalidOperationException("AppDbContext is not available.");

            try
            {
                var localEvents = await dbContext.Events.Where(e => e.UserID == UserService.Instance.UserId).ToListAsync();
                if (!localEvents.Any())
                {
                    Console.WriteLine("Нет локальных событий для синхронизации.");
                    return;
                }
                if (string.IsNullOrEmpty(UserService.Instance.AuthToken))
                {
                    Console.WriteLine("Ошибка: Токен аутентификации не найден.");
                    throw new InvalidOperationException("Токен аутентификации не найден");
                }
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserService.Instance.AuthToken);
                Console.WriteLine("Токен авторизации установлен.");

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
                Console.WriteLine($"JSON для отправки: {json}");
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine("Отправка POST-запроса на http://paevik.antonivanov23.thkit.ee/events/sync...");
                var response = await _httpClient.PostAsync("http://paevik.antonivanov23.thkit.ee/events/sync", content);
                Console.WriteLine($"Статус ответа сервера: {response.StatusCode}");
                response.EnsureSuccessStatusCode();
                Console.WriteLine("Синхронизация событий client -> server завершена успешно.");


                App.CalendarViewModel.MarkForRefresh(); ////
                App.HomePageViewModel.MarkForRefresh(); ////
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в UploadEventsToServerAsync: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
                throw new Exception("Ошибка синхронизации событий client -> server", ex);
            }
        }

        public static async Task DownloadEventsFromServerAsync()
        {
            var dbContext = App.Services.GetService<AppDbContext>();
            if (dbContext == null)
            {
                Console.WriteLine("Ошибка: AppDbContext недоступен.");
                throw new InvalidOperationException("AppDbContext is not available.");
            }

            try
            {
                Console.WriteLine("Начало синхронизации server -> client для событий...");
                var userId = UserService.Instance.UserId;
                if (userId == 0)
                {
                    Console.WriteLine("Ошибка: ID пользователя не найден.");
                    throw new InvalidOperationException("ID пользователя не найден.");
                }

                var token = UserService.Instance.AuthToken;
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("Ошибка: Токен аутентификации не найден.");
                    throw new InvalidOperationException("Токен аутентификации не найден");
                }
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine("Токен авторизации установлен.");

                var url = "http://paevik.antonivanov23.thkit.ee/events";
                Console.WriteLine($"Отправка GET-запроса на {url}...");
                var response = await _httpClient.GetAsync(url);
                Console.WriteLine($"Статус ответа сервера: {response.StatusCode}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Сырой ответ сервера: {content}");

                // Парсим JSON и исправляем формат даты
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
                Console.WriteLine($"Исправленный JSON: {modifiedContent}");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                options.Converters.Add(new CustomDateTimeConverter());
                options.Converters.Add(new CustomNullableDateTimeConverter());
                options.Converters.Add(new JsonStringEnumConverter());
                var serverEvents = JsonSerializer.Deserialize<List<Event>>(modifiedContent, options);
                Console.WriteLine($"Десериализовано событий: {serverEvents?.Count ?? 0}");

                if (serverEvents == null) serverEvents = new List<Event>();

                var localEvents = await dbContext.Events.Where(e => e.UserID == userId).ToListAsync();
                Console.WriteLine($"Локальных событий: {localEvents.Count}");

                foreach (var serverEvent in serverEvents)
                {
                    serverEvent.UserID = userId;
                    var localEvent = localEvents.FirstOrDefault(e => e.SyncId == serverEvent.SyncId);
                    if (localEvent != null)
                    {
                        if (serverEvent.Status == "deleted")
                        {
                            Console.WriteLine($"Удаление локального события с SyncId={serverEvent.SyncId}");
                            dbContext.Events.Remove(localEvent);
                        }
                        else
                        {
                            Console.WriteLine($"Обновление локального события с SyncId={serverEvent.SyncId}");
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
                        Console.WriteLine($"Добавление нового события с SyncId={serverEvent.SyncId}");
                        dbContext.Events.Add(serverEvent);
                    }
                }

                var serverSyncIds = serverEvents.Select(e => e.SyncId).ToHashSet();
                var eventsToRemove = localEvents.Where(e => !serverSyncIds.Contains(e.SyncId)).ToList();
                if (eventsToRemove.Any())
                {
                    Console.WriteLine($"Удаление {eventsToRemove.Count} локальных событий, отсутствующих на сервере.");
                    dbContext.Events.RemoveRange(eventsToRemove);
                }

                await dbContext.SaveChangesAsync();
                UserService.Instance.UpdateLastSyncTime(DateTime.Now);
                Console.WriteLine("Синхронизация событий server -> client завершена успешно.");

                App.CalendarViewModel.MarkForRefresh();
                App.HomePageViewModel.MarkForRefresh();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в DownloadEventsFromServerAsync: {ex.Message}");
                throw new Exception("Ошибка синхронизации событий server -> client", ex);
            }
        }

        public static async Task DeleteEventAsync(string syncId)
        {
            var token = UserService.Instance.AuthToken;
            if (string.IsNullOrEmpty(token)) throw new InvalidOperationException("Токен аутентификации не найден.");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            Console.WriteLine($"Отправка DELETE-запроса для SyncId={syncId}...");
            var response = await _httpClient.DeleteAsync($"http://paevik.antonivanov23.thkit.ee/events/{syncId}");
            Console.WriteLine($"Статус ответа сервера: {response.StatusCode}");
            response.EnsureSuccessStatusCode();
            Console.WriteLine("Событие успешно удалено с сервера.");

            App.HomePageViewModel.MarkForRefresh(); ////
            App.CalendarViewModel.MarkForRefresh(); ////
        }

        public static async Task UploadNotesToServerAsync()
        {
            var dbContext = App.Services.GetService<AppDbContext>();
            if (dbContext == null) throw new InvalidOperationException("AppDbContext is not available.");

            try
            {
                var localNotes = await dbContext.Notes.Where(n => n.UserID == UserService.Instance.UserId).ToListAsync();
                if (!localNotes.Any())
                {
                    Console.WriteLine("Нет локальных заметок для синхронизации.");
                    return;
                }
                if (string.IsNullOrEmpty(UserService.Instance.AuthToken))
                {
                    Console.WriteLine("Ошибка: Токен аутентификации не найден.");
                    throw new InvalidOperationException("Токен аутентификации не найден");
                }
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserService.Instance.AuthToken);
                Console.WriteLine("Токен авторизации установлен.");

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
                Console.WriteLine($"JSON для отправки: {json}");
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine("Отправка POST-запроса на http://paevik.antonivanov23.thkit.ee/notes/sync...");
                var response = await _httpClient.PostAsync("http://paevik.antonivanov23.thkit.ee/notes/sync", content);
                Console.WriteLine($"Статус ответа сервера: {response.StatusCode}");
                response.EnsureSuccessStatusCode();
                Console.WriteLine("Синхронизация заметок client -> server завершена успешно.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в UploadNotesToServerAsync: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
                throw new Exception("Ошибка синхронизации заметок client -> server", ex);
            }
        }

        public static async Task DownloadNotesFromServerAsync()
        {
            var dbContext = App.Services.GetService<AppDbContext>();
            if (dbContext == null)
            {
                Console.WriteLine("Ошибка: AppDbContext недоступен.");
                throw new InvalidOperationException("AppDbContext is not available.");
            }

            try
            {
                Console.WriteLine("Начало синхронизации server -> client...");
                var userId = UserService.Instance.UserId;
                if (userId == 0)
                {
                    Console.WriteLine("Ошибка: ID пользователя не найден, загрузка заметок невозможна.");
                    throw new InvalidOperationException("ID пользователя не найден.");
                }

                var token = UserService.Instance.AuthToken;
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("Ошибка: Токен аутентификации не найден.");
                    throw new InvalidOperationException("Токен аутентификации не найден");
                }
                Console.WriteLine("Токен авторизации найден.");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine("Заголовок Authorization установлен.");

                var url = "http://paevik.antonivanov23.thkit.ee/notes";
                Console.WriteLine($"Отправка GET-запроса на {url}...");
                var response = await _httpClient.GetAsync(url);
                Console.WriteLine($"Статус ответа сервера: {response.StatusCode}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Сырой ответ сервера: {content}");

                Console.WriteLine("Начало десериализации ответа...");
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                options.Converters.Add(new CustomDateTimeConverter());
                options.Converters.Add(new CustomNullableDateTimeConverter());
                Console.WriteLine("JsonSerializerOptions настроены с CustomDateTimeConverter и CustomNullableDateTimeConverter.");

                List<Note> serverNotes;
                try
                {
                    serverNotes = JsonSerializer.Deserialize<List<Note>>(content, options);
                    Console.WriteLine($"Десериализовано заметок: {serverNotes?.Count ?? 0}");
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"Ошибка десериализации JSON: {jsonEx.Message}");
                    Console.WriteLine($"Стек вызовов: {jsonEx.StackTrace}");
                    throw;
                }

                if (serverNotes == null)
                {
                    Console.WriteLine("Ошибка: сервер вернул null или пустой список заметок.");
                    serverNotes = new List<Note>();
                }

                var localNotes = await dbContext.Notes.Where(n => n.UserID == userId).ToListAsync();
                Console.WriteLine($"Локальных заметок: {localNotes.Count}");

                foreach (var serverNote in serverNotes)
                {
                    serverNote.UserID = userId;
                    var localNote = localNotes.FirstOrDefault(n => n.SyncId == serverNote.SyncId);
                    if (localNote != null)
                    {
                        if (serverNote.Status == "deleted")
                        {
                            Console.WriteLine($"Удаление локальной заметки с SyncId={serverNote.SyncId}");
                            dbContext.Notes.Remove(localNote);
                        }
                        else
                        {
                            Console.WriteLine($"Обновление локальной заметки с SyncId={serverNote.SyncId}");
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
                        Console.WriteLine($"Добавление новой заметки с SyncId={serverNote.SyncId}");
                        dbContext.Notes.Add(serverNote);
                    }
                }

                var serverSyncIds = serverNotes.Select(n => n.SyncId).ToHashSet();
                var notesToRemove = localNotes.Where(n => !serverSyncIds.Contains(n.SyncId)).ToList();
                if (notesToRemove.Any())
                {
                    Console.WriteLine($"Удаление {notesToRemove.Count} локальных заметок, отсутствующих на сервере.");
                    dbContext.Notes.RemoveRange(notesToRemove);
                }

                await dbContext.SaveChangesAsync();
                UserService.Instance.UpdateLastSyncTime(DateTime.Now);
                App.DiaryViewModel.MarkForRefresh();
                App.HomePageViewModel.MarkForRefresh();
                Console.WriteLine("Синхронизация заметок server -> client завершена успешно.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в DownloadNotesFromServerAsync: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
                throw new Exception("Ошибка синхронизации заметок server -> client", ex);
            }
        }

        public static async Task DeleteNoteAsync(string syncId)
        {
            var token = UserService.Instance.AuthToken;;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            Console.WriteLine($"Отправка DELETE-запроса для SyncId={syncId}...");
            var response = await _httpClient.DeleteAsync($"http://paevik.antonivanov23.thkit.ee/notes/{syncId}");
            Console.WriteLine($"Статус ответа сервера: {response.StatusCode}");
            response.EnsureSuccessStatusCode();
            Console.WriteLine("Заметка успешно удалена с сервера.");

            App.HomePageViewModel.MarkForRefresh(); ////
        }

        public static async Task ClearLocalDatabaseAsync()
        {
            var dbContext = App.Services.GetService<AppDbContext>();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            UserService.Instance.UpdateLastSyncTime(default);

            App.DiaryViewModel.MarkForRefresh();
            App.HomePageViewModel.MarkForRefresh(); ////
            App.CalendarViewModel.MarkForRefresh(); ////
        }

        public static async Task ClearLocalEventsForCurrentUserAsync()
        {
            var dbContext = App.Services.GetService<AppDbContext>();
            var userEvents = await dbContext.Events.Where(n => n.UserID == UserService.Instance.UserId).ToListAsync();
            dbContext.Events.RemoveRange(userEvents);
            await dbContext.SaveChangesAsync();

            UserService.Instance.UpdateLastSyncTime(default);
            App.HomePageViewModel.MarkForRefresh(); ////
            App.CalendarViewModel.MarkForRefresh(); ////
        }

        public static async Task ClearLocalNotesForCurrentUserAsync()
        {
            var dbContext = App.Services.GetService<AppDbContext>();
            var userNotes = await dbContext.Notes.Where(n => n.UserID == UserService.Instance.UserId).ToListAsync();
            dbContext.Notes.RemoveRange(userNotes);
            await dbContext.SaveChangesAsync();

            UserService.Instance.UpdateLastSyncTime(default);
            App.DiaryViewModel.MarkForRefresh(); ////
            App.HomePageViewModel.MarkForRefresh(); ////
        }
    }
}
using NutikasPaevik.Enums;
using NutikasPaevik.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NutikasPaevik.Database
{
    public class Note
    {
        [Key]
        public int Id { get; set; }
        public required string SyncId { get; set; }
        public required int UserID { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public required NoteColor NoteColor { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime CreationTime { get; set; } = DateTime.Now;
        [JsonConverter(typeof(CustomNullableDateTimeConverter))]
        public DateTime? NotifyTime { get; set; } = DateTime.Now;
        [JsonConverter(typeof(CustomNullableDateTimeConverter))]
        public DateTime? ModifyTime { get; set; }
        [JsonConverter(typeof(CustomNullableDateTimeConverter))]
        public DateTime? LastSyncTime { get; set; }
        public string Status { get; set; }
    }
}

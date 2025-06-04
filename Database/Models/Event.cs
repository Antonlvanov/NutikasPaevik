using NutikasPaevik.Enums;
using System;

namespace NutikasPaevik.Database
{
    public class Event
    {
        public int Id { get; set; }
        public string SyncId { get; set; }
        public int UserID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public EventType Type { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public DateTime ModifyTime { get; set; } = DateTime.Now;
        public DateTime LastSyncTime { get; set; }
        public string Status { get; set; } = "created"; // "created", "updated", "deleted"
    }
}
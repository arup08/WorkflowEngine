using System;

namespace WorkflowEngine.Models
{
    public class HistoryEntry
    {
        public required string ActionId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    }
}

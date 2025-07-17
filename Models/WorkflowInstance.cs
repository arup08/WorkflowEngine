using System.Collections.Generic;

namespace WorkflowEngine.Models
{
    public class WorkflowInstance
    {
        public required string Id           { get; init; }
        public required string DefinitionId { get; init; }
        public required string CurrentState { get; set; }
        public List<HistoryEntry> History   { get; init; } = new();

        // Parameterless ctor for model binding, if needed
        public WorkflowInstance() { }

        // Convenience ctor when you create a new instance
        public WorkflowInstance(string id, string defId, string initialState)
        {
            Id = id;
            DefinitionId = defId;
            CurrentState = initialState;
        }
    }
}

using System.Collections.Generic;

namespace WorkflowEngine.Models
{
    public class WorkflowDefinition
    {
        public required string Id { get; init; }
        public List<State> States { get; init; } = new();
        public List<Action> Actions { get; init; } = new();
    }
}

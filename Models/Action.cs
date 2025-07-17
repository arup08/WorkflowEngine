using System.Collections.Generic;

namespace WorkflowEngine.Models
{
    public class Action
    {
        public required string Id         { get; init; }
        public required string Name       { get; init; }
        public bool Enabled               { get; init; } = true;
        public List<string> FromStates    { get; init; } = new();
        public required string ToState    { get; init; }
    }
}

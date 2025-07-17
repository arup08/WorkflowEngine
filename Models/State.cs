using System;

namespace WorkflowEngine.Models
{
    public class State
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public bool IsInitial { get; init; }
        public bool IsFinal   { get; init; }
        public bool Enabled   { get; init; } = true;
    }
}

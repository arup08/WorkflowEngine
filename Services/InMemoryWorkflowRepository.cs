using WorkflowEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WorkflowEngine.Services
{
    public class InMemoryWorkflowRepository : IWorkflowRepository
    {
        // Backing stores
        private readonly Dictionary<string, WorkflowDefinition> _definitions = new();
        private readonly Dictionary<string, WorkflowInstance>   _instances   = new();

        // ========== Definitions ==========

        public void AddDefinition(WorkflowDefinition definition)
        {
            if (_definitions.ContainsKey(definition.Id))
                throw new ArgumentException($"Workflow '{definition.Id}' already exists.");

            ValidateDefinition(definition);
            _definitions[definition.Id] = definition;
        }

        public WorkflowDefinition? GetDefinition(string id) =>
            _definitions.TryGetValue(id, out var def) ? def : null;

        public IEnumerable<WorkflowDefinition> ListDefinitions() =>
            _definitions.Values;

        // ========== Instances ==========

        public void AddInstance(WorkflowInstance instance)
        {
            if (_instances.ContainsKey(instance.Id))
                throw new ArgumentException($"Instance '{instance.Id}' already exists.");

            _instances[instance.Id] = instance;
        }

        public WorkflowInstance? GetInstance(string id) =>
            _instances.TryGetValue(id, out var inst) ? inst : null;

        public IEnumerable<WorkflowInstance> ListInstances() =>
            _instances.Values;

        // ========== Validation Helper ==========

        private void ValidateDefinition(WorkflowDefinition def)
        {
            // 1) Unique state IDs
            var stateIds = def.States.Select(s => s.Id).ToList();
            if (stateIds.Distinct().Count() != stateIds.Count)
                throw new ArgumentException("Duplicate State.Id values are not allowed.");

            // 2) Exactly one initial state
            if (def.States.Count(s => s.IsInitial) != 1)
                throw new ArgumentException("Must have exactly one state with IsInitial = true.");

            // 3) All actions refer to valid state IDs
            var validIds = stateIds.ToHashSet();
            foreach (var act in def.Actions)
            {
                if (!validIds.Contains(act.ToState)
                    || act.FromStates.Any(fs => !validIds.Contains(fs)))
                {
                    throw new ArgumentException(
                        $"Action '{act.Id}' has invalid FromStates or ToState.");
                }
            }
        }
    }
}

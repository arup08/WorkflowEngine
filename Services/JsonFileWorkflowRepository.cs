using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using WorkflowEngine.Models;

namespace WorkflowEngine.Services
{
    public class JsonFileWorkflowRepository : IWorkflowRepository
    {
        private readonly string _defsPath = Path.Combine("Data", "workflows.json");
        private readonly string _instsPath = Path.Combine("Data", "instances.json");

        private Dictionary<string, WorkflowDefinition> _definitions;
        private Dictionary<string, WorkflowInstance>   _instances;

        public JsonFileWorkflowRepository()
        {
            // Load or initialize definitions
            if (File.Exists(_defsPath))
            {
                var json = File.ReadAllText(_defsPath);
                _definitions = JsonSerializer.Deserialize<Dictionary<string, WorkflowDefinition>>(json)
                              ?? new Dictionary<string, WorkflowDefinition>();
            }
            else
            {
                _definitions = new Dictionary<string, WorkflowDefinition>();
            }

            // Load or initialize instances
            if (File.Exists(_instsPath))
            {
                var json = File.ReadAllText(_instsPath);
                _instances = JsonSerializer.Deserialize<Dictionary<string, WorkflowInstance>>(json)
                             ?? new Dictionary<string, WorkflowInstance>();
            }
            else
            {
                _instances = new Dictionary<string, WorkflowInstance>();
            }
        }

        // ─── Definitions ───────────────────────────────────────

        public void AddDefinition(WorkflowDefinition def)
        {
            if (_definitions.ContainsKey(def.Id))
                throw new ArgumentException($"Workflow '{def.Id}' already exists.");

            ValidateDefinition(def);
            _definitions[def.Id] = def;
            PersistDefinitions();
        }

        public WorkflowDefinition? GetDefinition(string id) =>
            _definitions.TryGetValue(id, out var d) ? d : null;

        public IEnumerable<WorkflowDefinition> ListDefinitions() =>
            _definitions.Values;

        // ─── Instances ─────────────────────────────────────────

        public void AddInstance(WorkflowInstance inst)
        {
            if (_instances.ContainsKey(inst.Id))
                throw new ArgumentException($"Instance '{inst.Id}' already exists.");

            _instances[inst.Id] = inst;
            PersistInstances();
        }

        public WorkflowInstance? GetInstance(string id) =>
            _instances.TryGetValue(id, out var i) ? i : null;

        public IEnumerable<WorkflowInstance> ListInstances() =>
            _instances.Values;

        // ─── Persistence Helpers ───────────────────────────────

        private void PersistDefinitions()
        {
            var json = JsonSerializer.Serialize(_definitions, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_defsPath, json);
        }

        private void PersistInstances()
        {
            var json = JsonSerializer.Serialize(_instances, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_instsPath, json);
        }

        // ─── Validation (copy from InMemory) ────────────────────

        private void ValidateDefinition(WorkflowDefinition def)
        {
            var stateIds = def.States.Select(s => s.Id).ToList();
            if (stateIds.Distinct().Count() != stateIds.Count)
                throw new ArgumentException("Duplicate State.Id values are not allowed.");

            if (def.States.Count(s => s.IsInitial) != 1)
                throw new ArgumentException("Must have exactly one state with IsInitial = true.");

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

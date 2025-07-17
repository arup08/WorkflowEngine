using WorkflowEngine.Models;
using System.Collections.Generic;

namespace WorkflowEngine.Services
{
    public interface IWorkflowRepository
    {
        // Definitions
        void AddDefinition(WorkflowDefinition definition);
        WorkflowDefinition? GetDefinition(string id);
        IEnumerable<WorkflowDefinition> ListDefinitions();

        // Instances (we’ll implement later)
        void AddInstance(WorkflowInstance instance);
        WorkflowInstance? GetInstance(string id);
        IEnumerable<WorkflowInstance> ListInstances();
    }
}

using System;
using System.Linq;
using WorkflowEngine.Models;
using WorkflowEngine.Services;


var builder = WebApplication.CreateBuilder(args);
// builder.Services.AddSingleton<IWorkflowRepository, InMemoryWorkflowRepository>();
builder.Services.AddSingleton<IWorkflowRepository, JsonFileWorkflowRepository>();

var app = builder.Build();

// Create a new workflow definition
app.MapPost("/workflows", (WorkflowDefinition def, IWorkflowRepository repo) =>
{
    try
    {
        repo.AddDefinition(def);
        return Results.Created($"/workflows/{def.Id}", def);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// List all definitions (for convenience)
app.MapGet("/workflows", (IWorkflowRepository repo) =>
    Results.Ok(repo.ListDefinitions()));

// Fetch a single definition
app.MapGet("/workflows/{id}", (string id, IWorkflowRepository repo) =>
    repo.GetDefinition(id) is { } def
        ? Results.Ok(def)
        : Results.NotFound(new { error = "Not found" }));

// Start a new instance of a workflow definition
app.MapPost("/workflows/{defId}/instances", (string defId, IWorkflowRepository repo) =>
{
    var def = repo.GetDefinition(defId);
    if (def is null)
        return Results.NotFound(new { error = $"Definition '{defId}' not found." });

    var initialState = def.States.Single(s => s.IsInitial && s.Enabled);
    var instId = Guid.NewGuid().ToString();
    var inst = new WorkflowInstance
    {
        Id           = instId,
        DefinitionId = def.Id,
        CurrentState = initialState.Id
    };


    repo.AddInstance(inst);
    return Results.Created($"/instances/{inst.Id}", inst);
});


// (Optional) List all instances
app.MapGet("/instances", (IWorkflowRepository repo) =>
    Results.Ok(repo.ListInstances()));

// Fetch a single instance
app.MapGet("/instances/{instId}", (string instId, IWorkflowRepository repo) =>
    repo.GetInstance(instId) is { } inst
        ? Results.Ok(inst)
        : Results.NotFound(new { error = $"Instance '{instId}' not found." }));



// Fire an action on an existing instance
app.MapPost("/instances/{instId}/actions/{actionId}", (string instId, string actionId, IWorkflowRepository repo) =>
{
    // 1) Fetch instance
    var inst = repo.GetInstance(instId);
    if (inst is null)
        return Results.NotFound(new { error = $"Instance '{instId}' not found." });

    // 2) Fetch definition
    var def = repo.GetDefinition(inst.DefinitionId);
    if (def is null)
        return Results.BadRequest(new { error = $"Definition '{inst.DefinitionId}' not found." });

    // 3) Find the action
    var act = def.Actions.SingleOrDefault(a => a.Id == actionId);
    if (act is null)
        return Results.NotFound(new { error = $"Action '{actionId}' not found." });
    if (!act.Enabled)
        return Results.BadRequest(new { error = $"Action '{actionId}' is disabled." });

    // 4) Validate current state
    if (!act.FromStates.Contains(inst.CurrentState))
        return Results.BadRequest(new { 
            error = $"Cannot fire action '{actionId}' from state '{inst.CurrentState}'." 
        });
    var currentStateDef = def.States.Single(s => s.Id == inst.CurrentState);
    if (currentStateDef.IsFinal)
        return Results.BadRequest(new { 
            error = $"Instance is already in final state '{currentStateDef.Id}'." 
        });

    // 5) Perform the transition
    inst.History.Add(new HistoryEntry { ActionId = actionId });
    inst.CurrentState = act.ToState;

    return Results.Ok(inst);
});

app.Run();

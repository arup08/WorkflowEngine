# WorkflowEngine

A minimal, configurable workflow engine backend built with .NET 9 and C#.
Clients can define workflows (states + transitions), start instances, execute actions (transitions), and inspect instance state/history.
Data is persisted in JSON files so definitions and instances survive restarts.

---

## Features

* **Define** workflows via HTTP (`POST /workflows`)
* **List** and **fetch** definitions (`GET /workflows`, `GET /workflows/{id}`)
* **Start** instances (`POST /workflows/{defId}/instances`)
* **Fire** actions (`POST /instances/{instId}/actions/{actionId}`)
* **Inspect** instances (`GET /instances`, `GET /instances/{instId}`)
* **Persistence**: workflows and instances saved to `Data/workflows.json` and `Data/instances.json`

---

## Prerequisites

* [.NET 9 SDK](https://dotnet.microsoft.com/download)
* PowerShell (Windows) or any REST client (curl, Postman)

---

## Getting Started

1. **Clone the repository**

   ```bash
   git clone <repo-url>
   cd WorkflowEngine
   ```

2. **Ensure `Data/` folder exists**

   ```bash
   mkdir Data
   ```

3. **Build & Run**

   ```bash
   dotnet build
   dotnet run
   ```

   By default, the API listens on `http://localhost:5087`.

---

## API Endpoints

### Create Workflow Definition

```http
POST /workflows
Content-Type: application/json

{
  "Id": "orderFlow",
  "States": [
    { "Id": "cart",     "Name": "Cart",     "IsInitial": true },
    { "Id": "placed",   "Name": "Placed" },
    { "Id": "shipped",  "Name": "Shipped" },
    { "Id": "delivered","Name": "Delivered", "IsFinal": true }
  ],
  "Actions": [
    { "Id": "place", "Name": "Place Order", "FromStates": ["cart"],   "ToState": "placed" },
    { "Id": "ship",  "Name": "Ship Order",  "FromStates": ["placed"], "ToState": "shipped" },
    { "Id": "deliver","Name": "Deliver Order","FromStates": ["shipped"],"ToState": "delivered" }
  ]
}
```

**Response:** `201 Created` with the created definition JSON.

### List All Definitions

```http
GET /workflows
```

**Response:** `200 OK` with array of definitions.

### Get Single Definition

```http
GET /workflows/{definitionId}
```

**Response:** `200 OK` with definition JSON, or `404 Not Found`.

### Start a Workflow Instance

```http
POST /workflows/{definitionId}/instances
```

**Response:** `201 Created` with new instance JSON:

```json
{
  "id": "<instance-guid>",
  "definitionId": "orderFlow",
  "currentState": "cart",
  "history": []
}
```

### List All Instances

```http
GET /instances
```

**Response:** `200 OK` array of instances.

### Get Single Instance

```http
GET /instances/{instanceId}
```

**Response:** `200 OK` with instance JSON, or `404 Not Found`.

### Fire an Action

```http
POST /instances/{instanceId}/actions/{actionId}
```

**Behavior:**

* Validates existence of instance, definition, and action.
* Ensures action is enabled and the instance’s current state is among `FromStates`.
* Rejects if instance is already in a final state.
* On success, updates `currentState`, appends a timestamped history entry, and returns `200 OK` with updated instance JSON.

**Error Responses:**

* `400 Bad Request` with `{ error: "..." }` for validation failures.
* `404 Not Found` if instance, definition, or action not found.

---

## Persistence

All data is stored in `Data/`:

* **Data/workflows.json** — stores all workflow definitions.
* **Data/instances.json** — stores all workflow instances.

On startup, the repository loads these files if they exist. On each create/mutate, it writes them back to disk (indented JSON).

---

## Error Handling

* **Duplicate IDs**: returns `400 Bad Request` (considered for upgrade to `409 Conflict`).
* **Invalid definitions**: missing initial state, duplicate state IDs, or actions referring to missing states.
* **Invalid transitions**: action fired from wrong state or on final state.

---

## Testing

Use PowerShell’s `Invoke-RestMethod`, `curl`, or Postman. Example in PowerShell:

```powershell
# Define workflow
$wf = '{"Id":"test","States":[{"Id":"S1","Name":"Start","IsInitial":true}],"Actions":[]}'
Invoke-RestMethod -Uri http://localhost:5087/workflows -Method Post -ContentType 'application/json' -Body $wf

# Start instance
$inst = Invoke-RestMethod -Uri http://localhost:5087/workflows/test/instances -Method Post

# Fire an action (if one exists)
Invoke-RestMethod -Uri http://localhost:5087/instances/$($inst.id)/actions/go -Method Post

# Inspect instance
Invoke-RestMethod -Uri http://localhost:5087/instances/$($inst.id)
```

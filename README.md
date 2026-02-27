# 🚀 Hybrid Tool-Calling AI Agent  
### ASP.NET Core + Ollama + Streaming + Extensible Tool Registry

 AI agent built with:

- **ASP.NET Core (.NET 8)**
- **Ollama (local LLM runtime)**
- **Streaming responses (SSE)**
- **Automatic tool selection**
- **Extensible Tool Registry**
- **Hybrid-ready architecture (RAG compatible)**

---

## ✨ Features

- ✅ Streaming responses (`text/event-stream`)
- ✅ Automatic tool detection & execution
- ✅ Structured JSON tool calls
- ✅ Extensible tool registry
- ✅ Clean separation of concerns
- ✅ SQLite (Code First, EF Core)
- ✅ Hybrid RAG-ready architecture

---

# 🏗 Architecture Overview

## 🏗 High-Level Architecture

```mermaid
flowchart LR
    subgraph Client Layer
        A[Web Client / HTTP Client]
    end

    subgraph API Layer
        B[ASP.NET Core API]
        C[ChatController]
    end

    subgraph Agent Layer
        D[AgentService]
        E[ToolRegistry]
        F[Tool Implementations]
    end

    subgraph LLM Layer
        G[Ollama Service]
        H[Local LLM Model]
    end

    A --> B
    B --> C
    C --> D
    D --> G
    G --> H

    D --> E
    E --> F
```

## 🔄 Agent Tool-Calling Execution Flow
```mermaid
sequenceDiagram
    participant Client
    participant Controller
    participant AgentService
    participant LLM
    participant ToolRegistry
    participant Tool

    Client->>Controller: HTTP Request (user message)
    Controller->>AgentService: RunAsync(userMessage)

    AgentService->>LLM: Send system prompt + tools + user message
    LLM-->>AgentService: Tool call JSON OR normal text

    alt Tool requested
        AgentService->>ToolRegistry: ExecuteAsync(toolName, args)
        ToolRegistry->>Tool: Run tool
        Tool-->>ToolRegistry: Tool result
        ToolRegistry-->>AgentService: Tool result

        AgentService->>LLM: Send tool result for final answer
        LLM-->>AgentService: Final response
    else No tool needed
        LLM-->>AgentService: Direct response
    end

    AgentService-->>Controller: Stream response
    Controller-->>Client: SSE stream
```
## 📋 Prerequisites

Before running this project, ensure the following dependencies are installed:

### 1️⃣ .NET SDK

- .NET 8.0 or later  
- Download: https://dotnet.microsoft.com/download

Verify installation:

```bash
dotnet --version
```

---

### 2️⃣ Ollama (Local LLM Runtime)

This project uses **Ollama** to run local LLM models.

Install Ollama:

- macOS / Linux:
```bash
curl -fsSL https://ollama.com/install.sh | sh
```

- Windows:
Download installer from:
https://ollama.com/download

Verify installation:

```bash
ollama --version
```

---

### 3️⃣ Pull a Supported LLM Model

After installing Ollama, pull a model:

Example (recommended):

```bash
ollama pull llama3
```

Verify the model is available:

```bash
ollama list
```

---

### 4️⃣ Start Ollama Server

Ollama must be running before starting the API:

```bash
ollama serve
```

By default, it runs at:

```
http://localhost:11434
```

## ⚠️ Important

If Ollama is not running, the API will fail when attempting to call the LLM.

Ensure:
- Ollama service is active
- The model is downloaded
- The model name matches your configuration

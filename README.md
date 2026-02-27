# 🚀 Hybrid Tool-Calling AI Agent  
### ASP.NET Core + Ollama + Streaming + Extensible Tool Registry

A production-ready AI agent built with:

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

### Request Flow
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

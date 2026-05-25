# ForgeAI — Architecture

> Clean Architecture · Dependency Inversion · Production-Grade · Microservice-Ready

---

## System Overview

ForgeAI is built on **Clean Architecture** — a layered design where dependencies only point inward. The outer layers know about inner layers. The inner layers know nothing about the outside world.

```
┌─────────────────────────────────────────────────────────────────┐
│                        EXTERNAL WORLD                           │
│              (HTTP Clients · Web UI · CLI · Other APIs)         │
└────────────────────────────┬────────────────────────────────────┘
                             │
┌────────────────────────────▼────────────────────────────────────┐
│                        API LAYER                                │
│         Controllers · Auth Middleware · Routing · Swagger       │
│                        (.NET 8)                                 │
└────────────────────────────┬────────────────────────────────────┘
                             │  calls
┌────────────────────────────▼────────────────────────────────────┐
│                    APPLICATION LAYER                            │
│         Use Cases · Command Handlers · Query Handlers           │
│         Service Orchestration · DTOs · Validation               │
└──────────┬────────────────────────────────────┬─────────────────┘
           │  uses                              │  uses
┌──────────▼──────────┐              ┌──────────▼──────────────────┐
│    DOMAIN LAYER     │              │    INFRASTRUCTURE LAYER      │
│                     │              │                              │
│  Entities           │◄─────────────│  PostgreSQL (EF Core)        │
│  Value Objects      │  implements  │  Redis Cache                 │
│  Domain Events      │  interfaces  │  Python AI Service           │
│  Business Rules     │              │  Vector Database             │
│  Interfaces         │              │  File Storage                │
│  (no deps at all)   │              │  External LLM APIs           │
└─────────────────────┘              └──────────────────────────────┘
```

**The Golden Rule:** Arrows point inward. Domain never imports from Infrastructure. Infrastructure implements interfaces defined in Domain.

---

## Layer Breakdown

### 🌐 API Layer — `ForgeAI.API`
The entry point. Thin by design. No business logic lives here.

**Responsibilities:**
- Receive and validate HTTP requests
- Authenticate via JWT middleware
- Map requests → Application commands/queries
- Map results → HTTP responses
- Expose Swagger documentation

**What it must NOT do:**
- Contain business logic
- Talk to the database directly
- Know how AI works internally

```
ForgeAI.API/
├── Controllers/
│   ├── AuthController.cs       # POST /auth/register, /auth/login
│   ├── ChatController.cs       # POST /chat, GET /chat/{id}/history
│   ├── FilesController.cs      # POST /files/upload, GET /files
│   └── AgentController.cs      # POST /agent/run
├── Middleware/
│   ├── JwtMiddleware.cs        # Validate Bearer token on every request
│   └── ErrorHandlingMiddleware.cs  # Global exception → clean error response
└── Program.cs                  # DI registration, pipeline configuration
```

---

### ⚙️ Application Layer — `ForgeAI.Application`
The brain of the system. Orchestrates everything. Contains all use cases.

**Responsibilities:**
- Implement every use case (register user, send message, upload file, run agent)
- Coordinate between Domain and Infrastructure via interfaces
- Define DTOs (what goes in, what comes out)
- Handle cross-cutting: logging, validation, transactions

**What it must NOT do:**
- Know which database is being used
- Import from `ForgeAI.Infrastructure`
- Contain HTTP concepts (no HttpContext here)

```
ForgeAI.Application/
├── UseCases/
│   ├── Auth/
│   │   ├── RegisterUser/
│   │   │   ├── RegisterUserCommand.cs   # Input DTO
│   │   │   ├── RegisterUserHandler.cs   # The actual logic
│   │   │   └── RegisterUserResult.cs    # Output DTO
│   │   └── LoginUser/
│   │       ├── LoginUserCommand.cs
│   │       └── LoginUserHandler.cs
│   ├── Chat/
│   │   ├── SendMessage/
│   │   └── GetHistory/
│   ├── Files/
│   │   └── UploadDocument/
│   └── RAG/
│       └── SearchDocuments/
├── Interfaces/                  # Contracts Infrastructure must implement
│   ├── IUserRepository.cs
│   ├── IChatRepository.cs
│   ├── IAIService.cs            # Python AI service contract
│   ├── IVectorStore.cs
│   └── IFileStorage.cs
└── Common/
    ├── Validators/              # FluentValidation rules
    └── Exceptions/              # Domain-specific exceptions
```

---

### 🏛️ Domain Layer — `ForgeAI.Core`
The heart of the system. Pure C#. Zero external dependencies. This layer never changes because of infrastructure decisions.

**Responsibilities:**
- Define all core entities (User, Conversation, Message, Document, Agent)
- Encode business rules directly on entities
- Define value objects (Email, Password, EmbeddingVector)
- Raise domain events when important things happen

**Dependencies:** None. Zero. Not even EF Core.

```
ForgeAI.Core/
├── Entities/
│   ├── User.cs                  # Id, Email, PasswordHash, Role, CreatedAt
│   ├── Conversation.cs          # Id, UserId, Title, Messages, CreatedAt
│   ├── Message.cs               # Id, ConversationId, Role, Content, Timestamp
│   ├── Document.cs              # Id, UserId, FileName, Status, Chunks
│   └── AgentRun.cs              # Id, Task, Steps[], FinalAnswer, Status
├── ValueObjects/
│   ├── Email.cs                 # Validates format on construction
│   └── MessageRole.cs           # Enum: User | Assistant | System
├── Events/                      # Things that happened (for event-driven later)
│   ├── UserRegisteredEvent.cs
│   ├── MessageSentEvent.cs
│   └── DocumentProcessedEvent.cs
└── Exceptions/
    ├── DomainException.cs        # Base class
    ├── UserNotFoundException.cs
    └── InvalidCredentialsException.cs
```

---

### 🔧 Infrastructure Layer — `ForgeAI.Infrastructure`
The outer shell. Implements all interfaces defined by Application. Swappable.

**Responsibilities:**
- Talk to PostgreSQL (via EF Core)
- Talk to Redis (cache, sessions, job queues)
- Call the Python AI service (via HttpClient)
- Interface with vector database (Qdrant or pgvector)
- Handle file storage (local disk or S3)

**Key rule:** If you swap PostgreSQL for MongoDB, only this layer changes. Nothing else.

```
ForgeAI.Infrastructure/
├── Persistence/
│   ├── ForgeAIDbContext.cs       # EF Core DbContext
│   ├── Migrations/               # EF Core migrations live here
│   └── Repositories/
│       ├── UserRepository.cs     # Implements IUserRepository
│       ├── ChatRepository.cs     # Implements IChatRepository
│       └── DocumentRepository.cs
├── Cache/
│   └── RedisCacheService.cs      # Session tokens, response caching
├── AI/
│   ├── PythonAIService.cs        # HttpClient wrapper → Python FastAPI
│   └── VectorStoreService.cs     # Implements IVectorStore
├── Storage/
│   └── LocalFileStorage.cs       # Implements IFileStorage (swap for S3 later)
└── DependencyInjection.cs        # Register all Infrastructure services into DI
```

---

### 🐍 Python AI Service — `services/ai/`
A separate FastAPI microservice. Handles everything LLM-related. Called by the .NET API internally.

**Why separate?** AI/ML Python ecosystem (LangChain, sentence-transformers, PyTorch) doesn't belong in .NET. Keep each language doing what it's best at.

```
services/ai/
├── routers/
│   ├── chat.py           # POST /ai/chat — LLM completion with streaming
│   ├── rag.py            # POST /rag/embed, POST /rag/search
│   ├── files.py          # POST /files/process — chunk + embed documents
│   └── agent.py          # POST /agent/run — ReAct agent loop
├── core/
│   ├── llm_client.py     # OpenAI / Ollama abstraction
│   ├── embeddings.py     # Generate embeddings (sentence-transformers)
│   ├── chunker.py        # Split documents into chunks
│   └── vector_store.py   # Read/write to vector DB
├── main.py               # FastAPI app, router registration
└── requirements.txt
```

**Internal API contract (called by .NET):**

| Endpoint | Method | What it does |
|---|---|---|
| `/ai/chat` | POST | Send messages array, stream LLM response |
| `/rag/embed` | POST | Generate embedding vector for a text chunk |
| `/rag/search` | POST | Find top-K most similar chunks to a query |
| `/files/process` | POST | Chunk + embed an entire document |
| `/agent/run` | POST | Run a ReAct agent loop with tools |

---

## Data Flow Examples

### User Sends a Chat Message

```
1. POST /chat/{id}/message  →  ChatController
2. ChatController           →  SendMessageCommand (Application)
3. SendMessageHandler       →  IChatRepository.GetConversation()
4. SendMessageHandler       →  IAIService.Chat(messages)
5. IAIService (Python)      →  RAG search → build prompt → call LLM → stream
6. SendMessageHandler       →  IChatRepository.SaveMessage()
7. Stream response          →  back to client
```

### User Uploads a Document

```
1. POST /files/upload       →  FilesController
2. FilesController          →  UploadDocumentCommand (Application)
3. UploadDocumentHandler    →  IFileStorage.Save(file)
4. UploadDocumentHandler    →  IAIService.ProcessDocument(fileId)
5. Python service           →  extract text → chunk → embed → store in vector DB
6. UploadDocumentHandler    →  IDocumentRepository.UpdateStatus("ready")
7. 200 OK                   →  back to client (processing happens async)
```

---

## Database Schema

```sql
-- Core tables (PostgreSQL)

users
  id            UUID PRIMARY KEY
  email         VARCHAR(255) UNIQUE NOT NULL
  password_hash VARCHAR(255) NOT NULL
  role          VARCHAR(50) DEFAULT 'user'
  created_at    TIMESTAMP DEFAULT NOW()

conversations
  id            UUID PRIMARY KEY
  user_id       UUID REFERENCES users(id)
  title         VARCHAR(255)
  created_at    TIMESTAMP DEFAULT NOW()

messages
  id            UUID PRIMARY KEY
  conversation_id UUID REFERENCES conversations(id)
  role          VARCHAR(50)   -- 'user' | 'assistant' | 'system'
  content       TEXT NOT NULL
  created_at    TIMESTAMP DEFAULT NOW()

documents
  id            UUID PRIMARY KEY
  user_id       UUID REFERENCES users(id)
  file_name     VARCHAR(255)
  status        VARCHAR(50)   -- 'uploading' | 'processing' | 'ready' | 'failed'
  created_at    TIMESTAMP DEFAULT NOW()

document_chunks
  id            UUID PRIMARY KEY
  document_id   UUID REFERENCES documents(id)
  content       TEXT
  chunk_index   INTEGER
  -- embedding stored in vector DB, referenced by chunk id
  created_at    TIMESTAMP DEFAULT NOW()

agent_runs
  id            UUID PRIMARY KEY
  user_id       UUID REFERENCES users(id)
  task          TEXT
  steps         JSONB         -- array of {thought, action, observation}
  final_answer  TEXT
  status        VARCHAR(50)
  created_at    TIMESTAMP DEFAULT NOW()
```

---

## Infrastructure Stack

```
┌─────────────────────────────────────────────┐
│              docker-compose.yml             │
│                                             │
│  ┌──────────────┐   ┌────────────────────┐  │
│  │  .NET API    │   │  Python AI Svc     │  │
│  │  :5000       │   │  :8001             │  │
│  └──────┬───────┘   └─────────┬──────────┘  │
│         │                     │             │
│  ┌──────▼───────┐   ┌─────────▼──────────┐  │
│  │  PostgreSQL  │   │  Qdrant            │  │
│  │  :5432       │   │  (Vector DB) :6333 │  │
│  └──────────────┘   └────────────────────┘  │
│                                             │
│  ┌──────────────┐   ┌────────────────────┐  │
│  │  Redis       │   │  Prometheus        │  │
│  │  :6379       │   │  + Grafana         │  │
│  └──────────────┘   └────────────────────┘  │
└─────────────────────────────────────────────┘
```

---

## Key Design Decisions

| Decision | Choice | Why |
|---|---|---|
| Architecture | Clean Architecture | Maintainable, testable, each layer replaceable |
| Backend language | .NET 8 (C#) | Strongly typed, fast, great for APIs |
| AI service language | Python (FastAPI) | Best ML/AI ecosystem — LangChain, transformers |
| Primary database | PostgreSQL | Reliable, supports pgvector for embeddings |
| Cache | Redis | Fast session storage, pub/sub for future use |
| Vector DB | Qdrant | Purpose-built for semantic search at scale |
| Containerization | Docker Compose | Reproducible environments, easy cloud deploy |
| Auth | JWT + Refresh tokens | Stateless, scalable, industry standard |

---

## Dependency Rule — Summary

```
Domain      → depends on: nothing
Application → depends on: Domain
API         → depends on: Application
Infrastructure → depends on: Domain (implements its interfaces)

NEVER:
  Domain → Infrastructure  ❌
  Domain → Application     ❌
  Application → API        ❌
  Application → Infrastructure (directly) ❌
```

If you find yourself importing `Infrastructure` inside `Application` — stop. Define an interface in `Application`, implement it in `Infrastructure`, inject via DI.

---

*This document should be updated every time a major architectural decision is made.*

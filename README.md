<div align="center">

# ⚡ ForgeAI

### Production-Grade AI Workspace Platform

*Built from scratch on Linux · .NET 8 backend · Python AI services · Cloud-native architecture*

[![Status](https://img.shields.io/badge/Status-Active%20Development-brightgreen?style=for-the-badge)](.)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](.)
[![Python](https://img.shields.io/badge/Python-AI%20Services-3776AB?style=for-the-badge&logo=python)](.)
[![Docker](https://img.shields.io/badge/Docker-Containerized-2496ED?style=for-the-badge&logo=docker)](.)
[![Platform](https://img.shields.io/badge/Platform-Linux-FCC624?style=for-the-badge&logo=linux&logoColor=black)](.)

</div>

---

## 🧠 What is ForgeAI?

**ForgeAI** is a production-grade AI Workspace Platform — not a chatbot wrapper, not a tutorial project.

It is a **scalable, backend-powered AI system** built to understand how real AI infrastructure works:
clean architecture, microservices, RAG pipelines, vector search, and cloud-native deployment —
all built and deployed on **Linux** from scratch.

> 💡 The goal is not to finish fast. The goal is to understand every layer deeply —
> from the database schema to the AI pipeline to the Docker network.

---

## 🛠️ Tech Stack

| Layer | Technology | Purpose |
|---|---|---|
| **Backend API** | .NET 8 (C#) | Core REST API, business logic, auth |
| **AI Services** | Python (FastAPI) | LLM integration, embeddings, RAG |
| **Primary DB** | PostgreSQL | Users, chat history, metadata |
| **Cache / Queue** | Redis | Session cache, job queues |
| **Vector DB** | Qdrant / pgvector | Semantic search, embeddings storage |
| **Containerization** | Docker + Compose | Local dev and production deployment |
| **Cloud** | TBD (AWS / VPS) | Production hosting |
| **OS / Platform** | Linux (Ubuntu) | Everything runs and is built on Linux |

---

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                        CLIENT                           │
│                  (API / Web / CLI)                      │
└────────────────────────┬────────────────────────────────┘
                         │ HTTP / REST
┌────────────────────────▼────────────────────────────────┐
│               .NET 8 API GATEWAY                        │
│         Auth · Routing · Rate Limiting                  │
└──────┬──────────────────────────┬───────────────────────┘
       │                          │
┌──────▼──────┐          ┌────────▼────────┐
│  PostgreSQL │          │  Python AI Svc  │
│  (Users,    │          │  (FastAPI)      │
│   History,  │          │  LLM · RAG      │
│   Metadata) │          │  Embeddings     │
└─────────────┘          └────────┬────────┘
                                  │
                    ┌─────────────▼──────────┐
                    │     Vector Database     │
                    │   (Qdrant / pgvector)   │
                    │   Semantic Search       │
                    └────────────────────────┘
       │
┌──────▼──────┐
│    Redis    │
│  Cache ·   │
│  Sessions · │
│  Queues     │
└─────────────┘
```

---

## 🗺️ Build Roadmap

> This is your working checklist. Update it as you go. Each phase builds on the last.

---

### ✅ Phase 0 — Foundation (Do This First)
> Get the skeleton running before writing any features.

- [ ] Set up Linux dev environment (Ubuntu, git, docker, .NET SDK, Python)
- [ ] Create GitHub repo with proper folder structure
- [ ] Write `docker-compose.yml` that starts PostgreSQL + Redis
- [ ] Verify both databases are reachable from your machine
- [ ] Create `.env.example` file with all required env variables
- [ ] Write a basic health-check endpoint in .NET (`GET /health → 200 OK`)
- [ ] Commit and push — **your first working state**

**You know Phase 0 is done when:** `docker compose up` starts everything and `/health` returns 200.

---

### 🔐 Phase 1 — Authentication System
> Before any AI feature, users need to exist and be authenticated.

- [ ] Design `Users` table in PostgreSQL (id, email, password_hash, created_at, role)
- [ ] Write EF Core migration and apply it
- [ ] `POST /auth/register` — create user, hash password (BCrypt)
- [ ] `POST /auth/login` — validate credentials, return JWT
- [ ] `GET /auth/me` — protected route, returns current user from JWT
- [ ] Middleware: validate JWT on protected routes
- [ ] Store refresh tokens in Redis
- [ ] Write unit tests for auth logic

**You know Phase 1 is done when:** you can register, log in, get a JWT, and hit a protected route.

---

### 💬 Phase 2 — AI Chat System
> First real AI feature. Connect .NET to an LLM via the Python service.

- [ ] Set up Python FastAPI service (`/services/ai/`)
- [ ] Connect Python service to an LLM (OpenAI API or Ollama for local)
- [ ] `POST /ai/chat` in Python — takes messages array, returns streaming response
- [ ] .NET calls Python service internally (HttpClient)
- [ ] Design `Conversations` and `Messages` tables in PostgreSQL
- [ ] `POST /chat` — create a new conversation
- [ ] `POST /chat/{id}/message` — send message, stream LLM response, save to DB
- [ ] `GET /chat/{id}/history` — return full conversation history
- [ ] Wire streaming responses end-to-end (.NET → Python → client)
- [ ] Add both services to `docker-compose.yml`

**You know Phase 2 is done when:** you can have a full conversation that persists in the database.

---

### 📄 Phase 3 — File Processing Pipeline
> Ingest documents so the RAG system has something to search.

- [ ] `POST /files/upload` — accept PDF, TXT files
- [ ] Save raw file to disk or object storage
- [ ] Python service: extract text from PDF (`pypdf2` or `pdfplumber`)
- [ ] Python service: chunk text into pieces (e.g. 512 tokens with overlap)
- [ ] Store chunks in PostgreSQL (`DocumentChunks` table)
- [ ] Use Redis queue (`Celery` or background job) so upload returns fast
- [ ] `GET /files` — list uploaded files
- [ ] `DELETE /files/{id}` — remove file and its chunks

**You know Phase 3 is done when:** you can upload a PDF and see its chunks stored in the DB.

---

### 🔍 Phase 4 — RAG System (Retrieval-Augmented Generation)
> The hardest and most important AI feature. Makes the chat actually smart.

- [ ] Set up vector database (start with **pgvector** extension in PostgreSQL — simpler)
- [ ] Python service: generate embeddings for each chunk (`sentence-transformers` or OpenAI)
- [ ] Store embeddings in vector DB alongside chunk text
- [ ] Python service: `POST /rag/search` — embed the user query, find top-K similar chunks
- [ ] Build RAG prompt: `[context from chunks] + [user question]`
- [ ] Wire into chat: before calling LLM, retrieve relevant chunks and inject into prompt
- [ ] `GET /rag/sources` — return which document chunks were used in a response
- [ ] Test: upload a document, ask a question about it, get a grounded answer

**You know Phase 4 is done when:** the AI answers questions using your uploaded documents.

---

### 🤖 Phase 5 — AI Agents
> Give the AI the ability to take actions, not just answer questions.

- [ ] Research: understand ReAct pattern (Reason + Act loop)
- [ ] Define tools the agent can use (web search, file read, calculator)
- [ ] Python service: implement basic agent loop with tool calling
- [ ] `POST /agent/run` — send a task, get back a result with reasoning steps
- [ ] Store agent runs in DB (task, steps taken, final answer)
- [ ] Add tool: search uploaded documents (calls RAG internally)

**You know Phase 5 is done when:** you can give the agent a task and it uses tools to solve it.

---

### 📊 Phase 6 — Monitoring & Observability
> You can't call it production-grade without knowing what's happening inside.

- [ ] Add structured logging to .NET (Serilog → writes JSON logs)
- [ ] Add structured logging to Python (structlog)
- [ ] Set up **Prometheus** to scrape metrics from both services
- [ ] Set up **Grafana** dashboard — requests/sec, error rate, response time
- [ ] Add health check endpoints for all services
- [ ] Set up alerts (e.g. error rate > 5% sends a notification)
- [ ] Add all monitoring services to `docker-compose.yml`

**You know Phase 6 is done when:** you can open Grafana and see live traffic from your API.

---

### 🚀 Phase 7 — CI/CD & Cloud Deployment
> Ship it. Make it run on a real server, not just your VM.

- [ ] Write `Dockerfile` for .NET service
- [ ] Write `Dockerfile` for Python service
- [ ] Set up GitHub Actions: on push → run tests → build Docker images
- [ ] Get a Linux VPS (DigitalOcean, Hetzner, or AWS EC2 — ~$5-6/month)
- [ ] SSH into VPS, install Docker
- [ ] Deploy with `docker compose up -d` on the VPS
- [ ] Set up Nginx as reverse proxy with HTTPS (Let's Encrypt)
- [ ] Point a domain at your server
- [ ] Set up automatic deploys: push to `main` → GitHub Actions → deploy to VPS

**You know Phase 7 is done when:** ForgeAI is live on the internet, accessible by URL, with HTTPS.

---

## 📁 Project Structure

```
ForgeAI/
│
├── src/
│   ├── ForgeAI.API/            # .NET 8 — main API gateway
│   │   ├── Controllers/        # Route handlers
│   │   ├── Middleware/         # JWT auth, error handling
│   │   ├── Services/           # Business logic
│   │   └── Program.cs
│   │
│   ├── ForgeAI.Core/           # .NET — domain models, interfaces
│   ├── ForgeAI.Infrastructure/ # .NET — DB, Redis, external calls
│   └── ForgeAI.Tests/          # Unit + integration tests
│
├── services/
│   └── ai/                     # Python FastAPI AI service
│       ├── routers/            # chat.py, rag.py, agent.py
│       ├── core/               # LLM client, embeddings, chunking
│       ├── main.py
│       └── requirements.txt
│
├── infra/
│   ├── docker-compose.yml      # Full local dev stack
│   ├── docker-compose.prod.yml # Production overrides
│   ├── nginx/                  # Nginx reverse proxy config
│   └── prometheus/             # Metrics scrape config
│
├── migrations/                 # EF Core database migrations
├── docs/                       # Architecture decisions, notes
├── .env.example                # All required env variables
├── .gitignore
└── README.md
```

---

## ⚡ Quick Start (Local Dev)

```bash
# 1. Clone
git clone https://github.com/sangambaral17/ForgeAI.git
cd ForgeAI

# 2. Set up environment
cp .env.example .env
# Edit .env and fill in your values (DB password, JWT secret, API keys)

# 3. Start infrastructure
docker compose up -d postgres redis

# 4. Run database migrations
cd src/ForgeAI.API
dotnet ef database update

# 5. Start .NET API
dotnet run

# 6. Start Python AI service (separate terminal)
cd services/ai
pip install -r requirements.txt
uvicorn main:app --reload --port 8001

# 7. Verify everything works
curl http://localhost:5000/health
```

---

## 🌍 Environment Variables

```bash
# .env.example

# Database
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_DB=forgeai
POSTGRES_USER=forgeai
POSTGRES_PASSWORD=changeme

# Redis
REDIS_URL=redis://localhost:6379

# JWT
JWT_SECRET=your-super-secret-key-change-this
JWT_EXPIRY_HOURS=24

# AI
OPENAI_API_KEY=sk-...          # or leave blank if using Ollama locally
OLLAMA_BASE_URL=http://localhost:11434

# Python AI Service
AI_SERVICE_URL=http://localhost:8001

# Vector DB
VECTOR_DB_URL=http://localhost:6333  # if using Qdrant
```

---

## 🏛️ Engineering Principles

```
Clean Architecture      →  Core has no external dependencies
Modular Design          →  Each service can be replaced independently  
Production-First        →  Built like it will be deployed, from day one
Security-First          →  Never store plaintext passwords, always validate input
Linux-Native            →  Developed, tested, and deployed on Linux
Observability-Ready     →  Logs and metrics from the start, not added later
```

---

## 📖 Learning Goals

This project exists to prove and develop:

| Skill | How ForgeAI Builds It |
|---|---|
| **Backend Engineering** | .NET 8 Clean Architecture, REST API design |
| **AI/ML Integration** | RAG pipelines, embeddings, LLM orchestration |
| **Linux Proficiency** | Built and run entirely on Linux |
| **DevOps** | Docker, CI/CD, cloud deployment, Nginx |
| **Database Design** | PostgreSQL schema design, migrations, vector search |
| **System Design** | Microservices, async processing, scalable architecture |

---

## 📜 License

MIT License — see [LICENSE](LICENSE) for details.

---

<div align="center">

**Built on Linux · .NET 8 + Python · Engineered for Production**

*ForgeAI — one commit at a time.*

⭐ If you're building something similar, star the repo

</div>

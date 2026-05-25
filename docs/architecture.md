# ForgeAI Architecture

## Overview

ForgeAI is a production-grade AI platform designed using Clean Architecture principles.

---

## System Design

Frontend → API → Application → Domain → Infrastructure

---

## Layers

### API Layer
- Handles HTTP requests
- Authentication
- Routing
- Swagger

### Application Layer
- Business use cases
- Orchestration logic
- Service coordination

### Domain Layer
- Core business entities
- Business rules
- Pure logic (no dependencies)

### Infrastructure Layer
- Database (PostgreSQL)
- Redis caching
- External APIs (AI services, LLMs)
- File storage

---

## Key Principles

- Dependency flows inward
- Domain has no external dependencies
- Infrastructure is replaceable
- API is thin
- Application contains use cases

---

## Goal

Build a scalable AI SaaS platform with:
- RAG pipelines
- AI agents
- Cloud deployment
- Microservice-ready architecture
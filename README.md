### SupportDesk

SupportDesk is a web-based multi-tenant customer support platform that allows organizations to manage support tickets, customers, support agents, categories, template answers, and knowledge base content with real-time collaboration.

This implementation is the product of the previously planned architectural refactor, transitioning the codebase into Clean Architecture with strong separation of concerns.

---

### Architecture and Design Patterns

* **Clean Architecture & DDD**: Clear separation across Domain, Application, Infrastructure, and WebApi layers. Core domain concepts are encapsulated using strongly typed Value Objects with self-validation rules and domain entities.
* **CQRS (Command Query Responsibility Segregation)**: Distinct pipelines and dispatchers for commands and queries with dedicated handlers and validation behaviors.
* **Event-Driven Real-time Updates**: Domain events and real-time push notifications published through RabbitMQ message broker and delivered to connected clients via SignalR hubs.
* **Multi-Tenancy & Tenant Isolation**: Context-based organization scoping and permission checks enforced across queries, commands, and real-time message streams.
* **Single-Module Modular Structure**: Feature-based vertical organization of application models, events, and query contracts.

---

### Tech Stack

* **Backend**: ASP.NET Core (.NET), Entity Framework Core
* **Database & Messaging**: PostgreSQL, RabbitMQ
* **Frontend**: Vue.js, TypeScript, Pinia, Vue Router, Tailwind CSS
* **Real-time**: SignalR, RabbitMQ Fanout Exchange backplane
* **Containerization**: Docker, Docker Compose

---

### Getting Started

#### Requirements

* Docker & Docker Compose
* .NET SDK
* PostgreSQL
* Bun or Node.js

#### Setup

1. Clone the repository:
```bash
git clone https://github.com/veljkotosic/SupportDesk.git
cd SupportDesk
```

2. Configure environment variables in `Aplikacija/.env` as indicated in `Aplikacija/.env.example`.

3. Start infrastructure dependencies (PostgreSQL, RabbitMQ) using Docker Compose:
```bash
cd docker
docker compose up -d
```

4. Run the backend Web API:
```bash
cd ../Aplikacija/dotnet/SupportDesk/SupportDesk.WebApi
dotnet run
```

5. Install frontend dependencies and run the development server:
```bash
cd ../../../front
bun install
bun run dev
```

---

### Planned Improvements

The next architectural iteration will decompose the system into a true modular monolith with strictly isolated module boundaries, independent domain models, and encapsulated databases/schemas, moving beyond this initial single-module implementation.
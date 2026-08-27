# SupportDesk

SupportDesk is a web-based customer support platform that allows organizations to manage support tickets, customers, support agents, categories, and knowledge-base content.

## Getting Started

### Requirements

* Docker
* .NET SDK
* PostgreSQL
* Visual Studio / JetBrains Rider
* Bun

### Setup

1. Clone the repository:

```bash
git clone https://github.com/veljkotosic/SupportDesk.git
cd SupportDesk
```

2. Configure Environment by adding `.env` file to:

```text
Aplikacija/.env
```

  as per `Aplikacija/.env.example`.

3. Install frontend dependencies:

```bash
cd frontend
bun install
```

4. Start the required services with Docker Compose:

```bash
cd docker
docker compose up 
```

5. Start the backend from the .NET project.

6. Start the Vue development server:

```bash
bun run dev
```

7. Open the frontend URL provided by Vite.

## Tech Stack

* ASP.NET Core
* Entity Framework Core
* PostgreSQL
* Vue.js
* TypeScript
* Pinia
* SignalR

## Planned Improvements

The project is planned to be refactored to improve its overall architecture, separation of concerns, and maintainability, including adopting **Clean Architecture** and applying additional clean-code principles.

# PPSR Batch Registration

This project implements a batch motor vehicle registration uploader for PPSR using a DDD-style .NET 9 Web API and a modern Next.js frontend. It is designed for the InfoTrack tech challenge.

---

## Features
- Upload CSV file via UI
- Backend validates, deduplicates, and stores records
- Idempotent updates if the same VIN + SPG + Grantor exists
- Enforces: one vehicle per owner
- Duplicate file detection by file hash
- Summary UI on completion
- Full test coverage for backend and E2E UI tests

---

## Tech Stack
| Layer        | Tech                                         |
|--------------|-----------------------------------------------|
| Frontend     | Next.js 14 (App Router) + Tailwind CSS       |
| Backend      | .NET 9 (DDD structure, Web API)              |
| Storage      | EF Core InMemory (demo only)                 |
| Container    | Docker, Docker Compose                       |
| Unit Testing | xUnit + FluentAssertions (C#)                |
| UI Testing   | Jest + React Testing Library (TSX)           |
| E2E Testing  | Playwright (upload form automation)          |

---

## Local Dev Setup (Optional)
> You can run frontend and backend locally without Docker if needed:

```bash
# Backend
cd registration-api/src/Registration.Api
dotnet run

# Frontend
cd registration-web
pnpm install
pnpm run dev
```

Then access:
- Frontend: http://localhost:3000
- Backend Swagger: http://localhost:5000/swagger

---

## Run with Docker Compose

```bash
# Build and run frontend + backend
docker-compose up --build
```

- Frontend: [http://localhost:3000](http://localhost:3000)
- Backend:  [http://localhost:5001/swagger](http://localhost:5001/swagger)

---

## Project Structure
```
registration-api/
├── src/
│   ├── Registration.Api/           # Controllers, DI, Logging
│   ├── Registration.Application/   # DTOs, Services, Validators
│   ├── Registration.Domain/        # Entities, Value Objects, Interfaces
│   ├── Registration.Infrastructure/ # EF Core, Repositories
├── tests/                          # xUnit test project

registration-web/
├── app/                            # Next.js App Router UI
├── __tests__/                      # Jest unit tests
├── e2e/                            # Playwright E2E tests
├── Dockerfile                      # Frontend container

docker-compose.yml
```

---

## Run Tests

### ✅ Backend Unit Tests
```bash
cd registration-api
dotnet test
```

### ✅ Frontend Unit Tests
```bash
cd registration-web
pnpm test
```

### ✅ Frontend E2E Tests
```bash
pnpm exec playwright test
```

---

## Notes
- Max CSV file size: 25MB
- Duplicate files detected via SHA256 file hash
- Each vehicle may only have one Grantor (owner)
- Upload requires at least 1 valid data row (plus headers)
- If Grantor+VIN+SPG match, record is updated (not duplicated)
- Error handling with proper HTTP status codes + ProblemDetails
- Logging included (Info, Warning, Error)

---

## Upload Scenarios

### Standard File Upload
The standard file upload process is designed for smaller CSV files (up to 25MB) and processes them synchronously. This is ideal for:
- Quick processing of small to medium-sized files
- Immediate feedback on upload results
- Real-time validation and error reporting

The upload endpoint returns a summary containing:
- Number of submitted records
- Number of processed records
- Number of added records
- Number of updated records
- Number of invalid records

### Big File Upload
The big file upload process is designed for larger CSV files and processes them asynchronously in the background. This is ideal for:
- Processing large datasets (up to 25MB)
- Long-running uploads that shouldn't block the user interface
- Batch processing of multiple files

The process works as follows:
1. File is uploaded and queued for processing
2. A task ID is returned immediately
3. The file is processed in the background
4. Progress can be monitored using the task ID
5. Results are available when processing is complete

The status endpoint returns:
- Current processing status
- Progress metrics (submitted, processed, added, updated, invalid records)
- Completion time or error message if processing failed

---

For any questions, feel free to reach out. Thanks for reviewing!

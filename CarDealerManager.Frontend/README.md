# Car Dealer Manager frontend

Angular frontend for the Candidate Financial Evaluation workflow.

## Local development

1. Start PostgreSQL and the Web API on `http://localhost:5250`.
2. Use Node.js 20.19+, 22.12+, or 24+, then run `pnpm install`.
3. From this directory, run `pnpm start` so the project-local Angular CLI is used.
4. Open `http://localhost:4200`.

The Angular development server proxies `/api` to the Web API using `proxy.conf.json`, so no development CORS change is required.

## Commands

- `pnpm start` — development server
- `pnpm build` — production build
- `pnpm test` — unit tests (single run)
- `pnpm test:watch` — unit tests in watch mode

Financial calculations are intentionally not implemented in Angular. The application displays the calculation results returned by `GET /api/vehicles/{id}`.

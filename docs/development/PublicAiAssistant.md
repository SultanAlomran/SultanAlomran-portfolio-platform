# Public Portfolio Assistant

## Purpose and scope

Portfolio Assistant is a public, read-only discovery experience for projects, technologies, the Visual Handbook, and approved professional-profile facts. It is not an admin bot, SQL console, mutation workflow, or unrestricted general-purpose chatbot.

## Architecture and data flow

`Portfolio.Web → POST /api/assistant/messages → PortfolioAssistantService → approved IProjectsService/IInfographicsService queries → EF Core → SQL Server`. The bounded evidence plus the visitor's message and at most eight recent messages are passed to `IAiAssistantClient`. Credentials, EF entities, connection strings, admin DTOs, telemetry, and private data never enter model context.

The first provider is `DeterministicAiAssistantClient`, a credential-free provider intended for development, preview, and CI. It makes the feature testable without network access or usage cost. A production provider can replace this one through DI; provider SDK calls must remain behind `IAiAssistantClient`. No paid provider or Azure AI resource is provisioned by this slice.

## Public-data audit

**Database-backed:** Projects and technologies; published Infographics, categories, tags, steps, resources, code examples, series relationships, and public media projections. Existing public query services enforce published status. Media Library metadata, users, authorization data, audit data, and Test Analytics exist but are not assistant tools.

**Static Web data:** profile proof points, experience, skills, certifications, professional development, and technical-series marketing copy. A deliberately bounded server-side public profile statement mirrors the approved home-page facts; static content was not migrated into SQL.

**Not yet implemented:** a standalone public education API (approved professional-development facts are available through the bounded assistant profile capability), persistent conversations, semantic/vector search, assistant analytics, and a real hosted model provider.

## Approved tool catalog and safety

The orchestrator provides bounded public project search (technology-aware), dedicated published Project detail retrieval, published Infographic search, and dedicated published Infographic detail retrieval. Searches return at most five projected results; detail text, steps, tags, and technologies are separately bounded. It returns internal navigation actions only (`/projects/{slug}` and `/visual-handbook/{slug}`).

There is no generic query executor, DbContext tool, raw SQL, reflection-based filtering, or write tool. The model cannot directly access the database. Draft/archive/admin records are excluded by the existing public services. Database/tool content and user input are untrusted reference data; they cannot add tools, override policy, reveal instructions, or expand authorization.

## Limits, cost and privacy

Configuration lives in `AiAssistant`: `Enabled`, `Provider`, `Model`, message/history/tool/output bounds, and timeout. Defaults disable the feature safely; Development enables the deterministic provider. No secret is committed. A future real provider must use a server-side environment variable/user secret/Azure application setting, never Angular configuration.

Requests are limited per server-observed IP to 10 per minute and return HTTP 429. Messages are capped at 1,000 characters, history at eight messages, and query results at five per content type. The configured maximum tool rounds is four for a future function-calling provider. Current cost is normal API/database hosting only. Future cost drivers are model input/output tokens, request volume, tool rounds, retained context, and provider pricing.

Prompts and complete responses are not logged. Structured logs record evidence counts. Permanent chat and assistant analytics are intentionally deferred.

## API and UX

`POST /api/assistant/messages` accepts `{ message, conversationContext? }` and returns `{ message, sources, actions }`. The endpoint is documented by existing OpenAPI/Scalar in Development and Preview. It returns validation problems, 429 for throttling, and a safe 503 while disabled.

Portfolio.Web provides an accessible floating launcher, responsive dialog/full-height mobile sheet, starter prompts, loading/error/retry states, bounded in-browser conversation state, clear action, safe Angular text rendering, and RouterLink source cards. Model-produced HTML is never rendered. Request/response is deliberately non-streaming for the first slice.

## Local, Preview and production configuration

Run the existing API and Web workflows. Development uses the deterministic provider. Preview/Production remain disabled unless `AiAssistant__Enabled=true` is explicitly configured. Azure Preview needs no new resource or secret for deterministic mode. Do not enable a paid provider until its model, region, pricing, credential handling, retention, networking, and production/preview budgets are approved.

## Testing and known limitations

Unit tests substitute scripted `IAiAssistantClient` and tool implementations; they never call a paid API. Security cases cover secrets/system prompts, SQL, unpublished/admin records, and mutation—all are refused and no capable backend tool exists. Playwright coverage uses deterministic route responses for launcher, starters, grounded source rendering, clearing, errors, keyboard opening, 375/430 mobile sheets, and an optional recorded journey. Existing Test Analytics remains unchanged and the Playwright feature selector is `assistant`.

The deterministic provider produces grounded discovery/profile answers rather than broad generative explanations. A production hosted provider remains a follow-up requiring explicit approval. Vector storage, Redis, permanent chat history, writes, and autonomous/MCP infrastructure are intentionally not included.

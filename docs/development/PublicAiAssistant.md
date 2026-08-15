# Public Portfolio Assistant

## Purpose and scope

Portfolio Assistant is a public, read-only discovery experience for projects, technologies, the Visual Handbook, and approved professional-profile facts. It is not an admin bot, SQL console, mutation workflow, or unrestricted general-purpose chatbot.

## Architecture and data flow

`Portfolio.Web â†’ POST /api/assistant/messages â†’ PortfolioAssistantService â†’ approved IProjectsService/IInfographicsService queries â†’ EF Core â†’ SQL Server`. The bounded evidence plus the visitor's message and at most eight recent messages are passed to `IAiAssistantClient`. Credentials, EF entities, connection strings, admin DTOs, telemetry, and private data never enter model context.

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

Unit tests substitute scripted `IAiAssistantClient` and tool implementations; they never call a paid API. Security cases cover secrets/system prompts, SQL, unpublished/admin records, and mutationâ€”all are refused and no capable backend tool exists. Playwright coverage uses deterministic route responses for launcher, starters, grounded source rendering, clearing, errors, keyboard opening, 375/430 mobile sheets, and an optional recorded journey. Existing Test Analytics remains unchanged and the Playwright feature selector is `assistant`.

The deterministic provider produces grounded discovery/profile answers rather than broad generative explanations. A production hosted provider remains a follow-up requiring explicit approval. Vector storage, Redis, permanent chat history, writes, and autonomous/MCP infrastructure are intentionally not included.

## Version 2: hosted reasoning with controlled tools

V2 reuses the V1 endpoint, application orchestrator, `IAiAssistantClient` boundary, deterministic provider, and published project/infographic services. It adds an optional OpenAI Responses API implementation in Infrastructure. The default remains disabled in shared configuration; Development, CI, Playwright, and PR Preview continue to select `Deterministic` and never make paid calls. Production activation is a separate owner decision.

### Provider and configuration

The selected hosted provider is OpenAI, model `gpt-5.6-luna`, called server-side over the Responses API. No vendor type crosses into Application. The implementation uses the .NET platform `HttpClient` and `System.Text.Json`; no vendor SDK package is required. It sends `store: false`. Configure through environment variables or user secrets only:

- `AiAssistant__Enabled=true`
- `AiAssistant__Provider=OpenAI`
- `AiAssistant__Model=gpt-5.6-luna`
- `AiAssistant__RealProviderEnabled=true`
- `AI_ASSISTANT_API_KEY=<secret>`

Additional typed bounds are `MaxUserMessageLength`, `MaxHistoryMessages`, `MaxToolRounds`, `MaxOutputTokens`, `MaxOutputCharacters`, `RequestTimeoutSeconds`, `Temperature`, `RateLimitPermitCount`, and `RateLimitWindowSeconds`. A configured OpenAI provider never silently falls back to deterministic behavior. Missing/invalid credentials, timeouts, provider limits/5xx, malformed responses, invalid tools, and tool failures return a generic unavailable response.

OpenAI API use is token-priced (input, cached input, and output). Current pricing must be confirmed before activation. By default, API content is not used to train OpenAI models unless the organization opts in; default abuse-monitoring retention may apply. Owners must review the current OpenAI data controls, region, retention, budget, and legal requirements before Production activation. Only bounded approved public portfolio projections are sent—never credentials, connection strings, admin data, DbContext objects, storage paths, private CV fields, or test analytics.

### Structured tool calling

The orchestrator supplies a finite catalog and validates every requested function before executing it. The catalog is: `search_projects`, `get_project_details`, `search_infographics`, `get_infographic_details`, `search_technologies`, `get_portfolio_profile`, `get_experience`, `get_certifications`, `get_education_and_professional_development`, `get_contact_options`, `compare_projects`, and `find_related_content`.

Searches return at most five public records per call. Detail tools return bounded public projections including case-study or guide data and safe public routes. Existing public service filters exclude Draft records. There is no SQL, arbitrary LINQ, DbContext, write, admin, shell, code-execution, or filesystem tool. Duplicate identical calls and unsupported calls fail closed; rounds are limited to 1–5.

### Context, language, grounding, and security

The client returns only the configured last messages (eight by default); nothing is persisted to SQL. Tool results are bounded and may provide lightweight selected-entity context through public slugs, without retaining full database payloads. The provider is instructed to clarify material ambiguity, answer Arabic prompts naturally in Arabic, and keep technical identifiers as appropriate. The UI applies RTL per Arabic message, keeps code LTR, renders grounded sources/actions, and shows at most three follow-ups.

Retrieved project and guide text is explicitly untrusted data. Backend capability boundaries—not prompt wording—make connection strings, private/admin/Draft records, arbitrary SQL, writes, system-prompt disclosure, and unsafe URL schemes inaccessible. Sources are restricted to public routes. Actions are allow-listed and external URLs are limited to exact approved HTTPS GitHub/LinkedIn destinations.

### Reliability, observability, and evaluation

Caller cancellation and the configured linked timeout cover provider and tool work. Requests log provider/model, duration, tool count/names/rounds, success/failure category, and reported token usage without logging full prompts or answers. The existing fixed-window IP limiter remains 10 requests per minute by default and is configuration-driven.

The deterministic evaluation dataset contains 60 cases across project/guide retrieval, profile, certifications, education, recruiter questions, explanations, ambiguity, Arabic, multi-turn, unsupported claims, prompt injection, private/admin requests, empty results, and tool failures. Normal CI and Playwright never call OpenAI. Generative manual evaluation should assess factual accuracy, relevance, clarification quality, latency, and grounding; it must not rely on exact-string matching alone.

### Preview, Production, and future V3

Azure PR Preview requires no new resource. Leave the assistant deterministic/disabled for zero AI cost. To test the hosted provider manually later, add `AI_ASSISTANT_API_KEY` and the explicit `AiAssistant__...` switches to the protected Preview environment; never expose the value or enable it for automated traffic. No Production deployment or credential activation is part of V2.

True token streaming is deferred: adding a second streaming transport through the current minimal endpoint and Angular client materially expands failure/cancellation/tool-loop semantics. V2 prioritizes correct grounded tool use. Embeddings and vector databases are also intentionally deferred to V3, and should be considered only if evaluation demonstrates a measurable retrieval gap that structured metadata cannot solve.

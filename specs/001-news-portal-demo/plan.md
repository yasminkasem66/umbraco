# Implementation Plan: News Portal Demo

**Branch**: `001-news-portal-demo` | **Date**: 2026-09-19 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-news-portal-demo/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Build a headless News portal demo on the existing Umbraco 18.1.1 (.NET 10) project: a News module
(categories + articles, browsable/filterable/searchable, with admin-curated featured articles), a
Site Settings singleton for header/footer chrome, an About Us page, and a Contact Us page with a
static-info read side plus a working submission endpoint — all served to an external Angular SPA
through Umbraco's Content Delivery API, with exactly one custom ASP.NET Core Web API controller
(`POST /api/contact`) for the one write operation the Delivery API cannot serve. Content
architecture (languages, Document Types, Data Types, starter content) is created through
idempotent, code-first composers rather than manual backoffice-only steps, and bilingual content
(English default, Arabic) uses native Umbraco culture variants throughout.

## Technical Context

**Language/Version**: C# on .NET 10 (`net10.0`)

**Primary Dependencies**: Umbraco.Cms 18.1.1 (including `Umbraco.Cms.Api.Delivery` for the Content
Delivery API and `Umbraco.Cms.Core`'s `IContentIndexHandler` extension point), ASP.NET Core Web
API (built-in, for the single custom controller) — no new NuGet packages added (see research.md §4
on why Umbraco Forms was evaluated and rejected)

**Storage**: SQL Server LocalDB (`umbracoPOC.mdf`, existing connection string). All content —
News, Site Settings, About Us, Contact Us, and Contact Submissions — is stored as native Umbraco
content nodes; no separate database/table is introduced.

**Testing**: Manual verification via a `.http` file / `curl` against the running app, exercising
every read endpoint and the contact POST (research.md §7) — matches the project brief's own
verification phase; no automated test project exists in the repo and none is added by this
feature.

**Target Platform**: ASP.NET Core web server (Kestrel) on Windows for local/demo hosting; consumed
by an Angular SPA at `http://localhost:4200` running as a separate process/repo (CORS already
configured in `Program.cs`).

**Project Type**: Single backend project (this repo). There is no frontend code in this repo — the
Angular SPA is an external consumer, out of scope to build or modify.

**Performance Goals**: None specified by the spec beyond the qualitative Success Criteria (spec.md
§"Success Criteria"); no numeric throughput/latency target — demo/local-hosting scale.

**Constraints**: No Razor view/template may be assigned to any Document Type in this feature
(Constitution Principle I); all public reads go through the Delivery API, with the one named
write-operation exception (Constitution Principle II); `PublicAccess=true` today with no API-key
layer in front of the Delivery API, and this feature does not add one (spec Assumptions — no
abuse protection on the contact endpoint either).

**Scale/Scope**: Demo scale — 3+ News Categories, 8-10 News Articles (mixed languages, several
featured), one Site Settings node, one About Us node, one Contact Us node, seeded in both English
and Arabic. Not sized for production traffic.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle / constraint | Status | How this plan satisfies it |
|---|---|---|
| I. Headless-Only Delivery (NON-NEGOTIABLE) | PASS | No new Document Type in data-model.md has a template assigned; the pre-existing scratch types/views that would have violated this are removed (research.md §6). |
| II. Delivery API First, Web API as Exception | PASS | Every read (User Stories 1-6) is served by the Content Delivery API (contracts/delivery-api-reads.md); the sole write (User Story 7) uses the one sanctioned custom-controller exception (contracts/contact-submission.md). |
| III. Code-First Content Architecture | PASS | Languages, Document Types, Data Types, and starter content are all created via idempotent composers (research.md §1), not manual-only backoffice steps. |
| IV. Native Bilingual Variants | PASS | English/Arabic registered via `ILanguageService` (research.md §3); every content type is culture-variant (data-model.md) rather than using parallel per-language properties. |
| Technology Constraints — platform baseline, Delivery API enabled | PASS | Uses the already-installed Umbraco.Cms 18.1.1 / net10.0 baseline and the already-enabled Delivery API config; no version change requested or needed. |
| Development Workflow — compliance check before "done" | Carried into quickstart.md §8 | Quickstart's final step is an explicit spot-check against Principle I and the scratch-content cleanup. |

No violations requiring justification — the Complexity Tracking table below is intentionally
empty.

## Project Structure

### Documentation (this feature)

```text
specs/001-news-portal-demo/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md         # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
│   ├── delivery-api-reads.md
│   └── contact-submission.md
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

This feature lives entirely in the existing single ASP.NET Core project at the repo root — there
is no separate frontend or additional backend project to create.

```text
Program.cs                              # existing: AddDeliveryApi(), AddComposers(), CORS, +ApiBehaviorOptions for ContactController
Composers/
├── SharedServicesComposer.cs           # DI registration for the four helpers below
├── ContentTypeHelpers.cs               # shared get-or-create helpers for Data Types/Document Types (not in the original plan — added once composer files started repeating the same boilerplate)
├── BlockListValueBuilder.cs            # builds raw Block List storage JSON from alias/value pairs (research.md §2 Block List pitfall)
├── MediaSeedingHelper.cs               # creates placeholder image Media items for mandatory Media Picker properties
├── LanguageSeedingComposer.cs          # LanguageSeedingInstaller: idempotent EN(default)/AR language registration
├── NewsContentTypesComposer.cs         # NewsContentTypesInstaller: News, News Category, News Article Document Types
├── SiteChromeContentTypesComposer.cs   # SiteChromeContentTypesInstaller: Site Root, Site Settings Document Types
├── AboutUsContentTypesComposer.cs      # AboutUsContentTypesInstaller: About Us Document Type
├── ContactUsContentTypesComposer.cs    # ContactUsContentTypesInstaller: Contact Us, Contact Submissions container, Contact Submission Document Types
├── StarterContentSeedingComposer.cs    # the ONLY UmbracoApplicationStartingNotification handler: calls every installer above in dependency order, then seeds content (categories, articles, singletons) in EN + AR
└── ScratchContentCleanupComposer.cs    # removes the pre-existing scratch Document Types/content (research.md §6) — code-driven, not manual backoffice clicks
DeliveryApi/
└── IndexHandlers/
    ├── NewsArticleIndexHandler.cs      # IContentIndexHandler for category/isFeatured/featuredOrder/tags
    └── NewsArticleQueryHandlers.cs     # IFilterHandler/ISortHandler for the same fields — required in addition to IContentIndexHandler (research.md §2)
Controllers/
└── ContactController.cs                # POST /api/contact
Models/
└── Contact/
    ├── ContactSubmissionRequest.cs     # request DTO + validation attributes
    └── ContactSubmissionResponse.cs    # success/validation-error response shapes
Views/                                   # existing folder; scratch .cshtml files removed (research.md §6)
umbraco/                                 # Umbraco's own backoffice app — unchanged
wwwroot/                                 # unchanged
```

Deviations from the structure originally planned above, both made during implementation once real
constraints surfaced: (1) the four shared helper files under `Composers/` were not anticipated —
without them, near-identical Data Type/Document Type creation boilerplate would have been repeated
across every composer; (2) `NewsContentTypesInstaller` etc. are plain injectable classes rather
than each being its own `UmbracoApplicationStartingNotification` handler, because Umbraco does not
guarantee handler execution order and `StarterContentSeedingComposer`'s seeding needs every content
type to already exist — a single orchestrating handler removes that ordering risk entirely.

**Structure Decision**: Single-project layout (no `frontend/`/`backend/` split, no `src/`/`tests/`
scaffold) because this feature is entirely backend work inside the existing Umbraco web
application, and the only external consumer (the Angular SPA) lives outside this repo. New code is
organized by concern (`Composers/`, `DeliveryApi/IndexHandlers/`, `Controllers/`, `Models/Contact/`)
alongside the existing `Program.cs`, `Views/`, `umbraco/`, and `wwwroot/`.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No violations — table intentionally omitted.

<!--
Sync Impact Report
Version change: (none) → 1.0.0
Modified principles: N/A (initial ratification)
Added sections:
  - Core Principles: I. Headless-Only Delivery (NON-NEGOTIABLE); II. Delivery API First, Web API
    as Exception; III. Code-First Content Architecture; IV. Native Bilingual Variants
  - Technology Constraints
  - Development Workflow
  - Governance
Removed sections: none (initial document)
Deferred / TODO placeholders: none
Templates requiring follow-up review (not modified by this command):
  - .specify/templates/plan-template.md — ⚠ verify its "Constitution Check" gate references these
    four principles once a feature plan is generated against them.
  - .specify/templates/spec-template.md — ⚠ verify it does not permit Razor view assignment.
  - .specify/templates/tasks-template.md — ⚠ verify task phrasing doesn't imply server-rendered
    views or manual-only backoffice steps.
  - .specify/templates/checklist-template.md — no action needed (generic).
-->

# News Portal Demo Constitution

## Core Principles

### I. Headless-Only Delivery (NON-NEGOTIABLE)
No Razor view or template MUST be assigned to any Document Type, and no page MUST be rendered
server-side. All public content MUST be served exclusively through the Umbraco Content Delivery
API to the Angular SPA client.
Rationale: the project exists to serve a decoupled frontend; a server-rendered page would create a
second, divergent rendering path and defeat the purpose of adopting the Delivery API.

### II. Delivery API First, Web API as Exception
Read access to public content MUST go through the Content Delivery API. A plain ASP.NET Core Web
API controller MAY be added only where the Delivery API structurally cannot serve the need — for
example a write operation such as the contact-form POST endpoint.
Rationale: keeping reads on a single, consistent contract avoids duplicate endpoints for the same
data and confines custom controllers to the narrow cases (writes) the Delivery API cannot cover.

### III. Code-First Content Architecture
Document Types, Data Types, languages, and starter content MUST be created through idempotent,
code-first composers or migrations, not only through manual backoffice steps. Running the
composers/migrations against a fresh clone MUST reproduce the full content architecture.
Rationale: manual-only backoffice configuration cannot be code-reviewed, diffed, or reliably
reproduced in a new environment or CI pipeline.

### IV. Native Bilingual Variants
Bilingual content (English default, Arabic) MUST use native Umbraco culture variants. Duplicate
per-language properties (e.g. separate "Title English" / "Title Arabic" fields on one variant)
are prohibited.
Rationale: culture variants let API consumers select language via a single `Accept-Language`
header or `culture` query parameter, instead of requiring per-language field-name mapping in
client code.

## Technology Constraints

- The platform baseline is Umbraco CMS 18.1.1 targeting .NET 10. Upgrades MUST be deliberate,
  version-pinned changes reviewed on their own, not incidental to unrelated feature work.
- The Content Delivery API MUST remain enabled (`Umbraco:CMS:DeliveryApi:Enabled=true`) in every
  environment where public content is served.
- Any content-shape change (new Document Type property, renamed alias, etc.) that affects what the
  Delivery API returns MUST be reflected in the project's API documentation in the same change.

## Development Workflow

- Compliance with the four Core Principles above MUST be checked before a feature is considered
  complete: no new Razor template assignment, no public-read logic added outside the Delivery API,
  new content structures created via code-first composers/migrations, and any new bilingual field
  using culture variants rather than parallel per-language properties.
- Future amendments MAY add code review, automated testing, or deployment gates as the project
  matures; none are mandated by this initial ratification beyond the checks above.

## Governance

This constitution supersedes any conflicting ad-hoc practice for this project. Amendments are made
by updating this document (via the `/speckit-constitution` command or a direct edit reviewed the
same way), recording the rationale for the change, and bumping the version per the policy below.

**Versioning policy**:
- MAJOR: backward-incompatible governance changes, or removal/redefinition of a principle.
- MINOR: a new principle or section added, or existing guidance materially expanded.
- PATCH: wording clarifications, typo fixes, or other non-semantic refinements.

**Compliance review**: every spec, plan, and task set produced by other Spec Kit commands, and
every code change, MUST be checked against the four Core Principles above before being marked
complete. A change that cannot comply MUST either be redesigned or MUST amend this constitution
first, with the rationale recorded in the amendment's Sync Impact Report.

**Version**: 1.0.0 | **Ratified**: 2026-09-19 | **Last Amended**: 2026-09-19

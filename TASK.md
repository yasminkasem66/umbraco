# News Portal Demo — Headless Umbraco (Content Delivery API only)

Execute this brief phase by phase. Do not start a phase until the previous one is confirmed complete. At the end of every phase, stop, summarize what changed, and wait before continuing to the next phase.

## Ground rules

1. **No MVC rendering.** No Razor view/template is assigned to any new Document Type and no page is rendered server-side for the public site. A plain ASP.NET Core Web API controller is allowed (needed for the contact-form POST, since the Delivery API is read-only) — that is not "MVC" in the sense this rule bans.
2. **Closed-cycle demo.** Fully self-contained. Nothing is left half-wired. Every endpoint listed in this brief must actually run and return real seeded data, not just be described in documentation.
3. **Code-first / reproducible.** Create Document Types, Data Types, languages, and starter content through an idempotent composer/startup notification handler rather than only manual backoffice clicks, so the whole content architecture rebuilds from a fresh clone. Use whichever mechanism is idiomatic for the installed Umbraco version — check the installed package's own docs/source rather than assuming an older version's API.
4. One phase at a time. No skipping ahead.

## Repo baseline

- Umbraco.Cms 18.1.1, target framework net10.0, SQL LocalDB (`umbracoPOC.mdf`).
- `Program.cs` already calls `.AddDeliveryApi()` and `app.UseCors("AngularApp")` for `http://localhost:4200`.
- `appsettings.json` already has `Umbraco:CMS:DeliveryApi:Enabled=true`, `PublicAccess=true`, `Media:Enabled=true`.
- ModelsBuilder mode is `InMemoryAuto` (no persisted `/Models` folder under source control).
- Existing Document Types are scratch/experimental and not part of this demo: `Home`, `TextPage`, `Folder` (starter kit), plus `VideoItem`, `TestAgain`, `AllowContentTest`, `CollectionTest`, `Rrr`, `Eee`, `Uuuu`, `TemplateType`. `Views/` has stray `.cshtml` files left over (`eee.cshtml`, `uuuu.cshtml`, `videoItem.cshtml`).
- Verify all of the above against the actual repo state before proceeding — treat it as a starting assumption, not ground truth.

## Objective

Build a headless News portal demo: a News module (categories + articles), a dynamic Header and Footer, an About Us page, and a Contact Us page with a working message-submission endpoint — all served to an Angular SPA purely through Umbraco's Content Delivery API, plus one small custom API controller for the contact form.

## Reference material — what to mirror, what to exclude

Reference file: `D:\Capmas\DS-CAPMAS Portal Backend\docs\SelfCensus\en\03-02-educational-content.md` (Educational Videos module), and its sibling `03-01-landing-page.md` for the "featured items on a landing area" pattern. Treat these purely as a formatting/structure reference — this demo has no other dependency on that codebase.

Mirror:
- One user story per capability (browse/filter, view detail, content model for a category, content model for an item), each with `As a ... I want ... so that ...` plus numbered `Scenario N` acceptance criteria, and a field table (`Field | Type | Description`, mandatory fields marked `*`).
- The "featured items, admin-ordered, shown on a landing area" pattern → reuse for featured News articles.

Exclude entirely:
- The editorial review workflow (Pending Review / Approve & Publish / Return to Editor / reviewer emails / audit trail). Use Umbraco's native Draft → Publish only.
- The coverage map, change-decision, and missed-visit flows — not relevant here.

Reinterpret "manage categories / manage items" stories: they are content-model specs (Document Type definition, property validation, list view columns) satisfied by the native Umbraco backoffice — there is no custom admin UI to build.

## Content architecture

**News**
- `News Category`: Name (bilingual, see Technical decisions), optional icon/colour, native sort order.
- `News Article`: Title, URL slug, Category (single picker), Summary (required, ~50–300 chars), Body (rich text / block content, required), Featured Image (required), optional Gallery, Published Date, optional Author, optional Tags, `IsFeatured` toggle + `FeaturedOrder` int.

**Site chrome**
- `Site Settings` (singleton, one node): Logo, Primary Navigation (repeatable label + link + "open in new tab"), Footer About text, Footer Link Columns (repeatable group of {column title, links}), Social Links (repeatable {platform, URL}), Copyright text.

**About Us** (single page): Title, Hero text/image, Body, optional repeatable "stats" blocks (label + value).

**Contact Us** (single page + submissions):
- Static info: Title/intro, Address, Phone, Email, Office hours, optional map coordinates, social links.
- Submittable contact message: Name, Email, Subject, Message.

## Technical decisions

- **Bilingual content = native Umbraco culture variants** (English default + Arabic), not parallel `Title (Arabic)` / `Title (English)` properties. This lets the Angular app select a language with a single `Accept-Language` header / `culture` query param on the Delivery API instead of a different field name per language. Register the two languages via a reproducible seeding step (migration/composer), not a manual backoffice-only click. State this decision explicitly when reporting Phase 1, so it can be confirmed rather than assumed.
- **Filtering/sorting by custom properties** (category, `IsFeatured`, tags) through the Delivery API requires those properties to be registered as filterable/sortable index fields. Look up the current Umbraco 18.1.1 extension point for this directly — it has changed across versions — rather than guessing. Do not ship a filter that silently no-ops.
- **Contact form submission**: since the Delivery API is read-only, implement one minimal `POST /api/contact` Web API controller (DTO validation, stores the submission as an Umbraco content node under a non-routed "Contact Submissions" container so it is visible in the backoffice list view, optionally sends a notification email via Umbraco's built-in email sender). Evaluate whether Umbraco Forms is a better fit before building the custom controller; if unsure, default to the custom controller since it adds no extra package dependency.
- Seed enough demo content to make every endpoint return real data: at least 3 categories, 8–10 articles (mixed languages, a few featured), one Site Settings node, About Us content, Contact Us content — in both English and Arabic.

## User stories to produce (acceptance criteria + field table each, in the reference doc's format)

**News**
1. Browse & filter news (public) — list, filter by category, free-text search, pagination, empty/no-results states
2. View news article detail (public)
3. Featured news on header/landing area (public + editorial `IsFeatured`/order)
4. Content model: News Category (Document Type + validation + list view)
5. Content model: News Article (Document Type + validation + list view)

**Site chrome**
6. Retrieve global header (logo, nav, language switcher) via API
7. Retrieve global footer (columns, social links, copyright) via API
8. Content model: Site Settings singleton

**About**
9. Retrieve About Us content via API
10. Content model: About Us

**Contact**
11. Retrieve Contact Us static info via API
12. Submit a contact message (validation rules, success/error responses, what happens to a submission afterward)
13. Content model: Contact Us page + submissions container (backoffice list view, no custom UI)

## Deliverables

Write these under `docs/requirements/`:
- `00-overview.md` — scope, architecture decisions, what was excluded and why.
- `01-news.md`, `02-site-chrome.md`, `03-about.md`, `04-contact.md` — one file per module, each containing that module's user stories and field tables.
- `05-api-reference.md` — the consolidated API contract: for every read endpoint, method, full path, required/optional headers (`Accept-Language`, and how to add an API key later since `PublicAccess` is currently `true`), every supported query parameter (`filter`, `sort`, `skip`/`take`, `fields`, `expand`) with concrete examples per content type, a realistic example JSON response, and error responses. For the contact POST: request schema, validation rules, success (2xx) and failure (4xx) response bodies with examples.

Optional, only if it doesn't block the above: `docs/requirements/angular-integration.md` with TypeScript interfaces matching each response shape and example `HttpClient` calls per module.

---

## Phases

### Phase 0 — Discovery & plan
Explore the repo, confirm or correct the baseline above, read the reference file. Decide what to do with the existing scratch content types and views (list them explicitly — do not delete anything yet). Propose language codes for the two cultures. Output the full step-by-step plan and any open questions.
**Stop. Wait for confirmation before writing or deleting anything.**

### Phase 1 — Foundation
Register the two languages (English default, Arabic) via a reproducible seeding step. Apply the agreed decision on scratch content types/views. Set up whatever groundwork the Delivery API needs for later custom filters.
**Stop. Report what changed.**

### Phase 2 — News content model
Create `News Category` and `News Article` Document Types (code-first), with validation and list views, per the field spec above.
**Stop. Report what changed.**

### Phase 3 — Site chrome content model
Create the `Site Settings` singleton Document Type (header + footer fields) per the spec above.
**Stop. Report what changed.**

### Phase 4 — About Us content model
Create the About Us Document Type per the spec above.
**Stop. Report what changed.**

### Phase 5 — Contact Us content model + submission endpoint
Create the Contact Us Document Type, the Contact Submissions container, and the `POST /api/contact` controller with validation and (optional) email notification.
**Stop. Report what changed.**

### Phase 6 — Seed demo content
Seed categories, articles (mixed languages, some featured), Site Settings, About Us, and Contact Us content in English and Arabic, per the minimums listed above.
**Stop. Report what was seeded.**

### Phase 7 — Verify
Run the project. Call every read endpoint and the contact POST endpoint (e.g. via `curl` or a `.http` file) and confirm the JSON shapes match what will be documented. Fix anything that doesn't match before proceeding.
**Stop. Report verification results.**

### Phase 8 — Documentation
Write all files listed under Deliverables, reflecting what was actually built and verified in Phases 1–7.
**Stop. Report what was written.**

### Phase 9 — Final summary
Summarize what is runnable, the exact URLs to hit, and anything deliberately left out of scope.

## Definition of done

- Every endpoint in `05-api-reference.md` is live and returns real seeded data when called.
- The contact form endpoint stores a real, visible submission in the backoffice.
- No Razor template is assigned to any of the new Document Types.
- All nine phases have been executed and reported on individually.

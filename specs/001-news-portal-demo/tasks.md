---

description: "Task list for the News Portal Demo feature"
---

# Tasks: News Portal Demo

**Input**: Design documents from `/specs/001-news-portal-demo/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md (all present)

**Tests**: Not included. The spec does not request automated tests, and plan.md's Testing
decision is manual verification via `quickstart.md` against a running app (research.md §7) — no
automated test project exists in the repo and this feature does not add one.

**Organization**: Tasks are grouped by user story (from spec.md, in priority order) so each story
can be implemented, verified, and demoed independently.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: Which user story this task belongs to (US1-US7)
- Every task names an exact file path from plan.md's Project Structure

## Phase 1: Setup

**Purpose**: Clear the constitution violation already sitting in the repo before adding new,
correctly-headless content (research.md §6).

- [X] T001 [P] Delete the stray scratch Razor views: `Views/eee.cshtml`, `Views/uuuu.cshtml`, `Views/videoItem.cshtml`, `Views/TemplateType.cshtml`
- [X] T002 [P] Remove the scratch Document Types and their content nodes (`VideoItem`, `TestAgain`, `AllowContentTest`, `CollectionTest`, `Rrr`, `Eee`, `Uuuu`, `TemplateType`) — implemented as `Composers/ScratchContentCleanupComposer.cs` (idempotent, via `IContentService`/`IContentTypeService`) rather than manual backoffice clicks, since it must be reproducible from a fresh clone the same way every other content-architecture step in this feature is; per research.md §6
- [X] T003 [P] Create the empty folders from plan.md's Project Structure: `Composers/`, `DeliveryApi/IndexHandlers/`, `Controllers/`, `Models/Contact/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Infrastructure every user story's content depends on — languages, and the single root
node every other content type nests under.

**⚠️ CRITICAL**: No user story content can be created until this phase is complete.

- [X] T004 [P] Create `Composers/LanguageSeedingComposer.cs`: an idempotent `IComposer` that, on `UmbracoApplicationStartingNotification`, uses `ILanguageService` to ensure English exists as the default language and Arabic (`ar`) exists as a non-default language with no `FallbackIsoCode` set, so an unpublished Arabic variant is never silently served as English (research.md §3, data-model.md)
- [X] T005 [P] Create `Composers/SiteChromeContentTypesComposer.cs` and define the `Site Root` (`siteRoot`) Document Type in it: no content-editable properties, allowed at root, no template assigned (data-model.md "Site Root")
- [X] T006 Extend `Composers/StarterContentSeedingComposer.cs` (new file): idempotent get-or-create logic that ensures exactly one `Site Root` content node exists at the content root — depends on T005

**Checkpoint**: Languages and the content-tree root exist. User story work can now begin.

---

## Phase 3: User Story 1 - Browse and Filter News Articles (Priority: P1) 🎯 MVP

**Goal**: A site visitor can retrieve published News Articles, filtered by category or a free-text
keyword, paginated, with correct empty-result behavior.

**Independent Test**: Request the news list with no filters, then with a category filter, then
with a search keyword, and confirm the returned set changes accordingly and only ever contains
published articles (contracts/delivery-api-reads.md "Browse / filter / search News Articles",
quickstart.md §2).

### Implementation for User Story 1

- [X] T007 [US1] In `Composers/NewsContentTypesComposer.cs` (new file), define the `News` (`newsFolder`) Document Type: non-routed, no content-editable properties, allowed as a child of `Site Root` (data-model.md "News")
- [X] T008 [US1] In `Composers/NewsContentTypesComposer.cs`, define the `News Category` (`newsCategory`) Document Type, culture-variant, allowed as a child of `News`: `name` (built-in, required), `icon` (Media Picker or Textstring, optional), `colour` (Textstring, optional); no template (data-model.md "News Category")
- [X] T009 [US1] In `Composers/NewsContentTypesComposer.cs`, define the `News Article` (`newsArticle`) Document Type, culture-variant, allowed as a child of `News`: `name`/title (built-in, required), `urlSlug` (Textstring, required, "unique per culture"), `category` (Content Picker constrained to `newsCategory`, required, invariant, single), `summary` (Textarea, required, "50-300 characters"), `body` (Rich Text Editor, required), `featuredImage` (Media Picker, single, image types only, required, invariant), `gallery` (Media Picker, multiple, image types only, optional, invariant), `publishedDate` (Date Picker, required, invariant), `author` (Textstring, optional), `tags` (Tags editor, optional), `isFeatured` (Toggle, invariant, default off), `featuredOrder` (Numeric/integer, invariant, "meaningful only when isFeatured is on"); no template (data-model.md "News Article") — depends on T008 for the category Content Picker's constraint
- [X] T010 [P] [US1] Create `DeliveryApi/IndexHandlers/NewsArticleIndexHandler.cs` implementing `IContentIndexHandler`: register `category` as a `FieldType.StringRaw` filter field (category content key), `tags` as a `FieldType.StringRaw` multi-value filter field, `isFeatured` as a `FieldType.Number` filter field, and `featuredOrder` as a `FieldType.Number` sort field (research.md §2, data-model.md "Delivery API filter/sort registration" — also used by User Story 3). Also create `DeliveryApi/IndexHandlers/NewsArticleQueryHandlers.cs` (`NewsArticleFilterHandler`/`NewsArticleSortHandler` implementing `IFilterHandler`/`ISortHandler`) — discovered during implementation that `IContentIndexHandler` alone only indexes a field; without a paired filter/sort handler, `filter=category:...` etc. 400s with "Filter option not found" (research.md §2)
- [X] T011 [US1] In `Composers/NewsContentTypesComposer.cs`, register via `builder.ContentIndexHandlers().Add<NewsArticleIndexHandler>()`, `builder.FilterHandlers().Add<NewsArticleFilterHandler>()`, and `builder.SortHandlers().Add<NewsArticleSortHandler>()` — depends on T009, T010
- [X] T012 [US1] Extend `Composers/StarterContentSeedingComposer.cs`: idempotently seed at least 3 News Categories and 8-10 News Articles under the `News` folder node, mixed English/Arabic culture variants, several articles marked `isFeatured` with distinct `featuredOrder` values (plan.md Scale/Scope) — depends on T006, T007, T008, T009, T011

**Checkpoint**: User Story 1 is fully functional and independently testable via
contracts/delivery-api-reads.md + quickstart.md §2.

---

## Phase 4: User Story 2 - View News Article Detail (Priority: P2)

**Goal**: A site visitor can retrieve the full detail of a single published News Article and gets
a not-found result for a Draft or nonexistent one.

**Independent Test**: Request a single published article directly by its identifier and confirm
every detail field is present; request a Draft or nonexistent identifier and confirm a
not-found result (contracts/delivery-api-reads.md "News Article detail", quickstart.md §3).

### Implementation for User Story 2

- [X] T013 [US2] Verify the Delivery API item/detail endpoint returns every News Article field from data-model.md (title, category, summary, body, featuredImage, gallery, publishedDate, author, tags) for a published article and 404s for a Draft-only or nonexistent one; if `featuredImage`, `gallery`, or `category` come back as bare references instead of expanded objects, adjust the relevant Data Type's Delivery API settings in `Composers/NewsContentTypesComposer.cs` accordingly — depends on T009, T012

**Checkpoint**: User Story 2 is independently testable against T009's content type and T012's
seeded data, with no new content type or controller code required.

---

## Phase 5: User Story 3 - Featured Articles (Priority: P3)

**Goal**: A site visitor can retrieve exactly the editor-curated, ordered set of featured News
Articles, empty when nothing is featured.

**Independent Test**: Mark a subset of seeded articles as featured with an explicit order, request
the featured set, and confirm exactly that subset returns in that order; clear a flag and confirm
it drops out (contracts/delivery-api-reads.md "Featured Articles", quickstart.md §4).

### Implementation for User Story 3

- [X] T014 [US3] Verify the Featured Articles query (`filter=contentType:newsArticle&filter=isFeatured:true&sort=featuredOrder:asc&sort=createDate:asc`) returns exactly T012's seeded featured articles in the assigned order, returns an empty `items: []` when nothing is featured, and drops an article the moment its `isFeatured` flag is cleared in the backoffice — depends on T010 (already built), T012

**Checkpoint**: User Story 3 is independently testable, reusing T010's index handler and T012's
seeded data with no new code.

---

## Phase 6: User Story 4 - Retrieve Site Header and Footer (Priority: P4)

**Goal**: The frontend can retrieve logo, navigation, footer columns, social links, and copyright
from a single Site Settings source.

**Independent Test**: Request the Site Settings content and confirm logo, navigation entries,
footer columns, social links, and copyright text are all present in one response
(contracts/delivery-api-reads.md "Site Settings", quickstart.md §5).

### Implementation for User Story 4

- [X] T015 [P] [US4] In `Composers/SiteChromeContentTypesComposer.cs`, define the `Site Settings` (`siteSettings`) singleton Document Type, allowed only as a single child of `Site Root`: `logo` (Media Picker, single, image types only, required, invariant), `primaryNavigation` (Block List of {label, link, open-in-new-tab toggle}, culture-variant, repeatable), `footerAboutText` (Rich Text Editor, optional, culture-variant), `footerLinkColumns` (Block List of {column title, links}, culture-variant, repeatable), `socialLinks` (Block List of {platform, URL}, invariant, repeatable), `copyrightText` (Textstring, optional, culture-variant); no template (data-model.md "Site Settings") — depends on T005
- [X] T016 [US4] Extend `Composers/StarterContentSeedingComposer.cs`: idempotently ensure exactly one Site Settings node exists under `Site Root`, seeded with logo, navigation entries, footer link columns, social links, and copyright text in English and Arabic — depends on T006, T015

**Checkpoint**: User Story 4 is independently testable via contracts/delivery-api-reads.md "Site
Settings" + quickstart.md §5.

---

## Phase 7: User Story 5 - Retrieve About Us Page (Priority: P5)

**Goal**: A site visitor can retrieve the About Us page's hero, body, and optional stats blocks.

**Independent Test**: Request the About Us content directly and confirm hero, body, and (if
seeded) stats blocks are present in the configured order (contracts/delivery-api-reads.md "About
Us", quickstart.md §6).

### Implementation for User Story 5

- [X] T017 [P] [US5] Create `Composers/AboutUsContentTypesComposer.cs` and define the `About Us` (`aboutUsPage`) singleton Document Type, allowed only as a single child of `Site Root`: `name`/title (built-in, required), `heroText` (Textstring, optional, culture-variant), `heroImage` (Media Picker, single, image types only, optional, invariant), `body` (Rich Text Editor, required, culture-variant), `stats` (Block List of {label, value}, optional, repeatable, culture-variant); no template (data-model.md "About Us") — depends on T005
- [X] T018 [US5] Extend `Composers/StarterContentSeedingComposer.cs`: idempotently ensure exactly one About Us node exists under `Site Root`, seeded with title, hero text/image, body, and at least one stats block, in English and Arabic — depends on T006, T017

**Checkpoint**: User Story 5 is independently testable via contracts/delivery-api-reads.md "About
Us" + quickstart.md §6.

---

## Phase 8: User Story 6 - Retrieve Contact Us Information (Priority: P6)

**Goal**: A site visitor can retrieve the Contact Us page's static info (address, phone, email,
office hours, and optional map/social links).

**Independent Test**: Request the Contact Us content directly and confirm the static fields are
present, with unset optional fields simply absent rather than erroring
(contracts/delivery-api-reads.md "Contact Us static info", quickstart.md §6).

### Implementation for User Story 6

- [X] T019 [P] [US6] Create `Composers/ContactUsContentTypesComposer.cs` and define the `Contact Us` (`contactUsPage`) singleton Document Type, allowed only as a single child of `Site Root`: `name`/title (built-in, required), `intro` (Textstring, culture-variant), `address` (Textstring, optional, culture-variant), `phone` (Textstring, optional, invariant), `email` (Textstring, optional, invariant), `officeHours` (Textstring, optional, culture-variant), `mapCoordinates` (Textstring, `"lat,lng"`, optional, invariant), `socialLinks` (Block List of {platform, URL}, optional, repeatable, invariant); no template (data-model.md "Contact Us") — depends on T005
- [X] T020 [US6] Extend `Composers/StarterContentSeedingComposer.cs`: idempotently ensure exactly one Contact Us node exists under `Site Root`, seeded with title/intro, address, phone, email, office hours, and (optionally) map coordinates/social links, in English and Arabic — depends on T006, T019

**Checkpoint**: User Story 6 is independently testable via contracts/delivery-api-reads.md
"Contact Us static info" + quickstart.md §6.

---

## Phase 9: User Story 7 - Submit a Contact Message (Priority: P7)

**Goal**: A site visitor can submit a contact message that is validated, stored for backoffice
review, and never exposed through any public read endpoint.

**Independent Test**: Submit valid data and confirm success plus visibility to backoffice review;
submit invalid data and confirm field-specific validation errors with nothing stored
(contracts/contact-submission.md, quickstart.md §7).

### Implementation for User Story 7

- [X] T021 [US7] In `Composers/ContactUsContentTypesComposer.cs`, define the `Contact Submissions` (`contactSubmissionsContainer`) Document Type: non-routed, no content-editable properties, allowed only as a single child of `Contact Us` (data-model.md "Contact Submissions") — depends on T019
- [X] T022 [US7] In `Composers/ContactUsContentTypesComposer.cs`, define the `Contact Submission` (`contactSubmission`) Document Type, invariant, allowed only as a child of `Contact Submissions`: `name` (built-in, reused as submitter name, required, "2-100 characters"), `email` (Textstring, required, "valid email format"), `subject` (Textstring, required, "3-150 characters"), `message` (Textarea, required, "10-2000 characters"); excluded from the Delivery API content index; no template (data-model.md "Contact Submission") — depends on T021
- [X] T023 [US7] Extend `Composers/StarterContentSeedingComposer.cs`: idempotently ensure exactly one Contact Submissions container node exists as a child of the Contact Us node — depends on T020, T021
- [X] T024 [P] [US7] Create `Models/Contact/ContactSubmissionRequest.cs`: request DTO with `Name` (required, "2-100 characters"), `Email` (required, "valid email format"), `Subject` (required, "3-150 characters"), `Message` (required, "10-2000 characters") validation attributes (contracts/contact-submission.md, data-model.md "Validation summary")
- [X] T025 [P] [US7] Create `Models/Contact/ContactSubmissionResponse.cs`: success shape (`status: "success"`, `submittedAt`) and validation-error shape (`status: "validationError"`, `errors: { field: [messages] }`) (contracts/contact-submission.md)
- [X] T026 [US7] Implement `Controllers/ContactController.cs`: a plain ASP.NET Core Web API controller (not an Umbraco surface/render controller) exposing `POST /api/contact`; validates the request against `ContactSubmissionRequest` and returns 400 with field-specific errors and stores nothing on failure; on success creates and publishes a `contactSubmission` node via `IContentService` under the Contact Submissions container and returns 201 with the success body; applies no rate limiting, CAPTCHA, or other abuse protection (spec.md Clarifications) — depends on T022, T023, T024, T025

**Checkpoint**: User Story 7 is independently testable via contracts/contact-submission.md +
quickstart.md §7.

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Final validation across every story.

- [X] T027 Run the full `quickstart.md` walkthrough (§1-§8) end-to-end against `dotnet run`, confirming every read endpoint and the contact POST behave as documented and that Draft/Arabic-fallback/empty-state behavior matches spec.md — depends on T013, T014, T016, T018, T020, T026
- [X] T028 Confirm no Document Type created in T005-T022 has a Template assigned (Constitution Principle I spot-check, quickstart.md §8), and confirm the scratch types/views removed in T001-T002 have not reappeared — depends on T027

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately.
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS every user story.
- **User Stories (Phase 3-9)**: All depend on Foundational (Phase 2) completion.
  - US1 (T007-T012), US4 (T015-T016), US5 (T017-T018), and US6 (T019-T020) each only depend on
    Foundational and can proceed in parallel with each other.
  - US2 (T013) and US3 (T014) additionally depend on US1's content type + seeded data (T009, T012).
  - US7 (T021-T026) additionally depends on US6's Contact Us Document Type and content (T019, T020).
- **Polish (Phase 10)**: Depends on every user story phase that was implemented.

### Within Each User Story

- Document Type definitions before the content that uses them.
- Content types before the seeding task that creates nodes of that type.
- Index handler (T010) before it is registered (T011) and before anything that queries by it (T014).
- DTOs (T024, T025) before the controller that uses them (T026).

### Parallel Opportunities

- Setup: T001, T002, T003 (different systems/files).
- Foundational: T004, T005 (different files).
- Once Foundational is done: US1, US4, US5, US6 content-type-definition tasks (T007/T008/T009's
  file, T015, T017, T019) can proceed in parallel — each is a different composer file. Their
  seeding tasks (T012, T016, T018, T020) all edit the shared `StarterContentSeedingComposer.cs`,
  so those must be serialized relative to each other even though the story work itself is
  independent.
- US7: T024 and T025 (different new files under `Models/Contact/`).

---

## Parallel Example: Foundational + first wave of user stories

```bash
# Foundational phase, launched together:
Task: "Create Composers/LanguageSeedingComposer.cs (T004)"
Task: "Create Composers/SiteChromeContentTypesComposer.cs with the Site Root Document Type (T005)"

# After Foundational completes, these can start in parallel (different composer files):
Task: "Define News/News Category/News Article Document Types in Composers/NewsContentTypesComposer.cs (T007-T009)"
Task: "Define Site Settings Document Type in Composers/SiteChromeContentTypesComposer.cs (T015)"
Task: "Define About Us Document Type in Composers/AboutUsContentTypesComposer.cs (T017)"
Task: "Define Contact Us Document Type in Composers/ContactUsContentTypesComposer.cs (T019)"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories).
3. Complete Phase 3: User Story 1 (T007-T012).
4. **STOP and VALIDATE**: run contracts/delivery-api-reads.md's browse/filter/search requests
   against the seeded data.
5. Demo if ready — the News list alone is a legitimate, independently-valuable increment.

### Incremental Delivery

1. Setup + Foundational → tree root and languages ready.
2. US1 (browse/filter/search) → demoable MVP.
3. US2 (detail) and US3 (featured) → essentially free once US1's content type and seed data
   exist; verify and move on.
4. US4, US5, US6 (site chrome, About Us, Contact Us reads) → can be built in any order, or in
   parallel by different people, once Foundational is done.
5. US7 (contact submission) → last, since it depends on US6's Contact Us page existing.
6. Phase 10 (Polish) → final end-to-end quickstart run and constitution spot-check.

## Notes

- [P] tasks touch different files and have no unfinished-task dependency.
- Every content-type task states its properties, editors, variance, and verbatim constraints so
  no validation rule is left to implementation-time guessing.
- Commit after each task or logical group.
- Stop at any checkpoint to validate a story independently before moving on.

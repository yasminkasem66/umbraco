# Feature Specification: News Portal Demo

**Feature Branch**: `001-news-portal-demo`

**Created**: 2026-09-19

**Status**: Draft

**Input**: User description: "Build a News portal demo on the existing Umbraco 18.1.1 project in this repository. It needs: a News module (News Category + News Article, browsable/filterable/searchable list, detail view, admin-selected featured articles); a Site Settings singleton providing header (logo, navigation, language switcher) and footer (link columns, social links, copyright) data; an About Us page (hero, body, optional stats blocks); and a Contact Us page with static contact info plus a working POST submission endpoint that stores messages for backoffice review. Model the requirements' structure on D:\Capmas\DS-CAPMAS Portal Backend\docs\SelfCensus\en\03-02-educational-content.md — one user story per capability, acceptance criteria as numbered scenarios, a field table per content type. Do not include any editorial review/approval workflow from that reference — use native Umbraco Draft → Publish only."

## Clarifications

### Session 2026-09-19

- Q: Should the public contact-submission capability include basic abuse protection (e.g., a rate limit on how many submissions one visitor can send in a short time), given it's a public, unauthenticated write endpoint with no API key currently in front of it? → A: No extra protection — field validation only (required fields + valid email format); no rate limit, CAPTCHA, or similar throttling is required for this demo.
- Q: Should the Contact Submission free-text fields (Name, Subject, Message) have explicit maximum length limits, the way other content fields in this spec do, or should their length stay unconstrained beyond being required? → A: Set reasonable max lengths — Name 2-100 characters, Subject 3-150 characters, Message 10-2000 characters.

### Session 2026-10-17

- Q: Verification against the running app showed that a News Article detail request for an identifier that genuinely doesn't exist, and one for an article that exists but has no translation in the requested language, return the exact same not-found response (same status, same empty body) — a caller can't tell the two apart from a single response. Should the spec require those to be distinguishable in one call, even though the Delivery API's native detail endpoint can't be made to do that without adding a custom read endpoint (which Constitution Principle II restricts to write operations)? → A: No new endpoint — document the two-request client-side check (retry the same identifier without a language header; a successful response there means "exists, not translated," a not-found means "identifier is invalid") as the supported way to tell the two apart, and note that list-driven navigation already avoids needing this because list results carry each item's available languages.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Browse and Filter News Articles (Priority: P1)

As a site visitor, I want to browse published news articles and filter them by category or a free-text keyword, so I can find the articles relevant to me among everything published.

**Why this priority**: This is the entry point of the News module and the first thing a visitor to the site sees. No other News capability (detail view, featured articles) is reachable without it.

**Independent Test**: Can be fully tested by requesting the news list with no filters, then with a category filter, then with a search keyword, and confirming the returned set changes accordingly and only ever contains published articles.

**Acceptance Scenarios**:

**Scenario 1** — Given no filter or search term is applied, when the news list is requested, then all published articles in the requested language are returned, paginated.

**Scenario 2** — Given at least one category exists, when a visitor filters by that category, then only published articles assigned to that category are returned.

**Scenario 3** — Given a search keyword, when a visitor searches, then only published articles whose title, summary, or tags match the keyword are returned.

**Scenario 4** — Given a filter or search matches no articles, when it is applied, then the list is returned empty with an explicit "no results" indication rather than an error.

**Scenario 5** — Given no published articles exist at all, when the list is requested, then an empty result with an explicit "no articles yet" indication is returned rather than an error.

**Scenario 6** — Given more articles exist than fit on one page, when the list is requested, then results are paginated and later pages can be retrieved.

---

### User Story 2 - View News Article Detail (Priority: P2)

As a site visitor, I want to open a single news article and read its full content, so I can get the complete story after finding it in the list.

**Why this priority**: The list (P1) is only useful if each item leads somewhere; the detail view is what actually delivers the article's value.

**Independent Test**: Request a single published article directly by its identifier and confirm every detail field is present, without needing to go through the list first.

**Acceptance Scenarios**:

**Scenario 1** — Given a published article's identifier, when its detail is requested, then title, category, summary, body, featured image, gallery (if present), published date, author (if present), and tags (if present) are all returned.

**Scenario 2** — Given an article exists only as a Draft, when its detail is requested by a site visitor, then it is not returned.

**Scenario 3** — Given an identifier for an article that does not exist, when its detail is requested, then a not-found result is returned rather than an error.

---

### User Story 3 - Featured Articles (Priority: P3)

As a site visitor, I want to see a curated set of featured news articles, so I can quickly see the most important stories without browsing the full list.

**Why this priority**: Adds editorial curation on top of P1/P2, used to power a landing or header highlight area — valuable, but the site is still functional without it.

**Independent Test**: Mark a subset of articles as featured with an explicit order in the backoffice, then request the featured set and confirm exactly that subset comes back in that order.

**Acceptance Scenarios**:

**Scenario 1** — Given one or more articles are marked Featured with a display order, when the featured set is requested, then only those articles are returned, sorted by their assigned order.

**Scenario 2** — Given no article is currently marked Featured, when the featured set is requested, then an empty result is returned, with no automatic substitution of other articles.

**Scenario 3** — Given an editor clears the Featured flag on an article, when the featured set is next requested, then that article no longer appears in it.

---

### User Story 4 - Retrieve Site Header and Footer (Priority: P4)

As the site's frontend, I want to retrieve the site's header (logo, navigation, language switcher data) and footer (link columns, social links, copyright) from one settings source, so every page can render consistent site chrome without duplicating it per page.

**Why this priority**: Needed on every page, but the News module (P1-P3) can be built and tested before site-wide chrome is wired in.

**Independent Test**: Request the Site Settings content and confirm logo, navigation entries, footer columns, social links, and copyright text are all present in one response.

**Acceptance Scenarios**:

**Scenario 1** — Given the Site Settings singleton has been configured, when it is requested, then logo, primary navigation entries (label, link, open-in-new-tab flag), footer about text, footer link columns, social links, and copyright text are all returned.

**Scenario 2** — Given the site supports English and Arabic, when Site Settings is requested for a given language, then the header/footer text reflects that language's content.

**Scenario 3** — Given a navigation entry or footer link is added, edited, or removed and published, when Site Settings is next requested, then the change is reflected without any code change.

---

### User Story 5 - Retrieve About Us Page (Priority: P5)

As a site visitor, I want to view the About Us page content, so I can learn about the organization.

**Why this priority**: A single static content page — self-contained and lower-traffic than News, but still part of the demo's required scope.

**Independent Test**: Request the About Us content directly and confirm hero, body, and optional stats blocks are present.

**Acceptance Scenarios**:

**Scenario 1** — Given About Us content has been published, when it is requested, then title, hero text/image, and body are returned.

**Scenario 2** — Given optional stats blocks have been added, when About Us is requested, then each stat's label and value are included, in the configured order.

**Scenario 3** — Given no stats blocks have been added, when About Us is requested, then the stats collection is simply empty, not an error.

---

### User Story 6 - Retrieve Contact Us Information (Priority: P6)

As a site visitor, I want to view the Contact Us page's static information, so I know how to reach the organization before deciding to send a message.

**Why this priority**: Static read content, needed before the submission capability (P7) makes sense to a visitor.

**Independent Test**: Request the Contact Us content directly and confirm the static fields are present.

**Acceptance Scenarios**:

**Scenario 1** — Given Contact Us content has been published, when it is requested, then title/intro, address, phone, email, and office hours are returned.

**Scenario 2** — Given optional map coordinates or social links have been configured, when Contact Us is requested, then they are included; when not configured, those fields are simply absent rather than causing an error.

---

### User Story 7 - Submit a Contact Message (Priority: P7)

As a site visitor, I want to submit a contact message with my name, email, subject, and message, so the organization can follow up with me.

**Why this priority**: The only write-capable, and only strictly "live" (non-cacheable), capability in this feature — depends on Contact Us (P6) existing first for a visitor to have a reason to submit.

**Independent Test**: Submit valid data and confirm success plus visibility to backoffice review; submit invalid data and confirm a validation error with nothing stored.

**Acceptance Scenarios**:

**Scenario 1** — Given a visitor fills in name, email, subject, and message, when they submit, then the system stores it as a reviewable record and returns a success confirmation.

**Scenario 2** — Given a required field is missing or the email is not a valid email format, when the visitor submits, then the system rejects the submission with field-specific validation errors and stores nothing.

**Scenario 3** — Given a message has been successfully submitted, when a backoffice user reviews submissions, then they can see the submitted name, email, subject, message, and submission date/time.

**Scenario 4** — Given a submission was already stored, when a visitor submits again, then it is stored as an independent additional record, never merged into or overwriting a prior one.

---

### Edge Cases

- A visitor requests content in a language for which the editor has never published a translation: that content is treated as unavailable in that language (see Assumptions), not silently shown in the other language. This produces the same not-found result as an identifier that does not exist at all (FR-017) — see FR-017 for the supported way a caller distinguishes the two.
- A News Article's assigned Category is later removed or unpublished in the backoffice: the article list/detail must continue to respond without error (exact display behavior is an editorial/content concern, not a custom-UI concern, per this feature's scope).
- Two articles are given the same Featured Order value: both are returned, with a stable but unspecified tie-break order between them.
- A list or featured-set request pages past the last available result: an empty page is returned, not an error.
- A contact submission is sent with a Name, Subject, or Message longer than its maximum length (see FR-013 and the Contact Submission field table), or containing markup/script content: an over-length submission is rejected with a validation error, and any accepted submission is stored as plain reviewable text, never executed or rendered as markup.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow a site visitor to retrieve a list of published News Articles, filterable by News Category and searchable by a free-text keyword.
- **FR-002**: System MUST return News Article list results with pagination so large result sets can be retrieved in pages.
- **FR-003**: System MUST exclude unpublished (Draft) News Articles from any public list or detail result.
- **FR-004**: System MUST return an explicit empty result, not an error, when a News Article list query or search matches nothing, or when no articles exist yet.
- **FR-005**: System MUST allow a site visitor to retrieve the full detail of a single published News Article by its identifier.
- **FR-006**: System MUST return a not-found result, not an error, when a News Article detail is requested for an identifier that does not exist or is not published.
- **FR-007**: System MUST allow an editor to mark a News Article as Featured and assign it a display order.
- **FR-008**: System MUST allow a site visitor to retrieve the set of currently Featured News Articles sorted by the editor-assigned order, and MUST return this set empty — never auto-filled with other articles — when nothing is currently featured.
- **FR-009**: System MUST provide a single Site Settings source containing logo, primary navigation entries, footer about text, footer link columns, social links, and copyright text, retrievable as one unit.
- **FR-010**: System MUST allow a site visitor to retrieve published About Us content including title, hero text/image, body, and any configured stats blocks in their configured order.
- **FR-011**: System MUST allow a site visitor to retrieve published Contact Us static content including title/intro, address, phone, email, and office hours, plus optional map coordinates and social links when configured.
- **FR-012**: System MUST allow a site visitor to submit a contact message containing name, email, subject, and message.
- **FR-013**: System MUST validate a contact message submission — all four fields required, email in a valid email format, Name 2-100 characters, Subject 3-150 characters, Message 10-2000 characters — and MUST reject an invalid submission with field-specific errors without storing it.
- **FR-014**: System MUST persist every successfully validated contact message submission as a record reviewable by backoffice staff, including its submission date/time.
- **FR-015**: System MUST NOT expose contact message submissions through any public read capability; they are backoffice-review-only.
- **FR-016**: System MUST serve all public-facing content (News, Site Settings, About Us, Contact Us) in both English (default) and Arabic, selectable per request.
- **FR-017**: System MUST treat content not yet published for a requested language as unavailable in that language rather than silently substituting the other language's content. A single detail request cannot distinguish "this identifier does not exist" from "this identifier exists but has no translation in the requested language" — both return the same not-found result (FR-006). A caller that needs to tell the two apart MUST retry the same identifier without a language qualifier: a successful result means the content exists but lacks that translation, while a repeated not-found result means the identifier itself is invalid. Callers that reach a detail view by following a list result do not need this check, since list results already carry each item's available languages (see Key Entities).
- **FR-018**: System MUST support the native Draft → Publish content lifecycle for every content type in this feature, with no additional editorial review/approval stage.

### Key Entities

#### News Category

| Field | Type | Description |
|---|---|---|
| Name * | Text | Category display name |
| Icon | Media/Text | Optional icon or symbol shown with the category |
| Colour | Text | Optional accent colour used when displaying the category |
| Sort Order | (native ordering) | Editor-controlled display order among categories |

#### News Article

| Field | Type | Description |
|---|---|---|
| Title * | Text | Article headline |
| URL Slug * | Text | Unique, URL-friendly identifier for the article |
| Category * | Single reference | The one News Category this article belongs to |
| Summary * | Text | Short teaser shown in list views; roughly 50-300 characters |
| Body * | Rich text/blocks | Full article content |
| Featured Image * | Media | Primary image shown in lists and detail |
| Gallery | Media collection | Optional additional images |
| Published Date * | Date | Date shown to visitors as the article's publish date |
| Author | Text | Optional byline |
| Tags | Text collection | Optional free-text tags, included in keyword search |
| Is Featured | Toggle | Marks the article for the Featured Articles capability; default off |
| Featured Order | Integer | Display order among featured articles; meaningful only when Is Featured is on |

#### Site Settings (singleton — exactly one instance)

| Field | Type | Description |
|---|---|---|
| Logo * | Media | Site logo image |
| Primary Navigation | Repeatable group (label, link, open in new tab) | Top-level navigation entries, in display order |
| Footer About Text | Rich text | Short about blurb shown in the footer |
| Footer Link Columns | Repeatable group (column title, links) | Grouped footer link columns |
| Social Links | Repeatable group (platform, URL) | Social media links shown in the footer |
| Copyright Text | Text | Footer copyright line |

#### About Us Page

| Field | Type | Description |
|---|---|---|
| Title * | Text | Page title |
| Hero Text | Text | Short hero/intro statement |
| Hero Image | Media | Hero banner image |
| Body * | Rich text/blocks | Main page content |
| Stats | Repeatable group (label, value) | Optional highlight statistics, in configured order |

#### Contact Us Page

| Field | Type | Description |
|---|---|---|
| Title/Intro * | Text | Page title and short introduction |
| Address | Text | Physical address |
| Phone | Text | Contact phone number |
| Email | Text | Contact email address |
| Office Hours | Text | Business hours description |
| Map Coordinates | Text/Geo | Optional latitude/longitude for a map |
| Social Links | Repeatable group (platform, URL) | Optional social links shown on the page |

#### Contact Submission (backoffice-only record — not publicly readable)

| Field | Type | Description |
|---|---|---|
| Name * | Text | Submitter's name; 2-100 characters |
| Email * | Text | Submitter's email; must be a valid email format |
| Subject * | Text | Message subject; 3-150 characters |
| Message * | Multiline text | Message body; 10-2000 characters |
| Submitted Date | Date/time | Set automatically at submission time |

All fields marked (*) are mandatory.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A site visitor can locate a specific news article — by category filter or by keyword search — without having to browse the entire article list.
- **SC-002**: 100% of published News Articles, About Us content, Contact Us content, and Site Settings are retrievable in both English and Arabic once an editor has published them in that language.
- **SC-003**: The Featured Articles shown to visitors match the editor's chosen set and order with 100% accuracy at all times.
- **SC-004**: A visitor who submits a complete, valid contact message receives a success confirmation, and that message is visible to backoffice staff with no additional manual step.
- **SC-005**: A visitor who submits an incomplete or invalid contact message immediately sees which fields need correction, and no partial record is stored.
- **SC-006**: Every public page (News list/detail, About Us, Contact Us) can render its header and footer from a single Site Settings retrieval, with no missing chrome element.
- **SC-007**: An editor can change navigation, footer links, social links, or copyright text and see it reflected on the next request, with no code change or redeploy.

## Assumptions

- Bilingual behavior follows Umbraco's native per-culture publish state: if an editor has not published a node's Arabic variant, requests for that content in Arabic are treated as unavailable in that language rather than falling back to English.
- Every piece of content in this feature is expected to be published in English (the default language) at minimum, even when Arabic is added later or omitted entirely. This is what makes the FR-017 "retry without a language qualifier" check a reliable way to distinguish "not translated" from "does not exist" — an editor publishing Arabic-only content (skipping English) is outside this assumption and is not a scenario this feature seeds or tests for.
- Pagination uses a reasonable fixed page size, since no specific size was requested; the exact number is a planning-level decision, not a scope decision.
- There is no enforced maximum number of simultaneously Featured Articles; the editor's own selection and ordering discipline governs how many are shown.
- A contact message submission does not require an outbound email notification to staff for this demo — visibility to backoffice staff via the stored record is sufficient. (Reflects the source brief's framing of email notification as optional.)
- The contact-submission capability has no abuse-prevention mechanism (rate limiting, CAPTCHA, or similar throttling) in this demo; only the field validation in FR-013 applies to an incoming submission.
- No editorial review/approval workflow (Pending Review, Approve & Publish, Return to Editor, reviewer emails, audit trail) applies to any content type in this feature; native Umbraco Draft → Publish is the only lifecycle, per this feature's explicit exclusion and the project constitution.
- "Backoffice review" of Contact Submissions uses Umbraco's native content list view; no separate custom admin screen is in scope.
- Creating and editing News Categories, Site Settings values, and other content is performed by editors through the native Umbraco backoffice; no custom admin UI is in scope for this feature.

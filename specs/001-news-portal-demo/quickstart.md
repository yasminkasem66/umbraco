# Quickstart: Validating the News Portal Demo

Proves the feature end-to-end once implemented. Assumes the composers described in
[data-model.md](data-model.md) and [research.md](research.md) have run and seeded demo content.

## Prerequisites

- SQL LocalDB available (existing `umbracoPOC.mdf` connection string in `appsettings.json`).
- `dotnet run` (or `dotnet watch run`) from the repo root; first run applies any pending Umbraco
  install/upgrade steps and executes the seeding composers.
- A REST client capable of custom headers (`curl`, VS Code REST Client `.http` file, Postman, etc.)
  — no browser UI is required since this is a headless backend.

## 1. Confirm languages were seeded

```
GET /umbraco/delivery/api/v2/content?filter=contentType:newsArticle
Accept-Language: ar
```

Expect at least the Arabic-published articles back (see data-model.md §"News Article"); English
is implicit as the default and needs no header.

## 2. Browse & filter News (User Story 1)

Run each of [contracts/delivery-api-reads.md](contracts/delivery-api-reads.md)'s "Browse / filter /
search" requests:
- No filter → all published articles, paginated.
- `filter=category:<id>` for one seeded category → only that category's articles.
- A keyword known to appear in one seeded article's tags/summary → only matching articles.
- A keyword that matches nothing → `items: []`, not an error.

## 3. Article detail (User Story 2)

Take one article id/path from step 2 and call the detail request in
[contracts/delivery-api-reads.md](contracts/delivery-api-reads.md). Confirm every field in the
News Article row of data-model.md is present. Then try a random/nonexistent id and confirm 404.

Then take an English-only article (seeded without an Arabic variant) and request its detail with
`Accept-Language: ar` — confirm this also 404s, identically to the nonexistent-id case. Retry the
same id with no language header and confirm it now returns 200, demonstrating the disambiguation
check from spec.md FR-017 / contracts/delivery-api-reads.md.

## 4. Featured Articles (User Story 3)

Mark 2-3 seeded articles `isFeatured` with distinct `featuredOrder` values in the backoffice, call
the Featured Articles request, and confirm the returned order matches. Clear the flag on one and
re-call — confirm it drops out.

## 5. Site chrome (User Story 4)

Call the Site Settings request; confirm logo, navigation, footer columns, social links, and
copyright are all present in one response, matching whatever was seeded.

## 6. About Us / Contact Us static content (User Stories 5-6)

Call each singleton request; confirm the fields from their data-model.md rows are present, and
that omitted optional fields (e.g. no stats seeded) come back empty rather than erroring.

## 7. Contact submission (User Story 7)

Using [contracts/contact-submission.md](contracts/contact-submission.md):
- Submit a fully valid payload → expect 201 and a success body.
- Submit with a missing field and an invalid email → expect 400 with both field errors, and
  confirm (via the backoffice content list under Contact Us → Contact Submissions) that nothing
  was stored for the failed attempt.
- Open the backoffice list view for Contact Submissions and confirm the valid submission from the
  previous step is visible with all four fields and its submission date.

## 8. Constitution spot-check

- Confirm no Document Type created for this feature has a template/view assigned (backoffice →
  Document Types → each type's Templates tab should be empty).
- Confirm `Views/eee.cshtml`, `Views/uuuu.cshtml`, `Views/videoItem.cshtml`, and
  `Views/TemplateType.cshtml` (and their content types) are gone, per research.md §6.

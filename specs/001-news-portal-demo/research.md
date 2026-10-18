# Phase 0 Research: News Portal Demo

All items below were open technical decisions in the Technical Context, not gaps in the spec
(the spec stays technology-agnostic by design). Each was resolved against the actual installed
package version rather than assumed from older Umbraco documentation. Evidence for the
Umbraco-API-shaped decisions was taken directly from the XML API docs shipped inside the
installed NuGet packages under `~/.nuget/packages/umbraco.cms.{core,infrastructure,api.delivery}
/18.1.1`, not from memory of older versions.

## 1. Content-architecture seeding mechanism

**Decision**: Seed languages, Document Types, Data Types, and starter content from one or more
`IComposer` implementations that run idempotent get-or-create logic (check the target exists by
alias/key before creating it) against `ILanguageService`, `IContentTypeService`,
`IDataTypeService`, and `IContentService`, invoked from `UmbracoApplicationStartingNotification`.

**Rationale**: The project brief calls for "an idempotent composer/startup notification handler,"
not a fully versioned migration system. Get-or-create composers satisfy that literally, are safe
to re-run on every boot without erroring or duplicating content, and require no extra ceremony
appropriate to a demo's scope. `ILanguageService`, `IContentTypeService`, `IDataTypeService`, and
`IContentService` are all still present and current in the installed 18.1.1 package (confirmed in
`Umbraco.Core.xml`), so this is not a legacy/removed API.

**Alternatives considered**: Umbraco Package Migrations (`PackageMigrationBase` + a migration
plan registered via `builder.AddPackageMigration(...)`) — the more "production-grade" mechanism,
with migration-state tracked in the database so each step runs exactly once. Rejected for this
feature only because it is heavier ceremony than a single-shot demo seed needs; it remains the
recommended upgrade path if this project grows past demo scope.

## 2. Making custom properties filterable/sortable on the Content Delivery API

**Decision**: Two extension points are required together, not one:

1. `Umbraco.Cms.Core.DeliveryApi.IContentIndexHandler` — once per field that needs to be
   queryable (`category`, `isFeatured`, `tags`, `featuredOrder`), registered via
   `builder.ContentIndexHandlers().Add<T>()`. `GetFields()` declares the field name(s) and
   `Umbraco.Cms.Core.DeliveryApi.FieldType` (`StringRaw` for `category`/`tags`, `Number` for
   `isFeatured`/`featuredOrder`); `GetFieldValues(content, culture)` supplies the value(s) per
   content item. This alone makes the field **indexed** (searchable), confirmed by inspecting the
   raw Examine-backed index behavior end to end against a running instance.
2. `Umbraco.Cms.Core.DeliveryApi.IFilterHandler` / `ISortHandler` — one implementation covering
   the custom field names, registered via `builder.FilterHandlers().Add<T>()` /
   `builder.SortHandlers().Add<T>()`. **This second registration is not optional and is easy to
   miss**: without it, every request using `filter=category:...`, `filter=isFeatured:...`, or
   `sort=featuredOrder:...` fails with `400 Filter option not found`, even though the field is
   correctly indexed. The built-in `contentType`/`name`/`createDate`/`updateDate` filters and
   `createDate`/`level`/`name`/`sortOrder`/`updateDate` sorts each have their own dedicated
   `IFilterHandler`/`ISortHandler` implementation in `Umbraco.Cms.Api.Delivery.Querying.Filters`/
   `.Sorts` — there is no generic handler that automatically exposes every `IContentIndexHandler`
   field as a query option, so custom fields need the same paired treatment.

Once both are registered, the fields are queryable through the standard Delivery API query
string, e.g. `filter=category:<id>`, `filter=isFeatured:1`, `sort=featuredOrder:asc`.

**Rationale**: These are the actual extension points present in the installed `Umbraco.Cms.Core`
18.1.1 / `Umbraco.Cms.Api.Delivery` 18.1.1 packages, confirmed two ways: first via the shipped XML
API docs, then — after the filter/sort gap surfaced as a live `400` against the running
application during implementation — via runtime reflection against the installed assemblies
(`LazyCollectionBuilderBase<TBuilder,TCollection,TItem>.Add<T>()`, `IFilterHandler`/`ISortHandler`
both extending `IQueryHandler.CanHandle(string)`) to get the exact member names and signatures
rather than continuing to guess from XML doc summaries alone.

**Two related pitfalls found the same way** (worth recording since they cost real debugging time):

- A Block List property's stored JSON needs a populated `expose` array
  (`Umbraco.Cms.Core.Models.Blocks.BlockItemVariation` entries, one per block, with `culture: null`
  even when the outer property is culture-variant and already saved once per culture) or the
  Delivery API returns `{"items": []}` for that property despite `contentData` and `layout` both
  being populated correctly — confirmed via direct inspection of `umbracoPropertyData` compared
  against the Delivery API response, before vs. after adding `Expose`.
- A **culture-invariant** property (`category`, a Content Picker) must always be read with a
  `null` culture when building index field values, even though the Delivery API's indexing pass
  invokes `IContentIndexHandler.GetFieldValues` once per culture of the (culture-variant) owning
  content item. Passing that per-pass culture straight through to `content.GetValue<T>(alias,
  culture)` silently returns `null` for the invariant property, so nothing gets indexed and every
  `filter=category:...` request matches zero articles with no error at all.

**Alternatives considered**: The community package `Umbraco.Community.DeliveryApiExtensions` is
present in the local NuGet cache (version 17.0.0 only — no 18.1.1 build), and would add an extra
third-party dependency for something the core package already exposes a first-party extension
point for. Rejected: unnecessary dependency, and not confirmed compatible with 18.1.1.

## 3. Bilingual content (English default, Arabic)

**Decision**: Register both languages through `ILanguageService.CreateAsync` (English as the
existing default, Arabic added with `FallbackIsoCode` unset so it does **not** silently fall back
to English), and mark every content type in this feature as culture-variant so each property is
entered once per culture rather than duplicated. A node with no published Arabic variant is
therefore correctly reported by the Delivery API as unavailable in Arabic (matches the spec's
Clarifications: no automatic fallback).

**Rationale**: `ILanguageService` is the current async language service in the installed 18.1.1
package (confirmed in `Umbraco.Core.xml`: `GetAsync`, `GetAllAsync`, `CreateAsync`), replacing the
older synchronous `ILocalizationService` API some pre-v14 tutorials still reference.

## 4. Contact form: custom controller vs. Umbraco Forms

**Decision**: A minimal `ASP.NET Core Web API controller (`POST /api/contact`) that validates the
request and stores it via `IContentService` as a content node under a non-routed "Contact
Submissions" container, per Constitution Principle II's explicit write-operation exception.

**Rationale**: `Directory.Packages.props` (repo root) does not reference `Umbraco.Forms` today,
and Umbraco Forms is a separate, additional package. Adding it purely to collect four fields would
introduce a new dependency, its own content-type conventions, and (depending on license terms)
licensing considerations the brief does not ask for. The brief explicitly says to default to the
custom controller "if unsure" — a custom controller with a 4-field DTO is materially simpler and
keeps the "closed-cycle, self-contained demo" ground rule intact.

**Alternatives considered**: Umbraco Forms — evaluated and rejected for the reasons above; noted
here so the trade-off is visible rather than silently skipped.

## 5. Content-tree root structure

**Decision**: Introduce one new, minimal root-level Document Type (`Site Root`, not routed to any
public output itself) as the single allowed-at-root type. Beneath it: a `News` folder holding
`News Category` and `News Article` nodes, one `Site Settings` singleton, one `About Us` page, and
one `Contact Us` page whose own child is the non-routed `Contact Submissions` container.

**Rationale**: The existing `Home`/`TextPage`/`Folder` starter-kit types are explicitly out of
scope for this demo per the project brief's own baseline notes, so a fresh, minimal root type
avoids depending on or repurposing content the project has already decided not to reuse. Every
node still needs some root ancestor; a single-purpose root type documents that intent explicitly
rather than implicitly relying on whatever the starter kit happened to install.

**Alternatives considered**: Reusing the existing `Home` Document Type as the root. Rejected
because the brief's baseline notes already earmark `Home` as scratch/not-part-of-this-demo, and
reusing it would blur that decision.

## 6. Existing scratch Document Types and stray Views

**Decision**: Remove the scratch Document Types (`VideoItem`, `TestAgain`, `AllowContentTest`,
`CollectionTest`, `Rrr`, `Eee`, `Uuuu`, `TemplateType`) together with their content nodes, and
delete the stray `Views/eee.cshtml`, `Views/uuuu.cshtml`, `Views/videoItem.cshtml`,
`Views/TemplateType.cshtml`. `Home`, `TextPage`, and `Folder` (the starter-kit types) are left in
place but unused by this feature — removing them is a separate, larger decision (they may still
back other starter-kit content) outside this feature's scope.

**Rationale**: Constitution Principle I forbids any Razor view/template being assigned to any
Document Type. Every one of the scratch types above currently has (or, per the repo baseline, is
positioned to have) a `.cshtml` view assigned, which is a direct, standing violation the moment
this feature's composers run in the same content tree. Removing them before adding new,
correctly-headless content types keeps the tree free of that contradiction from the start, and
satisfies the brief's own "closed-cycle demo, nothing left half-wired" rule.

**Alternatives considered**: Leaving them in place since they're "someone else's scratch work."
Rejected: they are unrouted/unreachable via any real content and directly contradict a
NON-NEGOTIABLE constitution principle if left wired to views; removing them is a one-time cleanup
step, not an ongoing cost.

## 7. Verification approach

**Decision**: Manual verification via a `.http` file (or equivalent `curl` calls) exercising every
read endpoint and the contact POST endpoint against seeded data, per the project brief's own
verification phase. No automated test project exists in the repo today, and adding one is not
requested by the spec or the brief.

**Rationale**: Matches the project brief exactly ("Run the project. Call every read endpoint and
the contact POST endpoint... confirm the JSON shapes match"). Introducing an automated test
project would be scope not asked for by either the spec or the technical brief.

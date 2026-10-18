using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;

namespace UmbracoDemo.Composers;

/// <summary>
/// Shared get-or-create helpers used by every content-architecture composer, so each one only
/// states what is different about its own Data Types / Document Types (data-model.md) instead of
/// repeating the boilerplate for the idempotent "does it already exist?" check every time
/// (research.md §1).
/// </summary>
public class ContentTypeHelpers
{
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IDataTypeService _dataTypeService;
    private readonly IContentTypeService _contentTypeService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

    public ContentTypeHelpers(
        IShortStringHelper shortStringHelper,
        IDataTypeService dataTypeService,
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        PropertyEditorCollection propertyEditors,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
    {
        _shortStringHelper = shortStringHelper;
        _dataTypeService = dataTypeService;
        _contentTypeService = contentTypeService;
        _mediaTypeService = mediaTypeService;
        _propertyEditors = propertyEditors;
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
    }

    public async Task<IDataType> EnsureDataTypeAsync(Guid seedingUserKey, string name, string editorAlias, string editorUiAlias, object? configurationObject = null)
    {
        IDataEditor editor = _propertyEditors[editorAlias]
            ?? throw new InvalidOperationException($"No property editor registered for alias '{editorAlias}'.");

        IDictionary<string, object>? intendedConfig = configurationObject is null
            ? null
            : editor.GetConfigurationEditor().FromConfigurationObject(configurationObject, _configurationEditorJsonSerializer);

        IDataType? existing = await _dataTypeService.GetAsync(name);
        if (existing is not null)
        {
            // Create-if-missing alone isn't enough for a code-first content architecture
            // (Constitution Principle III): if the config (or, as happened here, the UI alias) in
            // code changes after a Data Type was already seeded once, an environment that seeded
            // it earlier would otherwise keep the stale value forever, silently drifting from what
            // the code now says. This self-corrects it instead, the same way creation itself is
            // idempotent. EditorUiAlias in particular was never set at all before this fix, which
            // is why every custom Data Type showed "configured property editor UI could not be
            // found" in the backoffice — the IDataType model distinguishes the .NET-level
            // "property editor" (editorAlias) from the backoffice web component that renders it
            // (editorUiAlias), and only the former was ever being set.
            bool needsUpdate = !ConfigurationMatches(existing.ConfigurationData, intendedConfig)
                || existing.EditorUiAlias != editorUiAlias;

            if (needsUpdate)
            {
                existing.ConfigurationData = intendedConfig ?? new Dictionary<string, object>();
                existing.EditorUiAlias = editorUiAlias;
                Attempt<IDataType, DataTypeOperationStatus> updateAttempt = await _dataTypeService.UpdateAsync(existing, seedingUserKey);
                if (updateAttempt.Success is false)
                {
                    throw new InvalidOperationException($"Failed to update Data Type '{name}': {updateAttempt.Status}.");
                }

                return updateAttempt.Result;
            }

            return existing;
        }

        var dataType = new DataType(editor, _configurationEditorJsonSerializer)
        {
            Name = name,
            EditorUiAlias = editorUiAlias,
        };

        if (intendedConfig is not null)
        {
            dataType.ConfigurationData = intendedConfig;
        }

        Attempt<IDataType, DataTypeOperationStatus> attempt = await _dataTypeService.CreateAsync(dataType, seedingUserKey);
        if (attempt.Success is false)
        {
            throw new InvalidOperationException($"Failed to create Data Type '{name}': {attempt.Status}.");
        }

        return attempt.Result;
    }

    private static bool ConfigurationMatches(IDictionary<string, object>? current, IDictionary<string, object>? intended)
    {
        string currentJson = System.Text.Json.JsonSerializer.Serialize(current ?? new Dictionary<string, object>());
        string intendedJson = System.Text.Json.JsonSerializer.Serialize(intended ?? new Dictionary<string, object>());
        return currentJson == intendedJson;
    }

    public sealed record PropertyDef(
        string Alias,
        string Name,
        IDataType DataType,
        bool Mandatory = false,
        string? MandatoryMessage = null,
        string? ValidationRegExp = null,
        string? ValidationRegExpMessage = null,
        ContentVariation Variation = ContentVariation.Culture);

    public async Task<IContentType> EnsureContentTypeAsync(
        Guid seedingUserKey,
        string alias,
        string name,
        string icon,
        ContentVariation variation,
        IReadOnlyList<PropertyDef> properties,
        bool isElement = false,
        bool allowedAsRoot = false,
        IReadOnlyList<IContentType>? allowedChildren = null)
    {
        IContentType? existing = _contentTypeService.Get(alias);
        if (existing is not null)
        {
            return existing;
        }

        var contentType = new ContentType(_shortStringHelper, -1)
        {
            Alias = alias,
            Name = name,
            Icon = icon,
            IsElement = isElement,
            AllowedAsRoot = allowedAsRoot,
            Variations = variation,
        };

        if (properties.Count > 0)
        {
            contentType.AddPropertyGroup("content", "Content");

            foreach (PropertyDef property in properties)
            {
                var propertyType = new PropertyType(_shortStringHelper, property.DataType)
                {
                    Alias = property.Alias,
                    Name = property.Name,
                    Mandatory = property.Mandatory,
                    Variations = property.Variation,
                };

                if (property.MandatoryMessage is not null)
                {
                    propertyType.MandatoryMessage = property.MandatoryMessage;
                }

                if (property.ValidationRegExp is not null)
                {
                    propertyType.ValidationRegExp = property.ValidationRegExp;
                    propertyType.ValidationRegExpMessage = property.ValidationRegExpMessage;
                }

                contentType.AddPropertyType(propertyType, "content");
            }
        }

        if (allowedChildren is { Count: > 0 })
        {
            contentType.AllowedContentTypes = allowedChildren
                .Select((child, index) => new ContentTypeSort(child.Key, index, child.Alias))
                .ToList();
        }

        Attempt<ContentTypeOperationStatus> attempt = await _contentTypeService.CreateAsync(contentType, seedingUserKey);
        if (attempt.Success is false)
        {
            throw new InvalidOperationException($"Failed to create Document Type '{alias}': {attempt.Result}.");
        }

        return contentType;
    }

    public async Task AllowChildAsync(Guid seedingUserKey, IContentType parent, IContentType child)
    {
        IContentType? current = _contentTypeService.Get(parent.Alias);
        if (current is null)
        {
            return;
        }

        if (current.AllowedContentTypes?.Any(a => a.Alias == child.Alias) == true)
        {
            return;
        }

        var allowed = (current.AllowedContentTypes ?? Enumerable.Empty<ContentTypeSort>()).ToList();
        allowed.Add(new ContentTypeSort(child.Key, allowed.Count, child.Alias));
        current.AllowedContentTypes = allowed;

        Attempt<ContentTypeOperationStatus> attempt = await _contentTypeService.UpdateAsync(current, seedingUserKey);
        if (attempt.Success is false)
        {
            throw new InvalidOperationException($"Failed to update Document Type '{parent.Alias}': {attempt.Result}.");
        }
    }

    public Task<IContentType> EnsureSiteRootAsync(Guid seedingUserKey) =>
        EnsureContentTypeAsync(
            seedingUserKey,
            alias: "siteRoot",
            name: "Site Root",
            icon: "icon-home",
            variation: ContentVariation.Nothing,
            allowedAsRoot: true,
            properties: []);

    public async Task<IDataType> EnsureBlockListDataTypeAsync(Guid seedingUserKey, string name, IReadOnlyList<IContentType> elementTypes)
    {
        var configuration = new BlockListConfiguration
        {
            Blocks = elementTypes
                .Select(elementType => new BlockListConfiguration.BlockConfiguration
                {
                    ContentElementTypeKey = elementType.Key,
                })
                .ToArray(),
        };

        return await EnsureDataTypeAsync(seedingUserKey, name, Constants.PropertyEditors.Aliases.BlockList, "Umb.PropertyEditorUi.BlockList", configuration);
    }

    // Shared, reusable element types (research.md-level "shared infra" — reused as-is across the
    // Site Settings, About Us, and Contact Us composers via the idempotent get-or-create check
    // above, regardless of which composer runs first).

    public async Task<IContentType> EnsureLinkElementTypeAsync(Guid seedingUserKey)
    {
        IDataType textstring = await TextstringDataTypeAsync(seedingUserKey);
        IDataType boolean = await BooleanDataTypeAsync(seedingUserKey);

        return await EnsureContentTypeAsync(
            seedingUserKey,
            alias: "link",
            name: "Link",
            icon: "icon-link",
            variation: ContentVariation.Nothing,
            isElement: true,
            properties:
            [
                new PropertyDef("label", "Label", textstring, Mandatory: true, Variation: ContentVariation.Nothing),
                new PropertyDef("url", "URL", textstring, Mandatory: true, Variation: ContentVariation.Nothing),
                new PropertyDef("openInNewTab", "Open in new tab", boolean, Variation: ContentVariation.Nothing),
            ]);
    }

    public async Task<IContentType> EnsureSocialLinkElementTypeAsync(Guid seedingUserKey)
    {
        IDataType textstring = await TextstringDataTypeAsync(seedingUserKey);

        return await EnsureContentTypeAsync(
            seedingUserKey,
            alias: "socialLink",
            name: "Social Link",
            icon: "icon-link",
            variation: ContentVariation.Nothing,
            isElement: true,
            properties:
            [
                new PropertyDef("platform", "Platform", textstring, Mandatory: true, Variation: ContentVariation.Nothing),
                new PropertyDef("url", "URL", textstring, Mandatory: true, Variation: ContentVariation.Nothing),
            ]);
    }

    public async Task<IContentType> EnsureStatItemElementTypeAsync(Guid seedingUserKey)
    {
        IDataType textstring = await TextstringDataTypeAsync(seedingUserKey);

        return await EnsureContentTypeAsync(
            seedingUserKey,
            alias: "statItem",
            name: "Stat Item",
            icon: "icon-indent",
            variation: ContentVariation.Nothing,
            isElement: true,
            properties:
            [
                new PropertyDef("label", "Label", textstring, Mandatory: true, Variation: ContentVariation.Nothing),
                new PropertyDef("value", "Value", textstring, Mandatory: true, Variation: ContentVariation.Nothing),
            ]);
    }

    public async Task<IContentType> EnsureFooterLinkColumnElementTypeAsync(Guid seedingUserKey)
    {
        IContentType link = await EnsureLinkElementTypeAsync(seedingUserKey);
        IDataType linkListDataType = await EnsureBlockListDataTypeAsync(seedingUserKey, "Link List", [link]);

        return await EnsureContentTypeAsync(
            seedingUserKey,
            alias: "footerLinkColumn",
            name: "Footer Link Column",
            icon: "icon-bulleted-list",
            variation: ContentVariation.Nothing,
            isElement: true,
            properties:
            [
                new PropertyDef("columnTitle", "Column Title", await TextstringDataTypeAsync(seedingUserKey), Mandatory: true, Variation: ContentVariation.Nothing),
                new PropertyDef("links", "Links", linkListDataType, Variation: ContentVariation.Nothing),
            ]);
    }

    public Task<IDataType> TextstringDataTypeAsync(Guid seedingUserKey) =>
        EnsureDataTypeAsync(seedingUserKey, "Textstring", Constants.PropertyEditors.Aliases.TextBox, "Umb.PropertyEditorUi.TextBox");

    public Task<IDataType> TextareaDataTypeAsync(Guid seedingUserKey) =>
        EnsureDataTypeAsync(seedingUserKey, "Textarea", Constants.PropertyEditors.Aliases.TextArea, "Umb.PropertyEditorUi.TextArea");

    public Task<IDataType> RichTextDataTypeAsync(Guid seedingUserKey) =>
        EnsureDataTypeAsync(seedingUserKey, "Rich Text Editor", Constants.PropertyEditors.Aliases.RichText, "Umb.PropertyEditorUi.Tiptap");

    public Task<IDataType> BooleanDataTypeAsync(Guid seedingUserKey) =>
        EnsureDataTypeAsync(seedingUserKey, "True/false", Constants.PropertyEditors.Aliases.Boolean, "Umb.PropertyEditorUi.Toggle");

    public Task<IDataType> DatePickerDataTypeAsync(Guid seedingUserKey) =>
        EnsureDataTypeAsync(seedingUserKey, "Date Picker", Constants.PropertyEditors.Aliases.DateTime, "Umb.PropertyEditorUi.DatePicker");

    public Task<IDataType> IntegerDataTypeAsync(Guid seedingUserKey) =>
        EnsureDataTypeAsync(seedingUserKey, "Numeric", Constants.PropertyEditors.Aliases.Integer, "Umb.PropertyEditorUi.Integer");

    public Task<IDataType> TagsDataTypeAsync(Guid seedingUserKey) =>
        EnsureDataTypeAsync(seedingUserKey, "Tags", Constants.PropertyEditors.Aliases.Tags, "Umb.PropertyEditorUi.Tags");

    public Task<IDataType> MediaPickerDataTypeAsync(Guid seedingUserKey, string name, bool multiple) =>
        EnsureDataTypeAsync(
            seedingUserKey,
            name,
            Constants.PropertyEditors.Aliases.MediaPicker3,
            "Umb.PropertyEditorUi.MediaPicker",
            new MediaPicker3Configuration
            {
                // The backoffice Media Picker's allow-list is NOT a media type alias — decompiling
                // the actual property-editor-ui-media-picker / input-upload-field backoffice
                // components showed the stored "filter" string is split on commas and each piece
                // compared via `allowedContentTypeIds.includes(mediaType.unique)`, i.e. it must be
                // the media type's GUID key. An alias string here (whatever casing) always fails
                // that comparison and silently leaves the picker/upload with nothing allowed —
                // this was wrong twice before this fix, first as "image" then as "Image".
                Filter = GetImageMediaTypeKey().ToString(),
                Multiple = multiple,
            });

    private Guid GetImageMediaTypeKey() =>
        _mediaTypeService.Get(Constants.Conventions.MediaTypes.Image)?.Key
            ?? throw new InvalidOperationException($"Built-in media type '{Constants.Conventions.MediaTypes.Image}' was not found.");

    public Task<IDataType> ContentPickerDataTypeAsync(Guid seedingUserKey, string name, Guid allowedContentTypeKey) =>
        EnsureDataTypeAsync(
            seedingUserKey,
            name,
            Constants.PropertyEditors.Aliases.ContentPicker,
            "Umb.PropertyEditorUi.DocumentPicker",
            new ContentPickerConfiguration
            {
                AllowedContentTypeIds = allowedContentTypeKey.ToString(),
            });
}

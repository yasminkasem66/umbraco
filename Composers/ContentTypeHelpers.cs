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
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

    public ContentTypeHelpers(
        IShortStringHelper shortStringHelper,
        IDataTypeService dataTypeService,
        IContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditors,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
    {
        _shortStringHelper = shortStringHelper;
        _dataTypeService = dataTypeService;
        _contentTypeService = contentTypeService;
        _propertyEditors = propertyEditors;
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
    }

    public async Task<IDataType> EnsureDataTypeAsync(Guid seedingUserKey, string name, string editorAlias, object? configurationObject = null)
    {
        IDataType? existing = await _dataTypeService.GetAsync(name);
        if (existing is not null)
        {
            return existing;
        }

        IDataEditor editor = _propertyEditors[editorAlias]
            ?? throw new InvalidOperationException($"No property editor registered for alias '{editorAlias}'.");

        var dataType = new DataType(editor, _configurationEditorJsonSerializer)
        {
            Name = name,
        };

        if (configurationObject is not null)
        {
            dataType.ConfigurationData = editor.GetConfigurationEditor()
                .FromConfigurationObject(configurationObject, _configurationEditorJsonSerializer);
        }

        Attempt<IDataType, DataTypeOperationStatus> attempt = await _dataTypeService.CreateAsync(dataType, seedingUserKey);
        if (attempt.Success is false)
        {
            throw new InvalidOperationException($"Failed to create Data Type '{name}': {attempt.Status}.");
        }

        return attempt.Result;
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

        return await EnsureDataTypeAsync(seedingUserKey, name, Constants.PropertyEditors.Aliases.BlockList, configuration);
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
        EnsureDataTypeAsync(seedingUserKey, "Textstring", Constants.PropertyEditors.Aliases.TextBox);

    public Task<IDataType> TextareaDataTypeAsync(Guid seedingUserKey) =>
        EnsureDataTypeAsync(seedingUserKey, "Textarea", Constants.PropertyEditors.Aliases.TextArea);

    public Task<IDataType> RichTextDataTypeAsync(Guid seedingUserKey) =>
        EnsureDataTypeAsync(seedingUserKey, "Rich Text Editor", Constants.PropertyEditors.Aliases.RichText);

    public Task<IDataType> BooleanDataTypeAsync(Guid seedingUserKey) =>
        EnsureDataTypeAsync(seedingUserKey, "True/false", Constants.PropertyEditors.Aliases.Boolean);

    public Task<IDataType> DatePickerDataTypeAsync(Guid seedingUserKey) =>
        EnsureDataTypeAsync(seedingUserKey, "Date Picker", Constants.PropertyEditors.Aliases.DateTime);

    public Task<IDataType> IntegerDataTypeAsync(Guid seedingUserKey) =>
        EnsureDataTypeAsync(seedingUserKey, "Numeric", Constants.PropertyEditors.Aliases.Integer);

    public Task<IDataType> TagsDataTypeAsync(Guid seedingUserKey) =>
        EnsureDataTypeAsync(seedingUserKey, "Tags", Constants.PropertyEditors.Aliases.Tags);

    public Task<IDataType> MediaPickerDataTypeAsync(Guid seedingUserKey, string name, bool multiple) =>
        EnsureDataTypeAsync(
            seedingUserKey,
            name,
            Constants.PropertyEditors.Aliases.MediaPicker3,
            new MediaPicker3Configuration
            {
                Filter = "image",
                Multiple = multiple,
            });

    public Task<IDataType> ContentPickerDataTypeAsync(Guid seedingUserKey, string name, Guid allowedContentTypeKey) =>
        EnsureDataTypeAsync(
            seedingUserKey,
            name,
            Constants.PropertyEditors.Aliases.ContentPicker,
            new ContentPickerConfiguration
            {
                AllowedContentTypeIds = allowedContentTypeKey.ToString(),
            });
}

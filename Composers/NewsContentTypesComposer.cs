using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using UmbracoDemo.DeliveryApi.IndexHandlers;

namespace UmbracoDemo.Composers;

public class NewsContentTypesComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.ContentIndexHandlers().Add<NewsArticleIndexHandler>();
        builder.FilterHandlers().Add<NewsArticleFilterHandler>();
        builder.SortHandlers().Add<NewsArticleSortHandler>();
        builder.Services.AddSingleton<NewsContentTypesInstaller>();
    }
}

/// <summary>
/// News, News Category, and News Article (data-model.md) — User Story 1 (browse/filter/search),
/// reused as-is by User Story 2 (detail) and User Story 3 (featured). Called directly by the
/// seeding composer's single startup handler — see StarterContentSeedingComposer.cs.
/// </summary>
public class NewsContentTypesInstaller
{
    private static readonly Guid SeedingUserKey = Constants.Security.SuperUserKey;

    private readonly ContentTypeHelpers _helpers;

    public NewsContentTypesInstaller(ContentTypeHelpers helpers)
    {
        _helpers = helpers;
    }

    public async Task<IContentType> EnsureNewsFolderAsync()
    {
        IContentType siteRoot = await _helpers.EnsureSiteRootAsync(SeedingUserKey);

        IContentType newsFolder = await _helpers.EnsureContentTypeAsync(
            SeedingUserKey,
            alias: "newsFolder",
            name: "News",
            icon: "icon-newspaper",
            variation: ContentVariation.Nothing,
            properties: []);

        await _helpers.AllowChildAsync(SeedingUserKey, siteRoot, newsFolder);

        return newsFolder;
    }

    public async Task<IContentType> EnsureNewsCategoryAsync()
    {
        IContentType newsFolder = await EnsureNewsFolderAsync();

        IDataType textstring = await _helpers.TextstringDataTypeAsync(SeedingUserKey);
        IDataType mediaPicker = await _helpers.MediaPickerDataTypeAsync(SeedingUserKey, "Media Picker (single image)", multiple: false);

        IContentType newsCategory = await _helpers.EnsureContentTypeAsync(
            SeedingUserKey,
            alias: "newsCategory",
            name: "News Category",
            icon: "icon-tags",
            variation: ContentVariation.Culture,
            properties:
            [
                new ContentTypeHelpers.PropertyDef("icon", "Icon", mediaPicker, Variation: ContentVariation.Nothing),
                new ContentTypeHelpers.PropertyDef("colour", "Colour", textstring, Variation: ContentVariation.Nothing),
            ]);

        await _helpers.AllowChildAsync(SeedingUserKey, newsFolder, newsCategory);

        return newsCategory;
    }

    public async Task<IContentType> EnsureNewsArticleAsync()
    {
        IContentType newsFolder = await EnsureNewsFolderAsync();
        IContentType newsCategory = await EnsureNewsCategoryAsync();

        IDataType textstring = await _helpers.TextstringDataTypeAsync(SeedingUserKey);
        IDataType textarea = await _helpers.TextareaDataTypeAsync(SeedingUserKey);
        IDataType richText = await _helpers.RichTextDataTypeAsync(SeedingUserKey);
        IDataType categoryPicker = await _helpers.ContentPickerDataTypeAsync(SeedingUserKey, "News Category Picker", newsCategory.Key);
        IDataType featuredImagePicker = await _helpers.MediaPickerDataTypeAsync(SeedingUserKey, "Media Picker (single image)", multiple: false);
        IDataType galleryPicker = await _helpers.MediaPickerDataTypeAsync(SeedingUserKey, "Media Picker (multiple images)", multiple: true);
        IDataType datePicker = await _helpers.DatePickerDataTypeAsync(SeedingUserKey);
        IDataType tags = await _helpers.TagsDataTypeAsync(SeedingUserKey);
        IDataType boolean = await _helpers.BooleanDataTypeAsync(SeedingUserKey);
        IDataType integer = await _helpers.IntegerDataTypeAsync(SeedingUserKey);

        IContentType newsArticle = await _helpers.EnsureContentTypeAsync(
            SeedingUserKey,
            alias: "newsArticle",
            name: "News Article",
            icon: "icon-article",
            variation: ContentVariation.Culture,
            properties:
            [
                new ContentTypeHelpers.PropertyDef("urlSlug", "URL Slug", textstring, Mandatory: true, Variation: ContentVariation.Culture),
                new ContentTypeHelpers.PropertyDef("category", "Category", categoryPicker, Mandatory: true, Variation: ContentVariation.Nothing),
                new ContentTypeHelpers.PropertyDef("summary", "Summary", textarea, Mandatory: true, ValidationRegExp: "^.{50,300}$", ValidationRegExpMessage: "Summary must be 50-300 characters.", Variation: ContentVariation.Culture),
                new ContentTypeHelpers.PropertyDef("body", "Body", richText, Mandatory: true, Variation: ContentVariation.Culture),
                new ContentTypeHelpers.PropertyDef("featuredImage", "Featured Image", featuredImagePicker, Mandatory: true, Variation: ContentVariation.Nothing),
                new ContentTypeHelpers.PropertyDef("gallery", "Gallery", galleryPicker, Variation: ContentVariation.Nothing),
                new ContentTypeHelpers.PropertyDef("publishedDate", "Published Date", datePicker, Mandatory: true, Variation: ContentVariation.Nothing),
                new ContentTypeHelpers.PropertyDef("author", "Author", textstring, Variation: ContentVariation.Culture),
                new ContentTypeHelpers.PropertyDef("tags", "Tags", tags, Variation: ContentVariation.Culture),
                new ContentTypeHelpers.PropertyDef("isFeatured", "Is Featured", boolean, Variation: ContentVariation.Nothing),
                new ContentTypeHelpers.PropertyDef("featuredOrder", "Featured Order", integer, Variation: ContentVariation.Nothing),
            ]);

        await _helpers.AllowChildAsync(SeedingUserKey, newsFolder, newsArticle);

        return newsArticle;
    }
}

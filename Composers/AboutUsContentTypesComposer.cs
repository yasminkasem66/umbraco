using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;

namespace UmbracoDemo.Composers;

public class AboutUsContentTypesComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<AboutUsContentTypesInstaller>();
    }
}

/// <summary>
/// About Us (data-model.md) — User Story 5. Called directly by the seeding composer's single
/// startup handler — see StarterContentSeedingComposer.cs.
/// </summary>
public class AboutUsContentTypesInstaller
{
    private static readonly Guid SeedingUserKey = Constants.Security.SuperUserKey;

    private readonly ContentTypeHelpers _helpers;

    public AboutUsContentTypesInstaller(ContentTypeHelpers helpers)
    {
        _helpers = helpers;
    }

    public async Task<IContentType> EnsureAboutUsPageAsync()
    {
        IContentType siteRoot = await _helpers.EnsureSiteRootAsync(SeedingUserKey);

        IDataType textstring = await _helpers.TextstringDataTypeAsync(SeedingUserKey);
        IDataType heroImagePicker = await _helpers.MediaPickerDataTypeAsync(SeedingUserKey, "Media Picker (single image)", multiple: false);
        IDataType richText = await _helpers.RichTextDataTypeAsync(SeedingUserKey);

        IContentType statItem = await _helpers.EnsureStatItemElementTypeAsync(SeedingUserKey);
        IDataType statsDataType = await _helpers.EnsureBlockListDataTypeAsync(SeedingUserKey, "Stat Item List", [statItem]);

        IContentType aboutUsPage = await _helpers.EnsureContentTypeAsync(
            SeedingUserKey,
            alias: "aboutUsPage",
            name: "About Us",
            icon: "icon-info-circle",
            variation: ContentVariation.Culture,
            properties:
            [
                new ContentTypeHelpers.PropertyDef("heroText", "Hero Text", textstring, Variation: ContentVariation.Culture),
                new ContentTypeHelpers.PropertyDef("heroImage", "Hero Image", heroImagePicker, Variation: ContentVariation.Nothing),
                new ContentTypeHelpers.PropertyDef("body", "Body", richText, Mandatory: true, Variation: ContentVariation.Culture),
                new ContentTypeHelpers.PropertyDef("stats", "Stats", statsDataType, Variation: ContentVariation.Culture),
            ]);

        await _helpers.AllowChildAsync(SeedingUserKey, siteRoot, aboutUsPage);

        return aboutUsPage;
    }
}

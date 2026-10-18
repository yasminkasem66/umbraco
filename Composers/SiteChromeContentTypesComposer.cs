using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;

namespace UmbracoDemo.Composers;

public class SiteChromeContentTypesComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<SiteChromeContentTypesInstaller>();
    }
}

/// <summary>
/// Site Root (data-model.md "Site Root" — Foundational, shared by every user story) and
/// Site Settings (data-model.md "Site Settings" — User Story 4). Called directly by the
/// seeding composer's single startup handler — see StarterContentSeedingComposer.cs.
/// </summary>
public class SiteChromeContentTypesInstaller
{
    private static readonly Guid SeedingUserKey = Constants.Security.SuperUserKey;

    private readonly ContentTypeHelpers _helpers;

    public SiteChromeContentTypesInstaller(ContentTypeHelpers helpers)
    {
        _helpers = helpers;
    }

    public async Task<IContentType> EnsureSiteSettingsAsync()
    {
        IContentType siteRoot = await _helpers.EnsureSiteRootAsync(SeedingUserKey);

        IDataType logoDataType = await _helpers.MediaPickerDataTypeAsync(SeedingUserKey, "Media Picker (single image)", multiple: false);

        IContentType link = await _helpers.EnsureLinkElementTypeAsync(SeedingUserKey);
        IDataType primaryNavigationDataType = await _helpers.EnsureBlockListDataTypeAsync(SeedingUserKey, "Link List", [link]);

        IDataType richText = await _helpers.RichTextDataTypeAsync(SeedingUserKey);

        IContentType footerLinkColumn = await _helpers.EnsureFooterLinkColumnElementTypeAsync(SeedingUserKey);
        IDataType footerLinkColumnsDataType = await _helpers.EnsureBlockListDataTypeAsync(SeedingUserKey, "Footer Link Columns", [footerLinkColumn]);

        IContentType socialLink = await _helpers.EnsureSocialLinkElementTypeAsync(SeedingUserKey);
        IDataType socialLinksDataType = await _helpers.EnsureBlockListDataTypeAsync(SeedingUserKey, "Social Link List", [socialLink]);

        IDataType textstring = await _helpers.TextstringDataTypeAsync(SeedingUserKey);

        IContentType siteSettings = await _helpers.EnsureContentTypeAsync(
            SeedingUserKey,
            alias: "siteSettings",
            name: "Site Settings",
            icon: "icon-settings",
            variation: ContentVariation.Culture,
            properties:
            [
                new ContentTypeHelpers.PropertyDef("logo", "Logo", logoDataType, Mandatory: true, Variation: ContentVariation.Nothing),
                new ContentTypeHelpers.PropertyDef("primaryNavigation", "Primary Navigation", primaryNavigationDataType, Variation: ContentVariation.Culture),
                new ContentTypeHelpers.PropertyDef("footerAboutText", "Footer About Text", richText, Variation: ContentVariation.Culture),
                new ContentTypeHelpers.PropertyDef("footerLinkColumns", "Footer Link Columns", footerLinkColumnsDataType, Variation: ContentVariation.Culture),
                new ContentTypeHelpers.PropertyDef("socialLinks", "Social Links", socialLinksDataType, Variation: ContentVariation.Nothing),
                new ContentTypeHelpers.PropertyDef("copyrightText", "Copyright Text", textstring, Variation: ContentVariation.Culture),
            ]);

        await _helpers.AllowChildAsync(SeedingUserKey, siteRoot, siteSettings);

        return siteSettings;
    }
}

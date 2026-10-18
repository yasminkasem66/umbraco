using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;

namespace UmbracoDemo.Composers;

public class ContactUsContentTypesComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<ContactUsContentTypesInstaller>();
    }
}

/// <summary>
/// Contact Us, Contact Submissions container, and Contact Submission (data-model.md) — User
/// Story 6 (static info) and User Story 7 (submission). Called directly by the seeding
/// composer's single startup handler — see StarterContentSeedingComposer.cs — and by
/// ContactController (Controllers/ContactController.cs) to locate the submissions container.
/// </summary>
public class ContactUsContentTypesInstaller
{
    public static readonly Guid SeedingUserKey = Constants.Security.SuperUserKey;

    private readonly ContentTypeHelpers _helpers;

    public ContactUsContentTypesInstaller(ContentTypeHelpers helpers)
    {
        _helpers = helpers;
    }

    public async Task<IContentType> EnsureContactUsPageAsync()
    {
        IContentType siteRoot = await _helpers.EnsureSiteRootAsync(SeedingUserKey);

        IDataType textstring = await _helpers.TextstringDataTypeAsync(SeedingUserKey);

        IContentType socialLink = await _helpers.EnsureSocialLinkElementTypeAsync(SeedingUserKey);
        IDataType socialLinksDataType = await _helpers.EnsureBlockListDataTypeAsync(SeedingUserKey, "Social Link List", [socialLink]);

        IContentType contactUsPage = await _helpers.EnsureContentTypeAsync(
            SeedingUserKey,
            alias: "contactUsPage",
            name: "Contact Us",
            icon: "icon-message",
            variation: ContentVariation.Culture,
            properties:
            [
                new ContentTypeHelpers.PropertyDef("intro", "Intro", textstring, Variation: ContentVariation.Culture),
                new ContentTypeHelpers.PropertyDef("address", "Address", textstring, Variation: ContentVariation.Culture),
                new ContentTypeHelpers.PropertyDef("phone", "Phone", textstring, Variation: ContentVariation.Nothing),
                new ContentTypeHelpers.PropertyDef("email", "Email", textstring, Variation: ContentVariation.Nothing),
                new ContentTypeHelpers.PropertyDef("officeHours", "Office Hours", textstring, Variation: ContentVariation.Culture),
                new ContentTypeHelpers.PropertyDef("mapCoordinates", "Map Coordinates", textstring, Variation: ContentVariation.Nothing),
                new ContentTypeHelpers.PropertyDef("socialLinks", "Social Links", socialLinksDataType, Variation: ContentVariation.Nothing),
            ]);

        await _helpers.AllowChildAsync(SeedingUserKey, siteRoot, contactUsPage);

        return contactUsPage;
    }

    public async Task<IContentType> EnsureContactSubmissionsContainerAsync()
    {
        IContentType contactUsPage = await EnsureContactUsPageAsync();

        IContentType container = await _helpers.EnsureContentTypeAsync(
            SeedingUserKey,
            alias: "contactSubmissionsContainer",
            name: "Contact Submissions",
            icon: "icon-mailbox",
            variation: ContentVariation.Nothing,
            properties: []);

        await _helpers.AllowChildAsync(SeedingUserKey, contactUsPage, container);

        return container;
    }

    public async Task<IContentType> EnsureContactSubmissionAsync()
    {
        IContentType container = await EnsureContactSubmissionsContainerAsync();

        IDataType textstring = await _helpers.TextstringDataTypeAsync(SeedingUserKey);
        IDataType textarea = await _helpers.TextareaDataTypeAsync(SeedingUserKey);

        IContentType contactSubmission = await _helpers.EnsureContentTypeAsync(
            SeedingUserKey,
            alias: "contactSubmission",
            name: "Contact Submission",
            icon: "icon-envelope",
            variation: ContentVariation.Nothing,
            properties:
            [
                new ContentTypeHelpers.PropertyDef("email", "Email", textstring, Mandatory: true, ValidationRegExp: @"^[^@\s]+@[^@\s]+\.[^@\s]+$", ValidationRegExpMessage: "A valid email address is required.", Variation: ContentVariation.Nothing),
                new ContentTypeHelpers.PropertyDef("subject", "Subject", textstring, Mandatory: true, ValidationRegExp: "^.{3,150}$", ValidationRegExpMessage: "Subject must be 3-150 characters.", Variation: ContentVariation.Nothing),
                new ContentTypeHelpers.PropertyDef("message", "Message", textarea, Mandatory: true, ValidationRegExp: "^.{10,2000}$", ValidationRegExpMessage: "Message must be 10-2000 characters.", Variation: ContentVariation.Nothing),
            ]);

        await _helpers.AllowChildAsync(SeedingUserKey, container, contactSubmission);

        return contactSubmission;
    }
}

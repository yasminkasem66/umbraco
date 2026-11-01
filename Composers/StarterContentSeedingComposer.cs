using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using static UmbracoDemo.Composers.StarterContentData;

namespace UmbracoDemo.Composers;

public class StarterContentSeedingComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationAsyncHandler<UmbracoApplicationStartingNotification, StarterContentSeedingHandler>();
    }
}

/// <summary>
/// The single startup entry point for this feature's content architecture (research.md §1):
/// ensures languages, then every Document Type (in dependency order, via the installer classes
/// from the other Composers/*.cs files), then seeds the starter content in StarterContentData.cs.
/// Deliberately the *only* <see cref="UmbracoApplicationStartingNotification"/> handler in this
/// feature, so setup always runs in a guaranteed order instead of depending on notification-handler
/// registration order.
/// </summary>
/// <remarks>
/// Each node is created if missing, and also re-populated if it still holds the placeholder copy
/// an earlier version of this seeder wrote (detected via the Legacy* markers below) — so existing
/// databases pick up the real starter content without losing node keys, while anything an editor
/// has changed since is left alone.
/// </remarks>
public class StarterContentSeedingHandler : INotificationAsyncHandler<UmbracoApplicationStartingNotification>
{
    private static readonly Guid SeedingUserKey = Constants.Security.SuperUserKey;
    private const int SeedingUserId = Constants.Security.SuperUserId;

    private const string LegacyArticleSummaryMarker = "a demo summary long enough";
    private const string LegacyCategoryColour = "#2563eb";
    private const string LegacySiteSettingsMarker = "headless Umbraco showcase site";
    private const string LegacyAboutUsMarker = "showcase a fully headless Umbraco";
    private const string LegacyContactEmail = "hello@newsportaldemo.test";

    private readonly LanguageSeedingInstaller _languages;
    private readonly SiteChromeContentTypesInstaller _siteChrome;
    private readonly NewsContentTypesInstaller _news;
    private readonly AboutUsContentTypesInstaller _aboutUs;
    private readonly ContactUsContentTypesInstaller _contactUs;
    private readonly ContentTypeHelpers _helpers;
    private readonly MediaSeedingHelper _media;
    private readonly BlockListValueBuilder _blockLists;
    private readonly IContentService _contentService;
    private readonly IContentTypeService _contentTypeService;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<StarterContentSeedingHandler> _logger;

    public StarterContentSeedingHandler(
        LanguageSeedingInstaller languages,
        SiteChromeContentTypesInstaller siteChrome,
        NewsContentTypesInstaller news,
        AboutUsContentTypesInstaller aboutUs,
        ContactUsContentTypesInstaller contactUs,
        ContentTypeHelpers helpers,
        MediaSeedingHelper media,
        BlockListValueBuilder blockLists,
        IContentService contentService,
        IContentTypeService contentTypeService,
        IJsonSerializer jsonSerializer,
        ILogger<StarterContentSeedingHandler> logger)
    {
        _languages = languages;
        _siteChrome = siteChrome;
        _news = news;
        _aboutUs = aboutUs;
        _contactUs = contactUs;
        _helpers = helpers;
        _media = media;
        _blockLists = blockLists;
        _contentService = contentService;
        _contentTypeService = contentTypeService;
        _jsonSerializer = jsonSerializer;
        _logger = logger;
    }

    public async Task HandleAsync(UmbracoApplicationStartingNotification notification, CancellationToken cancellationToken)
    {
        await _languages.EnsureArabicAsync(SeedingUserKey);
        string en = (await _languages.GetDefaultLanguageAsync()).IsoCode;
        const string ar = "ar";

        IContentType siteRootType = await _helpers.EnsureSiteRootAsync(SeedingUserKey);
        IContentType siteSettingsType = await _siteChrome.EnsureSiteSettingsAsync();
        IContentType newsCategoryType = await _news.EnsureNewsCategoryAsync();
        IContentType newsArticleType = await _news.EnsureNewsArticleAsync();
        IContentType aboutUsType = await _aboutUs.EnsureAboutUsPageAsync();
        IContentType contactUsType = await _contactUs.EnsureContactUsPageAsync();
        IContentType contactSubmissionsContainerType = await _contactUs.EnsureContactSubmissionsContainerAsync();
        await _contactUs.EnsureContactSubmissionAsync();

        IContent siteRoot = EnsureSingletonAtRoot(siteRootType.Alias, "Site Root");
        if (siteRoot.Published is false)
        {
            Publish(siteRoot, ["*"]);
        }

        SeedSiteSettings(siteRoot, siteSettingsType, en, ar);
        (IContent newsFolder, IReadOnlyDictionary<string, IContent> categories) = SeedNewsCategories(siteRoot, newsCategoryType, en, ar);
        SeedNewsArticles(newsFolder, categories, newsArticleType, en, ar);
        SeedAboutUs(siteRoot, aboutUsType, en, ar);
        IContent contactUs = SeedContactUs(siteRoot, contactUsType, en, ar);

        (IContent submissionsContainer, bool containerIsNew) = EnsureSingletonChild(contactUs, contactSubmissionsContainerType.Alias, "Contact Submissions");
        if (containerIsNew)
        {
            _contentService.Save(submissionsContainer, SeedingUserId);
        }
    }

    // --- Site Settings (User Story 4) ---------------------------------------------------------

    private void SeedSiteSettings(IContent siteRoot, IContentType siteSettingsType, string en, string ar)
    {
        (IContent siteSettings, bool isNew) = EnsureSingletonChild(siteRoot, siteSettingsType.Alias, "Site Settings");
        if (isNew is false && ContainsMarker(siteSettings, "footerAboutText", en, LegacySiteSettingsMarker) is false)
        {
            return;
        }

        IMedia logo = _media.EnsureImage("Delta News Logo", "delta-news-logo.png");
        siteSettings.SetValue("logo", ToDocumentUdi(logo));

        IContentType link = GetElementType("link");
        IContentType footerLinkColumn = GetElementType("footerLinkColumn");
        IContentType socialLink = GetElementType("socialLink");

        foreach ((string culture, SiteSettings.Localized copy) in new[] { (en, SiteSettings.En), (ar, SiteSettings.Ar) })
        {
            siteSettings.SetCultureName(copy.Name, culture);

            siteSettings.SetValue("primaryNavigation", BuildLinks(link, copy.Navigation), culture);
            siteSettings.SetValue("footerAboutText", BuildRichTextValue(copy.FooterAboutHtml), culture);

            (string Label, string Url)[] sectionLinks = Categories
                .Select(c => (culture == en ? c.En : c.Ar, $"/news?category={c.Slug}"))
                .ToArray();

            string footerColumns = _blockLists.Build(footerLinkColumn,
            [
                new Dictionary<string, object> { ["columnTitle"] = copy.SectionsColumnTitle, ["links"] = BuildLinks(link, sectionLinks) },
                new Dictionary<string, object> { ["columnTitle"] = copy.CompanyColumnTitle, ["links"] = BuildLinks(link, copy.CompanyLinks) },
            ]);
            siteSettings.SetValue("footerLinkColumns", footerColumns, culture);

            siteSettings.SetValue("copyrightText", copy.Copyright, culture);
        }

        siteSettings.SetValue("socialLinks", BuildSocialLinks(socialLink, SiteSettings.SocialLinks));

        _contentService.Save(siteSettings, SeedingUserId);
        Publish(siteSettings, [en, ar]);
    }

    // --- News Categories (User Story 1 / 4) ---------------------------------------------------

    private (IContent NewsFolder, IReadOnlyDictionary<string, IContent> Categories) SeedNewsCategories(
        IContent siteRoot, IContentType newsCategoryType, string en, string ar)
    {
        (IContent newsFolder, bool newsFolderIsNew) = EnsureSingletonChild(siteRoot, "newsFolder", "News");
        if (newsFolderIsNew)
        {
            _contentService.Save(newsFolder, SeedingUserId);
        }

        if (newsFolder.Published is false)
        {
            Publish(newsFolder, ["*"]);
        }

        var result = new Dictionary<string, IContent>();

        foreach (CategorySeed seed in Categories)
        {
            (IContent category, bool isNew) = EnsureSingletonChild(newsFolder, newsCategoryType.Alias, seed.En, matchByEnglishName: seed.En);

            // Earlier versions of this seeder gave every category the same placeholder colour.
            bool hasLegacyColour = category.GetValue<string>("colour") == LegacyCategoryColour && seed.Colour != LegacyCategoryColour;

            if (isNew || hasLegacyColour)
            {
                category.SetCultureName(seed.En, en);
                category.SetCultureName(seed.Ar, ar);
                category.SetValue("colour", seed.Colour);

                _contentService.Save(category, SeedingUserId);
                Publish(category, [en, ar]);
            }

            result[seed.En] = category;
        }

        return (newsFolder, result);
    }

    // --- News Articles (User Stories 1-3) ------------------------------------------------------

    private void SeedNewsArticles(IContent newsFolder, IReadOnlyDictionary<string, IContent> categories, IContentType newsArticleType, string en, string ar)
    {
        Dictionary<string, IContent> existingArticles = _contentService.GetPagedChildren(newsFolder.Id, 0, 1000, out _)
            .Where(c => c.ContentType.Alias == newsArticleType.Alias && c.Name is not null)
            .GroupBy(c => c.Name!)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (ArticleSeed seed in Articles)
        {
            if (existingArticles.TryGetValue(seed.En.Title, out IContent? article))
            {
                if (ContainsMarker(article, "summary", en, LegacyArticleSummaryMarker) is false)
                {
                    continue;
                }
            }
            else
            {
                article = _contentService.Create(seed.En.Title, newsFolder, newsArticleType.Alias, SeedingUserId);
            }

            IMedia featuredImage = _media.EnsureImage(seed.En.Title, seed.Image);
            IMedia[] gallery = seed.Gallery
                .Select((fileName, i) => _media.EnsureImage($"{seed.En.Title} ({i + 1})", fileName))
                .ToArray();

            article.SetValue("category", ToDocumentUdi(categories[seed.Category]));
            article.SetValue("featuredImage", ToDocumentUdi(featuredImage));
            article.SetValue("gallery", string.Join(",", gallery.Select(ToDocumentUdi)));
            article.SetValue("publishedDate", DateTime.UtcNow.Date.AddDays(-seed.DaysAgo).AddHours(9));
            article.SetValue("isFeatured", seed.Featured ? 1 : 0);
            article.SetValue("featuredOrder", seed.FeaturedOrder);

            SetArticleCulture(article, seed.En, seed.Slug, en);

            var culturesToPublish = new List<string> { en };

            if (seed.Ar is not null)
            {
                SetArticleCulture(article, seed.Ar, $"{seed.Slug}-ar", ar);
                culturesToPublish.Add(ar);
            }

            _contentService.Save(article, SeedingUserId);
            Publish(article, culturesToPublish.ToArray());
        }
    }

    private void SetArticleCulture(IContent article, LocalizedArticle copy, string slug, string culture)
    {
        article.SetCultureName(copy.Title, culture);
        article.SetValue("urlSlug", slug, culture);
        article.SetValue("summary", copy.Summary, culture);
        article.SetValue("body", BuildRichTextValue(copy.BodyHtml), culture);
        article.SetValue("author", copy.Author, culture);
        article.SetValue("tags", _jsonSerializer.Serialize(copy.Tags), culture);
    }

    // --- About Us (User Story 5) ---------------------------------------------------------------

    private void SeedAboutUs(IContent siteRoot, IContentType aboutUsType, string en, string ar)
    {
        (IContent aboutUs, bool isNew) = EnsureSingletonChild(siteRoot, aboutUsType.Alias, "About Us");
        if (isNew is false && ContainsMarker(aboutUs, "body", en, LegacyAboutUsMarker) is false)
        {
            return;
        }

        IMedia hero = _media.EnsureImage("About Us Newsroom", AboutUs.HeroImage);
        aboutUs.SetValue("heroImage", ToDocumentUdi(hero));

        IContentType statItem = GetElementType("statItem");

        foreach ((string culture, AboutUs.Localized copy) in new[] { (en, AboutUs.En), (ar, AboutUs.Ar) })
        {
            aboutUs.SetCultureName(copy.Title, culture);
            aboutUs.SetValue("heroText", copy.HeroText, culture);
            aboutUs.SetValue("body", BuildRichTextValue(copy.BodyHtml), culture);

            string stats = _blockLists.Build(statItem, copy.Stats
                .Select(s => (IReadOnlyDictionary<string, object>)new Dictionary<string, object> { ["label"] = s.Label, ["value"] = s.Value })
                .ToList());
            aboutUs.SetValue("stats", stats, culture);
        }

        _contentService.Save(aboutUs, SeedingUserId);
        Publish(aboutUs, [en, ar]);
    }

    // --- Contact Us (User Story 6) --------------------------------------------------------------

    private IContent SeedContactUs(IContent siteRoot, IContentType contactUsType, string en, string ar)
    {
        (IContent contactUs, bool isNew) = EnsureSingletonChild(siteRoot, contactUsType.Alias, "Contact Us");
        if (isNew is false && contactUs.GetValue<string>("email") != LegacyContactEmail)
        {
            return contactUs;
        }

        contactUs.SetValue("phone", ContactUs.Phone);
        contactUs.SetValue("email", ContactUs.Email);
        contactUs.SetValue("mapCoordinates", ContactUs.MapCoordinates);
        contactUs.SetValue("socialLinks", BuildSocialLinks(GetElementType("socialLink"), SiteSettings.SocialLinks));

        foreach ((string culture, ContactUs.Localized copy) in new[] { (en, ContactUs.En), (ar, ContactUs.Ar) })
        {
            contactUs.SetCultureName(copy.Title, culture);
            contactUs.SetValue("intro", copy.Intro, culture);
            contactUs.SetValue("address", copy.Address, culture);
            contactUs.SetValue("officeHours", copy.OfficeHours, culture);
        }

        _contentService.Save(contactUs, SeedingUserId);
        Publish(contactUs, [en, ar]);

        return contactUs;
    }

    // --- Shared plumbing -------------------------------------------------------------------------

    private IContent EnsureSingletonAtRoot(string contentTypeAlias, string name)
    {
        IContent? existing = _contentService.GetRootContent().FirstOrDefault(c => c.ContentType.Alias == contentTypeAlias);
        if (existing is not null)
        {
            return existing;
        }

        IContent content = _contentService.Create(name, Constants.System.Root, contentTypeAlias, SeedingUserId);
        _contentService.Save(content, SeedingUserId);
        return content;
    }

    /// <summary>
    /// Returns the existing singleton child if one exists (so every Seed* method above can decide
    /// whether to re-populate it), otherwise creates a new, empty shell ready for the caller to
    /// populate, save, and publish. <paramref name="matchByEnglishName"/> narrows the "does it
    /// already exist" check for content types with more than one seeded instance (News Category).
    /// </summary>
    private (IContent Content, bool IsNew) EnsureSingletonChild(IContent parent, string contentTypeAlias, string name, string? matchByEnglishName = null)
    {
        IEnumerable<IContent> children = _contentService.GetPagedChildren(parent.Id, 0, 500, out _);
        IContent? existing = matchByEnglishName is null
            ? children.FirstOrDefault(c => c.ContentType.Alias == contentTypeAlias)
            : children.FirstOrDefault(c => c.ContentType.Alias == contentTypeAlias && c.Name == matchByEnglishName);

        if (existing is not null)
        {
            return (existing, false);
        }

        IContent content = _contentService.Create(name, parent, contentTypeAlias, SeedingUserId);
        return (content, true);
    }

    private static bool ContainsMarker(IContent content, string propertyAlias, string culture, string marker) =>
        content.GetValue<string>(propertyAlias, culture)?.Contains(marker, StringComparison.Ordinal) == true;

    private void Publish(IContent content, string[] cultures)
    {
        PublishResult result = _contentService.Publish(content, cultures, SeedingUserId);
        if (result.Success is false)
        {
            _logger.LogWarning(
                "Starter content '{Name}' could not be published ({Result}). Invalid properties: {Properties}",
                content.Name,
                result.Result,
                string.Join(", ", result.InvalidProperties?.Select(p => p.Alias) ?? []));
        }
    }

    private string BuildLinks(IContentType linkType, IEnumerable<(string Label, string Url)> links) =>
        _blockLists.Build(linkType, links
            .Select(l => (IReadOnlyDictionary<string, object>)new Dictionary<string, object> { ["label"] = l.Label, ["url"] = l.Url, ["openInNewTab"] = false })
            .ToList());

    private string BuildSocialLinks(IContentType socialLinkType, IEnumerable<(string Platform, string Url)> links) =>
        _blockLists.Build(socialLinkType, links
            .Select(l => (IReadOnlyDictionary<string, object>)new Dictionary<string, object> { ["platform"] = l.Platform, ["url"] = l.Url })
            .ToList());

    private string BuildRichTextValue(string html)
    {
        var value = new
        {
            markup = html,
            blocks = new
            {
                layout = new Dictionary<string, object>(),
                contentData = Array.Empty<object>(),
                settingsData = Array.Empty<object>(),
                expose = Array.Empty<object>(),
            },
        };

        return _jsonSerializer.Serialize(value);
    }

    private IContentType GetElementType(string alias) =>
        _contentTypeService.Get(alias) ?? throw new InvalidOperationException($"Element type '{alias}' has not been created yet.");

    private static string ToDocumentUdi(IContent content) => new GuidUdi(Constants.UdiEntityType.Document, content.Key).ToString();

    private static string ToDocumentUdi(IMedia media) => new GuidUdi(Constants.UdiEntityType.Media, media.Key).ToString();
}

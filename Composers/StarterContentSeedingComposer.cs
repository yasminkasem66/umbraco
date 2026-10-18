using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

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
/// from the other Composers/*.cs files), then seeds demo content. Deliberately the *only*
/// <see cref="UmbracoApplicationStartingNotification"/> handler in this feature, so setup always
/// runs in a guaranteed order instead of depending on notification-handler registration order.
/// </summary>
public class StarterContentSeedingHandler : INotificationAsyncHandler<UmbracoApplicationStartingNotification>
{
    private static readonly Guid SeedingUserKey = Constants.Security.SuperUserKey;
    private const int SeedingUserId = Constants.Security.SuperUserId;

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
        IJsonSerializer jsonSerializer)
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
        IReadOnlyList<IContent> categories = SeedNewsCategories(siteRoot, newsCategoryType, en, ar);
        SeedNewsArticles(categories, newsArticleType, en, ar);
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
        if (isNew is false)
        {
            return;
        }

        IMedia logo = _media.EnsurePlaceholderImage("Site Logo");
        siteSettings.SetValue("logo", ToDocumentUdi(logo));

        IContentType link = GetElementType("link");
        IContentType footerLinkColumn = GetElementType("footerLinkColumn");
        IContentType socialLink = GetElementType("socialLink");

        foreach ((string culture, string homeLabel, string aboutLabel) in new[]
                 {
                     (en, "Home", "About"),
                     (ar, "الرئيسية", "من نحن"),
                 })
        {
            siteSettings.SetCultureName(culture == en ? "Site Settings" : "إعدادات الموقع", culture);

            string navigation = _blockLists.Build(link,
            [
                new Dictionary<string, object> { ["label"] = homeLabel, ["url"] = "/", ["openInNewTab"] = false },
                new Dictionary<string, object> { ["label"] = aboutLabel, ["url"] = "/about-us", ["openInNewTab"] = false },
            ]);
            siteSettings.SetValue("primaryNavigation", navigation, culture);

            siteSettings.SetValue("footerAboutText", BuildRichTextValue(culture == en
                ? "<p>News Portal Demo is a headless Umbraco showcase site.</p>"
                : "<p>بوابة الأخبار التجريبية هي موقع عرض توضيحي بدون واجهة يعتمد على Umbraco.</p>"), culture);

            string footerColumns = _blockLists.Build(footerLinkColumn,
            [
                new Dictionary<string, object>
                {
                    ["columnTitle"] = culture == en ? "Company" : "الشركة",
                    ["links"] = _blockLists.Build(link,
                    [
                        new Dictionary<string, object> { ["label"] = aboutLabel, ["url"] = "/about-us", ["openInNewTab"] = false },
                    ]),
                },
            ]);
            siteSettings.SetValue("footerLinkColumns", footerColumns, culture);

            siteSettings.SetValue("copyrightText", culture == en
                ? "(c) News Portal Demo"
                : "(c) بوابة الأخبار التجريبية", culture);
        }

        string socialLinks = _blockLists.Build(socialLink,
        [
            new Dictionary<string, object> { ["platform"] = "Twitter/X", ["url"] = "https://x.com/example" },
            new Dictionary<string, object> { ["platform"] = "Facebook", ["url"] = "https://facebook.com/example" },
        ]);
        siteSettings.SetValue("socialLinks", socialLinks);

        _contentService.Save(siteSettings, SeedingUserId);
        Publish(siteSettings, [en, ar]);
    }

    // --- News Categories (User Story 1 / 4) ---------------------------------------------------

    private IReadOnlyList<IContent> SeedNewsCategories(IContent siteRoot, IContentType newsCategoryType, string en, string ar)
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

        (string enName, string arName)[] seeds =
        [
            ("Technology", "تقنية"),
            ("Sports", "رياضة"),
            ("Business", "أعمال"),
        ];

        var result = new List<IContent>();

        foreach ((string enName, string arName) in seeds)
        {
            (IContent category, bool isNew) = EnsureSingletonChild(newsFolder, newsCategoryType.Alias, enName, matchByEnglishName: enName);

            if (isNew)
            {
                category.SetCultureName(enName, en);
                category.SetCultureName(arName, ar);
                category.SetValue("colour", "#2563eb");

                _contentService.Save(category, SeedingUserId);
                Publish(category, [en, ar]);
            }

            result.Add(category);
        }

        return result;
    }

    // --- News Articles (User Stories 1-3) ------------------------------------------------------

    private void SeedNewsArticles(IReadOnlyList<IContent> categories, IContentType newsArticleType, string en, string ar)
    {
        IContent newsFolder = _contentService.GetPagedChildren(
                _contentService.GetRootContent().First(c => c.ContentType.Alias == "siteRoot").Id, 0, 50, out _)
            .First(c => c.ContentType.Alias == "newsFolder");

        if (_contentService.CountChildren(newsFolder.Id, newsArticleType.Alias) > 0)
        {
            return;
        }

        IMedia articleImage = _media.EnsurePlaceholderImage("Article Photo");

        var articles = new[]
        {
            new { Title = "City Unveils New Tech Hub", Category = 0, Featured = true, Order = 1, Bilingual = true },
            new { Title = "Local Team Wins Championship", Category = 1, Featured = true, Order = 2, Bilingual = true },
            new { Title = "Startup Raises Seed Funding", Category = 2, Featured = true, Order = 3, Bilingual = true },
            new { Title = "AI Adoption Grows Among Small Businesses", Category = 0, Featured = false, Order = 0, Bilingual = true },
            new { Title = "Marathon Draws Record Crowds", Category = 1, Featured = false, Order = 0, Bilingual = true },
            new { Title = "Regional Bank Reports Strong Quarter", Category = 2, Featured = false, Order = 0, Bilingual = false },
            new { Title = "New Data Centre Opens Downtown", Category = 0, Featured = false, Order = 0, Bilingual = false },
            new { Title = "Youth League Expands to Two New Cities", Category = 1, Featured = false, Order = 0, Bilingual = false },
        };

        int i = 0;
        foreach (var a in articles)
        {
            i++;
            IContent article = _contentService.Create(a.Title, newsFolder, newsArticleType.Alias, SeedingUserId);

            string slug = a.Title.ToLowerInvariant().Replace(" ", "-").Replace(",", string.Empty);

            article.SetCultureName(a.Title, en);
            article.SetValue("urlSlug", slug, en);
            article.SetValue("summary", $"{a.Title} - a demo summary long enough to satisfy the fifty to three hundred character minimum required for News Article summaries in this showcase site.", en);
            article.SetValue("body", BuildRichTextValue($"<p>{a.Title}. This is placeholder body copy for the News Portal Demo, seeded automatically so every endpoint returns real data.</p>"), en);
            article.SetValue("author", "Newsroom Staff", en);
            article.SetValue("tags", _jsonSerializer.Serialize(new[] { "demo", "news" }), en);

            article.SetValue("category", ToDocumentUdi(categories[a.Category]));
            article.SetValue("featuredImage", ToDocumentUdi(articleImage));
            article.SetValue("publishedDate", DateTime.UtcNow.AddDays(-i));
            article.SetValue("isFeatured", a.Featured ? 1 : 0);
            article.SetValue("featuredOrder", a.Order);

            var culturesToPublish = new List<string> { en };

            if (a.Bilingual)
            {
                string arTitle = $"{a.Title} (عربي)";
                article.SetCultureName(arTitle, ar);
                article.SetValue("urlSlug", $"{slug}-ar", ar);
                article.SetValue("summary", "هذا ملخص تجريبي باللغة العربية لهذا المقال ضمن بوابة الأخبار التجريبية، وهو طويل بما يكفي لتجاوز الحد الأدنى للأحرف المطلوب.", ar);
                article.SetValue("body", BuildRichTextValue("<p>هذا محتوى تجريبي باللغة العربية لهذا المقال في بوابة الأخبار التجريبية.</p>"), ar);
                article.SetValue("author", "فريق التحرير", ar);
                article.SetValue("tags", _jsonSerializer.Serialize(new[] { "demo", "أخبار" }), ar);

                culturesToPublish.Add(ar);
            }

            _contentService.Save(article, SeedingUserId);
            Publish(article, culturesToPublish.ToArray());
        }
    }

    // --- About Us (User Story 5) ---------------------------------------------------------------

    private void SeedAboutUs(IContent siteRoot, IContentType aboutUsType, string en, string ar)
    {
        (IContent aboutUs, bool isNew) = EnsureSingletonChild(siteRoot, aboutUsType.Alias, "About Us");
        if (isNew is false)
        {
            return;
        }

        IMedia hero = _media.EnsurePlaceholderImage("About Us Hero");
        aboutUs.SetValue("heroImage", ToDocumentUdi(hero));

        IContentType statItem = GetElementType("statItem");

        foreach ((string culture, string title, string heroText, string body) in new[]
                 {
                     (en, "About Us", "Who we are", "<p>News Portal Demo exists to showcase a fully headless Umbraco content architecture.</p>"),
                     (ar, "من نحن", "من نحن", "<p>تهدف بوابة الأخبار التجريبية إلى عرض بنية محتوى Umbraco بدون واجهة بالكامل.</p>"),
                 })
        {
            aboutUs.SetCultureName(title, culture);
            aboutUs.SetValue("heroText", heroText, culture);
            aboutUs.SetValue("body", BuildRichTextValue(body), culture);

            string stats = _blockLists.Build(statItem,
            [
                new Dictionary<string, object> { ["label"] = culture == en ? "Articles" : "المقالات", ["value"] = "8+" },
                new Dictionary<string, object> { ["label"] = culture == en ? "Categories" : "الفئات", ["value"] = "3" },
            ]);
            aboutUs.SetValue("stats", stats, culture);
        }

        _contentService.Save(aboutUs, SeedingUserId);
        Publish(aboutUs, [en, ar]);
    }

    // --- Contact Us (User Story 6) --------------------------------------------------------------

    private IContent SeedContactUs(IContent siteRoot, IContentType contactUsType, string en, string ar)
    {
        (IContent contactUs, bool isNew) = EnsureSingletonChild(siteRoot, contactUsType.Alias, "Contact Us");
        if (isNew is false)
        {
            return contactUs;
        }

        contactUs.SetValue("phone", "+1-555-0100");
        contactUs.SetValue("email", "hello@newsportaldemo.test");
        contactUs.SetValue("mapCoordinates", "30.0444,31.2357");

        IContentType socialLinkType = GetElementType("socialLink");
        string socialLinks = _blockLists.Build(socialLinkType,
        [
            new Dictionary<string, object> { ["platform"] = "Twitter/X", ["url"] = "https://x.com/example" },
        ]);
        contactUs.SetValue("socialLinks", socialLinks);

        foreach ((string culture, string title, string intro, string address, string hours) in new[]
                 {
                     (en, "Contact Us", "We would love to hear from you.", "123 Demo Street, Cairo", "Sun-Thu, 9am-5pm"),
                     (ar, "اتصل بنا", "يسعدنا التواصل معك.", "١٢٣ شارع تجريبي، القاهرة", "الأحد-الخميس، ٩ص-٥م"),
                 })
        {
            contactUs.SetCultureName(title, culture);
            contactUs.SetValue("intro", intro, culture);
            contactUs.SetValue("address", address, culture);
            contactUs.SetValue("officeHours", hours, culture);
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
    /// Returns the existing singleton child if one exists (so every Seed* method above can skip
    /// re-populating it), otherwise creates a new, empty shell ready for the caller to populate,
    /// save, and publish. <paramref name="matchByEnglishName"/> narrows the "does it already
    /// exist" check for content types with more than one seeded instance (News Category).
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

    private void Publish(IContent content, string[] cultures) => _contentService.Publish(content, cultures, SeedingUserId);

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

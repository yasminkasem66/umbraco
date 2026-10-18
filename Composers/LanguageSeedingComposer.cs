using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace UmbracoDemo.Composers;

public class LanguageSeedingComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<LanguageSeedingInstaller>();
    }
}

/// <summary>
/// Registers Arabic alongside the already-installed default English language (research.md §3).
/// Called directly by the seeding composer's single startup handler — see
/// StarterContentSeedingComposer.cs — rather than being its own notification handler, so
/// content-architecture setup always runs in a guaranteed order.
/// </summary>
public class LanguageSeedingInstaller
{
    private readonly ILanguageService _languageService;

    public LanguageSeedingInstaller(ILanguageService languageService)
    {
        _languageService = languageService;
    }

    public async Task<ILanguage> EnsureArabicAsync(Guid seedingUserKey)
    {
        ILanguage? arabic = await _languageService.GetAsync("ar");
        if (arabic is not null)
        {
            return arabic;
        }

        arabic = new Language("ar", "Arabic (Arabic)")
        {
            IsDefault = false,
            IsMandatory = false,
            FallbackIsoCode = null,
        };

        await _languageService.CreateAsync(arabic, seedingUserKey);

        return arabic;
    }

    public async Task<ILanguage> GetDefaultLanguageAsync()
    {
        IEnumerable<ILanguage> all = await _languageService.GetAllAsync();
        return all.First(l => l.IsDefault);
    }
}

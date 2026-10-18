using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace UmbracoDemo.Composers;

/// <summary>
/// Registers the shared content-type creation helper (ContentTypeHelpers.cs) used by every
/// content-architecture composer in this feature.
/// </summary>
public class SharedServicesComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<ContentTypeHelpers>();
        builder.Services.AddSingleton<BlockListValueBuilder>();
        builder.Services.AddSingleton<MediaSeedingHelper>();
    }
}

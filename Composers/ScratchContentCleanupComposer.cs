using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace UmbracoDemo.Composers;

public class ScratchContentCleanupComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationAsyncHandler<UmbracoApplicationStartingNotification, ScratchContentCleanupHandler>();
    }
}

/// <summary>
/// One-time removal of the pre-existing scratch/experimental Document Types and their content
/// (research.md §6): each one had, or was positioned to have, a Razor view assigned, which is a
/// standing violation of Constitution Principle I the moment this feature's own composers run in
/// the same content tree. Idempotent: once a type is gone, later runs skip it. Runs via code
/// (Umbraco's own services) rather than manual backoffice clicks, since it must be reproducible
/// from a fresh clone the same way every other content-architecture step in this feature is.
/// </summary>
public class ScratchContentCleanupHandler : INotificationAsyncHandler<UmbracoApplicationStartingNotification>
{
    private const int SeedingUserId = Constants.Security.SuperUserId;

    private static readonly string[] ScratchContentTypeAliases =
    [
        "testAgain",
        "allowContentTest",
        "collectionTest",
        "templateType",
        "rrr",
        "eee",
        "uuuu",
        "videoItem",
    ];

    private readonly IContentTypeService _contentTypeService;
    private readonly IContentService _contentService;

    public ScratchContentCleanupHandler(IContentTypeService contentTypeService, IContentService contentService)
    {
        _contentTypeService = contentTypeService;
        _contentService = contentService;
    }

    public Task HandleAsync(UmbracoApplicationStartingNotification notification, CancellationToken cancellationToken)
    {
        foreach (string alias in ScratchContentTypeAliases)
        {
            IContentType? contentType = _contentTypeService.Get(alias);
            if (contentType is null)
            {
                continue;
            }

            _contentService.DeleteOfType(contentType.Id, SeedingUserId);
            _contentTypeService.Delete(contentType, SeedingUserId);
        }

        return Task.CompletedTask;
    }
}

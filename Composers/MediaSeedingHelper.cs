using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace UmbracoDemo.Composers;

/// <summary>
/// Creates minimal placeholder image Media items so the mandatory Media Picker properties in
/// data-model.md (Featured Image, Logo, Hero Image, Category Icon) have something real to
/// reference. A tiny solid-colour PNG stands in for real demo photography.
/// </summary>
public class MediaSeedingHelper
{
    // A minimal valid 1x1 solid-colour PNG.
    private static readonly byte[] PlaceholderPng =
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
        0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53,
        0xDE, 0x00, 0x00, 0x00, 0x0C, 0x49, 0x44, 0x41, 0x54, 0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00,
        0x00, 0x03, 0x01, 0x01, 0x00, 0x18, 0xDD, 0x8D, 0xB0, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E,
        0x44, 0xAE, 0x42, 0x60, 0x82,
    ];

    private readonly IMediaService _mediaService;
    private readonly MediaFileManager _mediaFileManager;
    private readonly MediaUrlGeneratorCollection _mediaUrlGeneratorCollection;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;

    public MediaSeedingHelper(
        IMediaService mediaService,
        MediaFileManager mediaFileManager,
        MediaUrlGeneratorCollection mediaUrlGeneratorCollection,
        IShortStringHelper shortStringHelper,
        IContentTypeBaseServiceProvider contentTypeBaseServiceProvider)
    {
        _mediaService = mediaService;
        _mediaFileManager = mediaFileManager;
        _mediaUrlGeneratorCollection = mediaUrlGeneratorCollection;
        _shortStringHelper = shortStringHelper;
        _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
    }

    public IMedia EnsurePlaceholderImage(string name)
    {
        IMedia? existing = _mediaService.GetRootMedia().FirstOrDefault(m => m.Name == name);
        if (existing is not null)
        {
            return existing;
        }

        IMedia media = _mediaService.CreateMedia(name, Constants.System.Root, Constants.Conventions.MediaTypes.Image);

        using var stream = new MemoryStream(PlaceholderPng);
        media.SetValue(
            _mediaFileManager,
            _mediaUrlGeneratorCollection,
            _shortStringHelper,
            _contentTypeBaseServiceProvider,
            Constants.Conventions.Media.File,
            $"{name.ToLowerInvariant().Replace(' ', '-')}.png",
            stream);

        _mediaService.Save(media);

        return media;
    }
}

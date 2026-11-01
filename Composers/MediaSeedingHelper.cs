using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace UmbracoDemo.Composers;

/// <summary>
/// Imports the starter photos committed under SeedData/images (see CREDITS.md there) as Image
/// Media items, so the mandatory Media Picker properties in data-model.md (Featured Image, Logo,
/// Hero Image) reference real pictures. The files are copied to the build/publish output by
/// UmbracoDemo.csproj, which is why they're resolved from <see cref="AppContext.BaseDirectory"/>.
/// </summary>
public class MediaSeedingHelper
{
    private static readonly string SeedImagesDirectory = Path.Combine(AppContext.BaseDirectory, "SeedData", "images");

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

    public IMedia EnsureImage(string name, string fileName)
    {
        IMedia? existing = _mediaService.GetRootMedia().FirstOrDefault(m => m.Name == name);
        if (existing is not null)
        {
            return existing;
        }

        string path = Path.Combine(SeedImagesDirectory, fileName);
        if (System.IO.File.Exists(path) is false)
        {
            throw new FileNotFoundException($"Seed image '{fileName}' was not found. Expected it at '{path}'.", path);
        }

        IMedia media = _mediaService.CreateMedia(name, Constants.System.Root, Constants.Conventions.MediaTypes.Image);

        using FileStream stream = System.IO.File.OpenRead(path);
        media.SetValue(
            _mediaFileManager,
            _mediaUrlGeneratorCollection,
            _shortStringHelper,
            _contentTypeBaseServiceProvider,
            Constants.Conventions.Media.File,
            fileName,
            stream);

        _mediaService.Save(media);

        return media;
    }
}

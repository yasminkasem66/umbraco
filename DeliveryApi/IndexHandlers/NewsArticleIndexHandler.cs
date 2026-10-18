using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;

namespace UmbracoDemo.DeliveryApi.IndexHandlers;

/// <summary>
/// Registers newsArticle's category/tags/isFeatured/featuredOrder as filterable/sortable Delivery
/// API fields (research.md §2, data-model.md "Delivery API filter/sort registration"). Used by
/// User Story 1 (category/tag filtering) and User Story 3 (featured set).
/// </summary>
public class NewsArticleIndexHandler : IContentIndexHandler
{
    private const string NewsArticleContentTypeAlias = "newsArticle";

    private readonly IJsonSerializer _jsonSerializer;

    public NewsArticleIndexHandler(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public IEnumerable<IndexField> GetFields()
    {
        yield return new IndexField
        {
            FieldName = "category",
            FieldType = FieldType.StringRaw,
            VariesByCulture = false,
        };

        yield return new IndexField
        {
            FieldName = "tags",
            FieldType = FieldType.StringRaw,
            VariesByCulture = true,
        };

        yield return new IndexField
        {
            FieldName = "isFeatured",
            FieldType = FieldType.Number,
            VariesByCulture = false,
        };

        yield return new IndexField
        {
            FieldName = "featuredOrder",
            FieldType = FieldType.Number,
            VariesByCulture = false,
        };
    }

    public IEnumerable<IndexFieldValue> GetFieldValues(IContent content, string? culture)
    {
        if (content.ContentType.Alias != NewsArticleContentTypeAlias)
        {
            yield break;
        }

        // "category" is an invariant property (data-model.md), so it must always be read with a
        // null culture regardless of which culture this indexing pass is for — passing the
        // per-pass culture through here would look up a non-existent culture-specific value and
        // silently index nothing, which is what caused every category filter to match 0 articles.
        Guid? categoryKey = TryGetContentPickerKey(content, "category", null);
        if (categoryKey is not null)
        {
            yield return new IndexFieldValue
            {
                FieldName = "category",
                Values = [categoryKey.Value.ToString()],
            };
        }

        string[] tags = TryGetTags(content, culture);
        if (tags.Length > 0)
        {
            yield return new IndexFieldValue
            {
                FieldName = "tags",
                Values = tags.Cast<object>().ToArray(),
            };
        }

        bool isFeatured = content.GetValue<bool>("isFeatured");
        yield return new IndexFieldValue
        {
            FieldName = "isFeatured",
            Values = [isFeatured ? 1 : 0],
        };

        int featuredOrder = content.GetValue<int>("featuredOrder");
        yield return new IndexFieldValue
        {
            FieldName = "featuredOrder",
            Values = [featuredOrder],
        };
    }

    private static Guid? TryGetContentPickerKey(IContent content, string propertyAlias, string? culture)
    {
        string? raw = content.GetValue<string>(propertyAlias, culture);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return UdiParser.TryParse(raw, out Udi? udi) && udi is GuidUdi guidUdi
            ? guidUdi.Guid
            : null;
    }

    private string[] TryGetTags(IContent content, string? culture)
    {
        string? raw = content.GetValue<string>("tags", culture);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        try
        {
            return _jsonSerializer.Deserialize<string[]>(raw) ?? [];
        }
        catch
        {
            return [];
        }
    }
}

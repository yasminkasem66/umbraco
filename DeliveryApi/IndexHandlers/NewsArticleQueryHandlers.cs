using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;

namespace UmbracoDemo.DeliveryApi.IndexHandlers;

/// <summary>
/// Makes `filter=category:`, `filter=tags:`, and `filter=isFeatured:` recognized query options.
/// <see cref="NewsArticleIndexHandler"/> only adds the fields to the Delivery API's content
/// index (so they're searchable); the Delivery API query string parser separately requires an
/// <see cref="IFilterHandler"/> per field name to accept `filter=fieldName:value` syntax at all —
/// without one, every request using these fields fails with "Filter option not found" even
/// though the field itself is indexed correctly.
/// </summary>
public class NewsArticleFilterHandler : IFilterHandler
{
    private static readonly string[] FieldNames = ["category", "tags", "isFeatured"];

    public bool CanHandle(string query) =>
        FieldNames.Any(field => query.StartsWith($"{field}:", StringComparison.OrdinalIgnoreCase));

    public FilterOption BuildFilterOption(string filter)
    {
        int separatorIndex = filter.IndexOf(':');
        string fieldName = filter[..separatorIndex];
        string value = filter[(separatorIndex + 1)..];

        return new FilterOption
        {
            FieldName = fieldName,
            Values = [value],
            Operator = FilterOperation.Is,
        };
    }
}

/// <summary>Makes `sort=featuredOrder:` a recognized query option (see NewsArticleFilterHandler).</summary>
public class NewsArticleSortHandler : ISortHandler
{
    public bool CanHandle(string query) => query.StartsWith("featuredOrder:", StringComparison.OrdinalIgnoreCase);

    public SortOption BuildSortOption(string sort)
    {
        int separatorIndex = sort.IndexOf(':');
        string fieldName = sort[..separatorIndex];
        string direction = sort[(separatorIndex + 1)..];

        return new SortOption
        {
            FieldName = fieldName,
            Direction = direction.Equals("desc", StringComparison.OrdinalIgnoreCase) ? Direction.Descending : Direction.Ascending,
        };
    }
}

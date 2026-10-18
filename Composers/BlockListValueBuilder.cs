using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Serialization;

namespace UmbracoDemo.Composers;

/// <summary>
/// Builds the raw stored JSON for a Block List property value from plain alias/value pairs, so
/// the seeding composer can populate primaryNavigation / footerLinkColumns / socialLinks / stats
/// (data-model.md) without hand-writing the Block List storage format.
/// </summary>
public class BlockListValueBuilder
{
    private readonly IJsonSerializer _jsonSerializer;

    public BlockListValueBuilder(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public string Build(IContentType elementType, IReadOnlyList<IReadOnlyDictionary<string, object>> blocks)
    {
        var layoutItems = new List<BlockListLayoutItem>();
        var contentData = new List<BlockItemData>();
        var expose = new List<BlockItemVariation>();

        foreach (IReadOnlyDictionary<string, object> block in blocks)
        {
            Guid key = Guid.NewGuid();
            layoutItems.Add(new BlockListLayoutItem(key));

            var itemData = new BlockItemData(key, elementType.Key, elementType.Alias)
            {
                Values = block
                    .Select(kvp => new BlockPropertyValue
                    {
                        Alias = kvp.Key,
                        Value = kvp.Value,
                        PropertyType = elementType.PropertyTypes.First(p => p.Alias == kvp.Key),
                    })
                    .ToList(),
            };

            contentData.Add(itemData);

            // Marks the block "exposed" (culture: null = invariant within this already
            // per-culture-scoped property value blob) — without an Expose entry, the Delivery
            // API's Block List converter returns an empty items array even though contentData
            // and layout are both populated correctly.
            expose.Add(new BlockItemVariation(key, null!, null!));
        }

        var value = new BlockListValue(layoutItems)
        {
            ContentData = contentData,
            Expose = expose,
        };

        return _jsonSerializer.Serialize(value);
    }
}

using System;

namespace uguimvvm.converters
{
    /// <summary>
    /// Example class showing how to leverage the base <see cref="ItemMapping{TSource, TTarget}"/> for a specific configuration.
    /// </summary>
    [Serializable]
    public class ItemMappingImpl : ItemMapping<int, string>
    {
    }

    /// <summary>
    /// Example class showing how to leverage the base <see cref="ValueConverter{ItemMapping, TSource, TTarget}"/> for a specific configuration.
    /// </summary>
    public class ValueConverterExample : ValueConverter<ItemMappingImpl, int, string>
    {
    }
}

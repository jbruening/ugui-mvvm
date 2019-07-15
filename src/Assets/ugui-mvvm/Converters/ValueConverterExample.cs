using System;

namespace uguimvvm.converters
{
    [Serializable]
    public class ItemMappingImpl : ItemMapping<int, string>
    {
    }

    public class ValueConverterExample : ValueConverter<ItemMappingImpl, int, string>
    {
    }
}
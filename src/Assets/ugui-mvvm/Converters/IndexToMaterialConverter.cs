using UnityEngine;

namespace uguimvvm.converters
{
    /// <summary>
    /// An <see cref="IValueConverter"/> for converting from an item index to a <see cref="Material"/> within a list of possibilities.
    /// </summary>
    public class IndexToMaterialConverter : IndexToObjectConverter<Material> { }
}

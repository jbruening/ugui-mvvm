using UnityEngine;

namespace uguimvvm.converters
{
    /// <summary>
    /// An <see cref="IValueConverter"/> for converting from an item index to a <see cref="Color"/> within a list of possibilities.
    /// </summary>
    public class IndexToColorConverter : IndexToObjectConverter<Color> { }
}

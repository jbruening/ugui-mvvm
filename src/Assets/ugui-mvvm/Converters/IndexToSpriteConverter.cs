using UnityEngine;

namespace uguimvvm.converters
{
    /// <summary>
    /// An <see cref="IValueConverter"/> for converting from an item index to a <see cref="Sprite"/> within a list of possibilities.
    /// </summary>
    public class IndexToSpriteConverter : IndexToObjectConverter<Sprite> { }
}

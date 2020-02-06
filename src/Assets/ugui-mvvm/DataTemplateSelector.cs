using UnityEngine;

namespace uguimvvm
{
    /// <summary>
    /// Provides a way to choose a DataTemplate (i.e. prefab) based on the data object.
    /// </summary>
    public abstract class DataTemplateSelector : ScriptableObject, IDataTemplateSelector
    {
        /// <inheritdoc />
        public abstract GameObject SelectTemplate(object data);
    }
}


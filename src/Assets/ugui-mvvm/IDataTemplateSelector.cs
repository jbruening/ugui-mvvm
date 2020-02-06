using UnityEngine;

namespace uguimvvm
{
    /// <summary>
    /// Provides a way to choose a DataTemplate (i.e. prefab) based on the data object.
    /// </summary>
    public interface IDataTemplateSelector
    {
        /// <summary>
        /// SelectTemplate is used to decide which DataTemplate (i.e. prefab) should be used for a given data context.
        /// </summary>
        /// <param name="data">The data object for which to select the template.</param>
        /// <returns>A DataTemplate (i.e. prefab) or <c>null</c>.  The default value is <c>null</c>.</returns>
        GameObject SelectTemplate(object data);
    }
}


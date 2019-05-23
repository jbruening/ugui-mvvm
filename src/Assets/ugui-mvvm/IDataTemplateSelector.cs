using UnityEngine;

namespace uguimvvm
{
    public interface IDataTemplateSelector
    {
        /// <summary>
        /// SelectTemplate is used to decide which prefab should be used for a given data context.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        GameObject SelectTemplate(object data);
    }
}


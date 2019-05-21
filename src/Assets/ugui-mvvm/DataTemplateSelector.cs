using UnityEngine;

namespace uguimvvm
{
    public abstract class DataTemplateSelector : ScriptableObject, IDataTemplateSelector
    {
        public abstract GameObject SelectTemplate(object data);
    }
}


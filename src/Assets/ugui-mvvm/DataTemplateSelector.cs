using UnityEngine;
// MRMW Change Start - Add DataTemplateSelector
namespace uguimvvm
{
    public abstract class DataTemplateSelector : ScriptableObject, IDataTemplateSelector
    {
        public abstract GameObject SelectTemplate(object data);
    }
}
// MRMW Change End - Add DataTemplateSelector


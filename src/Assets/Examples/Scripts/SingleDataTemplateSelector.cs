using uguimvvm;
using UnityEngine;
// MRMW Change Start - Sample DataTemplateSelector
public class SingleDataTemplateSelector : DataTemplateSelector
{
    public GameObject DefaultPrefab;

    override public GameObject SelectTemplate(object data)
    {
        return DefaultPrefab;
    }
}
// MRMW Change End - Sample DataTemplateSelector
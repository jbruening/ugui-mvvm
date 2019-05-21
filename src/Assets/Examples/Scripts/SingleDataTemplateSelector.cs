using uguimvvm;
using UnityEngine;

public class SingleDataTemplateSelector : DataTemplateSelector
{
    public GameObject DefaultPrefab;

    override public GameObject SelectTemplate(object data)
    {
        return DefaultPrefab;
    }
}

using uguimvvm;
using UnityEngine;
// MRMW Change Start - Sample Prefab Selector
public class PrefabSelector : BasePrefabSelector
{
    public GameObject DefaultPrefab;

    override public GameObject SelectPrefab(object data)
    {
        return DefaultPrefab;
    }
}
// MRMW Change End - Sample Prefab Selector
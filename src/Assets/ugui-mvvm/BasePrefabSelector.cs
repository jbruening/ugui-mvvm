using UnityEngine;
// MRMW Change Start - Add BasePrefabSelector
namespace uguimvvm
{
    public class BasePrefabSelector : MonoBehaviour, IPrefabSelector
    {
        public virtual GameObject SelectPrefab(object data)
        {
            throw new System.NotImplementedException();
        }
    }
}
// MRMW Change End - Add BasePrefabSelector


using UnityEngine;
// MRMW BEGIN CHANGE - Added IPrefabSelector
namespace uguimvvm
{
    public interface IPrefabSelector
    {
        /// <summary>
        /// Select Prefab is used to decide which prefab should be used for a given data context.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        GameObject SelectPrefab(object data);
    }
}
// MRMW END CHANGE - Added IPrefabSelector

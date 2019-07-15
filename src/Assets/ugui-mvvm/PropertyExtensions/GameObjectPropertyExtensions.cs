using UnityEngine;

namespace uguimvvm.extensions
{
    /// <summary>
    /// Use this monobehavior to use INPC bindings to perform operations on gameobjects
    /// e.g setting gameobject active state bound to a Viewmodel property
    /// </summary>
    [RequireComponent(typeof(INPCBinding))]
    public class GameObjectPropertyExtensions : MonoBehaviour
    {
        public bool ActiveState
        {
            set
            {
                this.gameObject.SetActive(value);
            }
        }
    }
}
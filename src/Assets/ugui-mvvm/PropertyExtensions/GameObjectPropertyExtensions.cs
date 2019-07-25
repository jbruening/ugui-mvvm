using UnityEngine;

namespace uguimvvm.extensions
{
    /// <summary>
    /// Use this monobehavior to use PropertyBindings to perform operations on gameobjects
    /// e.g setting gameobject active state bound to a Viewmodel property
    /// </summary>
    [RequireComponent(typeof(PropertyBinding))]
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

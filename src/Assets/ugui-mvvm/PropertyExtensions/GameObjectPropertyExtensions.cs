using UnityEngine;

namespace uguimvvm.extensions
{
    /// <summary>
    /// Use this <see cref="MonoBehaviour"/> to use <see cref="PropertyBinding"/>s to perform operations on <see cref="GameObject"/>s
    /// </summary>
    [RequireComponent(typeof(PropertyBinding))]
    public class GameObjectPropertyExtensions : MonoBehaviour
    {
        /// <summary>
        /// Settable property for pushing a value through the <see cref="GameObject.SetActive(bool)"/> method.
        /// </summary>
        public bool ActiveState
        {
            set
            {
                this.gameObject.SetActive(value);
            }
        }
    }
}

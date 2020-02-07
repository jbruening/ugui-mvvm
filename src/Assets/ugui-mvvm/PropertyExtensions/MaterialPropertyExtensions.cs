using UnityEngine;

namespace uguimvvm.extensions
{
    /// <summary>
    /// Use this <see cref="MonoBehaviour"/> to use PropertyBindings to perform operations on material properties
    /// </summary>
    [RequireComponent(typeof(PropertyBinding))]
    public class MaterialPropertyExtensions : MonoBehaviour
    {
        [SerializeField]
        private Material material = null;

        [SerializeField]
        private string materialPropertyName = null;

        /// <summary>
        /// Settable property for pushing a value through the <see cref="Material.SetFloat(string, float)"/> method.
        /// </summary>
        public float MaterialFloatProperty
        {
            set
            {
                this.material.SetFloat(materialPropertyName, value);
            }
        }

        /// <summary>
        /// Settable property for pushing a value through the <see cref="Material.SetColor(string, Color)"/> method.
        /// </summary>
        public Color MaterialColorProperty
        {
            set
            {
                this.material.SetColor(materialPropertyName, value);
            }
        }

        /// <summary>
        /// Settable property for pushing a value through the <see cref="Material.SetTexture(string, Texture)"/> method.
        /// </summary>
        public Texture MaterialTextureProperty
        {
            set
            {
                this.material.SetTexture(materialPropertyName, value);
            }
        }

        /// <summary>
        /// Settable property for pushing a value through the <see cref="Material.SetVector(string, Vector4)"/> method.
        /// </summary>
        public Vector4 MaterialVectorProperty
        {
            set
            {
                this.material.SetVector(materialPropertyName, value);
            }
        }
    }
}

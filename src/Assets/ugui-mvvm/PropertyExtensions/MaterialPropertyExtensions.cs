using UnityEngine;

namespace uguimvvm.extensions
{
    /// <summary>
    /// Use this monobehavior to use INPC bindings to perform operations on material properties
    /// </summary>
    [RequireComponent(typeof(INPCBinding))]
    public class MaterialPropertyExtensions : MonoBehaviour
    {
        [SerializeField]
        private Material material = null;

        [SerializeField]
        private string materialPropertyName = null; 

        public float MaterialFloatProperty
        {
            set
            {
                this.material.SetFloat(materialPropertyName, value);
            }
        }

        public Color MaterialColorProperty
        {
            set
            {
                this.material.SetColor(materialPropertyName, value);
            }
        }

        public Texture MaterialTextureProperty
        {
            set
            {
                this.material.SetTexture(materialPropertyName, value);
            }
        }

        public Vector4 MaterialVectorProperty
        {
            set
            {
                this.material.SetVector(materialPropertyName, value);
            }
        }
    }
}
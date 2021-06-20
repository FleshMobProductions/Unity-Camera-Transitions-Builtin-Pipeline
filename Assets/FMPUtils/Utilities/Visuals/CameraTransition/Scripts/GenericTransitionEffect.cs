using UnityEngine;

namespace FMPUtils.Visuals.CameraTransition
{
    /// <summary>
    /// Can be used to preview camera transition effects in the editor. Applies the material to the RenderTexture of the camera OnRenderImage
    /// </summary>
    [ExecuteInEditMode]
    public class GenericTransitionEffect : MonoBehaviour, ITransitionCameraEffect
    {
        [SerializeField]
        private Material material;

        public Material EffectMaterial
        {
            get => material;
            set => material = value;
        }
        /// <summary>
        /// Destruction will only have an effect is the application is playing, so not when we do editor stuff
        /// </summary>
        public bool DestroyMaterialOnDestroy { get; set; } = false;

        public void AssignFromCameraTexture(RenderTexture fromCameraTexture)
        {
            if (material != null)
                material.SetTexture(TransitionEffectShaderPropertyIDs.originCamTexture, fromCameraTexture);
        }
        public void SetTransitionEffectProgress(float progress)
        {
            if (material != null)
                material.SetFloat(TransitionEffectShaderPropertyIDs.transitionProgress, Mathf.Clamp01(progress));
        }

        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (material != null)
                Graphics.Blit(source, destination, material);
        }

        private void OnDestroy()
        {
            if (DestroyMaterialOnDestroy && material != null && Application.isPlaying)
                Destroy(material);
        }
    }
}

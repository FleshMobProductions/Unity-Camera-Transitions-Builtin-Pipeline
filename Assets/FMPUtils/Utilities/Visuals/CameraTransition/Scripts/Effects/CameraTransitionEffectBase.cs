using System;
using UnityEngine;

namespace FMPUtils.Visuals.CameraTransition
{
    // Creates a new material and assigns the shader with the name {TransitionEffectShaderName} to it 
    [RequireComponent(typeof(Camera))]
    public abstract class CameraTransitionEffectBase : MonoBehaviour, ITransitionCameraEffect
    {
        protected abstract string TransitionEffectShaderName { get; }

        protected Material effectMaterial;

        public Material EffectMaterial
        {
            get
            {
                ValidateMaterial();
                return effectMaterial;
            }
        }

        public void SetTransitionEffectProgress(float progress)
        {
            ValidateMaterial();
            effectMaterial.SetFloat(TransitionEffectShaderPropertyIDs.transitionProgress, Mathf.Clamp01(progress));
        }

        public void AssignFromCameraTexture(RenderTexture fromCameraTexture)
        {
            ValidateMaterial();
            effectMaterial.SetTexture(TransitionEffectShaderPropertyIDs.originCamTexture, fromCameraTexture);
        }

        protected void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            ValidateMaterial();
            Graphics.Blit(source, destination, effectMaterial);
        }

        protected void ValidateMaterial()
        {
            if (effectMaterial == null)
            {
                Shader transitionShader = Shader.Find(TransitionEffectShaderName);
                if (transitionShader == null)
                {
                    string errorMsg = $"{GetType()}: Shader {TransitionEffectShaderName} not found, make sure to add it in your 'Always Included Shaders' in the Graphics Settings";
                    Debug.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }
                effectMaterial = new Material(transitionShader);
            }
        }

        protected void OnDestroy()
        {
            if (effectMaterial != null)
            {
                if (Application.isPlaying)
                    Destroy(effectMaterial);
                else
                    DestroyImmediate(effectMaterial);
            }
        }
    }
}

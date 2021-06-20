using UnityEngine;

namespace FMPUtils.Visuals.CameraTransition
{
    public class TransitionEffectShaderPropertyIDs
    {
        public static readonly int originCamTexture = Shader.PropertyToID("_OriginCamTex");
        public static readonly int transitionProgress = Shader.PropertyToID("_TransitionProgress");
        /// <summary>
        /// Applies only to the FadeMaskTransitionEffect shader
        /// </summary>
        public static readonly int transitionMaskTexture = Shader.PropertyToID("_TransitionMaskTex");
    }
}

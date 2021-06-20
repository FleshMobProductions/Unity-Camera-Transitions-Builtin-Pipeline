using UnityEngine;

namespace FMPUtils.Visuals.CameraTransition
{
    [AddComponentMenu(null)] // Prevent the behaviour from showing up in the "Add Component" menu
    [RequireComponent(typeof(Camera))]
    public class DiamondTransitionEffect : CameraTransitionEffectBase, ITransitionCameraEffect
    {
        protected override string TransitionEffectShaderName => "FMPUtils/CameraTransitions/DiamondTransitionEffect";
    }
}

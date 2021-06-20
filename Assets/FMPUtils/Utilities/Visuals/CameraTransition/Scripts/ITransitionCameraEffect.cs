using UnityEngine;

namespace FMPUtils.Visuals.CameraTransition
{
    public interface ITransitionCameraEffect
    {
        void AssignFromCameraTexture(RenderTexture fromCameraTexture);
        void SetTransitionEffectProgress(float progress);
        Material EffectMaterial { get;  }
    }
}


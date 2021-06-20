using UnityEngine;

namespace FMPUtils.Visuals.CameraTransition.Examples
{
    public class TransitionEffectTester : MonoBehaviour
    {
        [Header("Essential Properties")]
        [SerializeField] private Camera fromCamera;
        [SerializeField] private Camera toCamera;
        [Range(0.1f, 10f)]
        [SerializeField] private float transitionDuration = 1f;
        [Header("User Defined Transition Properties")]
        [SerializeField] private Material transitionTestMaterial;
        [SerializeField] private Texture transitionTestMaskTexture;
        [Header("Input Keycodes")]
        
        [SerializeField] private KeyCode keyMaterialBasedTransition = KeyCode.Alpha1;
        [SerializeField] private KeyCode keyMaskTextureBasedTransition = KeyCode.Alpha2;
        [SerializeField] private KeyCode keyBuiltinVerticalLinesTransition = KeyCode.Alpha3;
        [SerializeField] private KeyCode keyBuiltinDiagonalTransition = KeyCode.Alpha4;
        [SerializeField] private KeyCode keyBuiltinDiamondTransition = KeyCode.Alpha5;
        [SerializeField] private KeyCode keyBuiltinAlphaFadeTransition = KeyCode.Alpha6;

        private void Update()
        {
            bool wasTransitionTriggered = false;
            bool reassignAudioListenerToTargetCamera = true;
            if (Input.GetKeyDown(keyMaterialBasedTransition) && transitionTestMaterial != null)
            {
                bool createNewMaterialForTransition = true;
                CameraTransitionEffectController.Instance.ActivateTransition(fromCamera, toCamera, transitionTestMaterial, createNewMaterialForTransition, transitionDuration, reassignAudioListenerToTargetCamera);
                wasTransitionTriggered = true;
            }
            else if (Input.GetKeyDown(keyMaskTextureBasedTransition) && transitionTestMaskTexture != null)
            {
                CameraTransitionEffectController.Instance.ActivateTransition(fromCamera, toCamera, transitionTestMaskTexture, transitionDuration, reassignAudioListenerToTargetCamera);
                wasTransitionTriggered = true;
            }
            else if (Input.GetKeyDown(keyBuiltinVerticalLinesTransition))
            {
                CameraTransitionEffectController.Instance.ActivateTransition<VerticalLinesTransitionEffect>(fromCamera, toCamera, transitionDuration, reassignAudioListenerToTargetCamera);
                wasTransitionTriggered = true;
            }
            else if (Input.GetKeyDown(keyBuiltinDiagonalTransition))
            {
                CameraTransitionEffectController.Instance.ActivateTransition<DiagonalTransitionEffect>(fromCamera, toCamera, transitionDuration, reassignAudioListenerToTargetCamera);
                wasTransitionTriggered = true;
            }
            else if (Input.GetKeyDown(keyBuiltinDiamondTransition))
            {
                CameraTransitionEffectController.Instance.ActivateTransition<DiamondTransitionEffect>(fromCamera, toCamera, transitionDuration, reassignAudioListenerToTargetCamera);
                wasTransitionTriggered = true;
            }
            else if (Input.GetKeyDown(keyBuiltinAlphaFadeTransition))
            {
                CameraTransitionEffectController.Instance.ActivateTransition<AlphaFadeTransitionEffect>(fromCamera, toCamera, transitionDuration, reassignAudioListenerToTargetCamera);
                wasTransitionTriggered = true;
            }

            if (wasTransitionTriggered)
                (fromCamera, toCamera) = (toCamera, fromCamera);
        }
    }
}


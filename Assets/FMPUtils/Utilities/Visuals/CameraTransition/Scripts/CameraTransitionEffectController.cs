using System;
using UnityEngine;

namespace FMPUtils.Visuals.CameraTransition
{
    public class CameraTransitionEffectController : SingletonFMP<CameraTransitionEffectController>
    {
        private static readonly string fadeMaskTransitionEffectShaderName = "FMPUtils/CameraTransitions/FadeMaskTransitionEffect";
        private static readonly float transitionDurationMin = 0.01f;

        public bool IsTransitionEffectActive { get; private set; }
        public Camera FromCamera { get; private set; }
        public Camera ToCamera { get; private set; }
        public float TransitionDuration { get; private set; }
        public float TransitionStartTime { get; private set; }
        public float TransitionEndTime => TransitionStartTime + TransitionDuration;
        // The _Time.y builtin variable in Unity shaders uses Time.timeSinceLevelLoad, to allow for matching time based calculations, it is used here too
        private float CurrentTime => Time.timeSinceLevelLoad;

        private RenderTexture renderTexture;
        //private float fromCameraDepthOriginal;
        private RenderTexture fromCameraTargetTextureOriginal;
        private ITransitionCameraEffect transitionEffectBehaviour;

        private void LogActivateTransitionArgumentNullError(string parameterName)
        {
            Debug.LogError($"CameraTransitionEffectController.ActivateTransition - Argument null: ${parameterName}. Abort camera transition");
        }

        // The new() constraint on the generic type makes sure that the abstract class CameraTransitionEffectBase cannot be passed as valid type
        /// <summary>
        /// Both cameras will be enabled the start of the transition. fromCamera will have a new temporary Depth value assigned that is 1 below the toCamera. 
        /// This temporary value wil be reset again once the transition is over. When the transition has ended, fromCamera will be disabled. 
        /// The fromCamera will start to render to a rendertexture during the transition, instead of rendering to the screen. Accepted generic types must be non-abstract 
        /// and inherit from CameraTransitionEffectBase
        /// </summary>
        /// <param name="fromCamera"></param>
        /// <param name="toCamera"></param>
        /// <param name="transitionDuration"></param>
        /// <param name="reassignAudioListener">If true, will remove the Audio Listener from the fromCamera if present and add an AudioListener to the toCamera if not present</param>
        public void ActivateTransition<T>(Camera fromCamera, Camera toCamera, float transitionDuration, bool reassignAudioListener = false) where T : CameraTransitionEffectBase, ITransitionCameraEffect, new()
        {
            ITransitionCameraEffect AssignAndReturnPredefinedTransitionEffect(Camera targetCamera)
            {
                ITransitionCameraEffect transitionEffectBehaviourTemp = targetCamera.GetComponent<T>();
                if (transitionEffectBehaviourTemp == null)
                    transitionEffectBehaviourTemp = targetCamera.gameObject.AddComponent<T>();
                return transitionEffectBehaviourTemp;
            }

            ActivateTransition(fromCamera, toCamera, AssignAndReturnPredefinedTransitionEffect, transitionDuration, reassignAudioListener);
        }

        // If this version is used, create a new material copy from the passed material, so that the passed material will not be changed
        public void ActivateTransition(Camera fromCamera, Camera toCamera, Material imageEffectMaterial, bool createNewMaterialInstance, float transitionDuration, bool reassignAudioListener = false)
        {
            ITransitionCameraEffect AssignAndReturnCustomMaterialTransitionEffect(Camera targetCamera)
            {
                GenericTransitionEffect transitionEffectBehaviourTemp = targetCamera.GetComponent<GenericTransitionEffect>();
                if (transitionEffectBehaviourTemp == null)
                    transitionEffectBehaviourTemp = targetCamera.gameObject.AddComponent<GenericTransitionEffect>();
                if (createNewMaterialInstance)
                {
                    Material materialInstance = Instantiate(imageEffectMaterial);
                    transitionEffectBehaviourTemp.EffectMaterial = materialInstance;
                    transitionEffectBehaviourTemp.DestroyMaterialOnDestroy = true;
                }
                else 
                {
                    transitionEffectBehaviourTemp.EffectMaterial = imageEffectMaterial;
                    transitionEffectBehaviourTemp.DestroyMaterialOnDestroy = false;
                }
                return transitionEffectBehaviourTemp;
            }

            if (imageEffectMaterial == null)
            {
                LogActivateTransitionArgumentNullError(nameof(imageEffectMaterial));
                return;
            }
            ActivateTransition(fromCamera, toCamera, AssignAndReturnCustomMaterialTransitionEffect, transitionDuration, reassignAudioListener);
        }

        public void ActivateTransition(Camera fromCamera, Camera toCamera, Texture transitionFadeMaskTexture, float transitionDuration, bool reassignAudioListener = false)
        {
            ITransitionCameraEffect AssignAndReturnCustomFadeMaskTextureTransitionEffect(Camera targetCamera)
            {
                GenericTransitionEffect transitionEffectBehaviourTemp = targetCamera.GetComponent<GenericTransitionEffect>();
                if (transitionEffectBehaviourTemp == null)
                    transitionEffectBehaviourTemp = targetCamera.gameObject.AddComponent<GenericTransitionEffect>();
                Shader transitionShader = Shader.Find(fadeMaskTransitionEffectShaderName);
                if (transitionShader == null)
                {
                    string errorMsg = $"{GetType()}: Shader {fadeMaskTransitionEffectShaderName} not found, make sure to add it in your 'Always Included Shaders' in the Graphics Settings";
                    Debug.LogError(errorMsg);
                    throw new Exception(errorMsg);
                }
                Material effectMaterial = new Material(transitionShader);
                effectMaterial.SetTexture(TransitionEffectShaderPropertyIDs.transitionMaskTexture, transitionFadeMaskTexture);
                transitionEffectBehaviourTemp.EffectMaterial = effectMaterial;
                transitionEffectBehaviourTemp.DestroyMaterialOnDestroy = true;
                return transitionEffectBehaviourTemp;
            }

            if (transitionFadeMaskTexture == null)
            {
                LogActivateTransitionArgumentNullError(nameof(transitionFadeMaskTexture));
                return;
            }
            ActivateTransition(fromCamera, toCamera, AssignAndReturnCustomFadeMaskTextureTransitionEffect, transitionDuration, reassignAudioListener);
        }

        private void ActivateTransition(Camera fromCamera, Camera toCamera, Func<Camera, ITransitionCameraEffect> transitionEffectBehaviourAssignment, float transitionDuration, bool reassignAudioListener = false)
        {
            if (fromCamera == null)
            {
                LogActivateTransitionArgumentNullError(nameof(fromCamera));
                return;
            }
            if (toCamera == null)
            {
                LogActivateTransitionArgumentNullError(nameof(toCamera));
                return;
            }

            // Clean up previous transition
            if (IsTransitionEffectActive)
                EndActiveTransition();

            FromCamera = fromCamera;
            ToCamera = toCamera;
            if (reassignAudioListener)
            {
                AudioListener fromListener = fromCamera.GetComponent<AudioListener>();
                if (fromListener != null)
                    Destroy(fromListener);
                AudioListener toListener = toCamera.GetComponent<AudioListener>();
                if (toListener == null)
                    toCamera.gameObject.AddComponent<AudioListener>();
            }

            fromCameraTargetTextureOriginal = fromCamera.targetTexture;
            TransitionDuration = Mathf.Max(transitionDuration, transitionDurationMin);
            TransitionStartTime = CurrentTime;
            // TODO: Consider whether the depth needs to be changed. Since we use a render texture for one camera during the transition, 
            // the depth settings of the cameras shouldn't play a role 
            //fromCameraDepthOriginal = fromCamera.depth;
            //fromCamera.depth = toCamera.depth - 1f;
            fromCamera.gameObject.SetActive(true);
            fromCamera.enabled = true;
            toCamera.gameObject.SetActive(true);
            toCamera.enabled = true;

            transitionEffectBehaviour = transitionEffectBehaviourAssignment(toCamera);
            ValidateAndAssignRenderTexture(fromCamera, transitionEffectBehaviour);
            // There is the possibility that we have a cam switch but the render texture still has valid dimensions and hence is not assigned to the effect, 
            // So we force it
            transitionEffectBehaviour.AssignFromCameraTexture(renderTexture);
            transitionEffectBehaviour.SetTransitionEffectProgress(0f);

            IsTransitionEffectActive = true;
        }

        private void ValidateAndAssignRenderTexture(Camera targetCamera, ITransitionCameraEffect cameraEffect)
        {
            if (renderTexture == null || renderTexture.width != targetCamera.pixelWidth || renderTexture.height != targetCamera.pixelHeight)
            {
                if (renderTexture != null)
                    renderTexture.Release();
                // Texture depth 16 bit will not have a stencil buffer as we don't need it
                renderTexture = new RenderTexture(targetCamera.pixelWidth, targetCamera.pixelHeight, 16, RenderTextureFormat.Default);
                // If the render texture has changed, we need to assign it again to the image effect using it
                cameraEffect.AssignFromCameraTexture(renderTexture);
                targetCamera.targetTexture = renderTexture;
            }
            else
            {
                if (targetCamera.targetTexture != renderTexture)
                    targetCamera.targetTexture = renderTexture;
            }
        }

        private void Update()
        {
            if (IsTransitionEffectActive)
            {
                float currentTime = CurrentTime;
                if (transitionEffectBehaviour != null)
                {
                    float progress = Mathf.Clamp01((currentTime - TransitionStartTime) / TransitionDuration);
                    transitionEffectBehaviour.SetTransitionEffectProgress(progress);
                }
                if (FromCamera != null)
                    ValidateAndAssignRenderTexture(FromCamera, transitionEffectBehaviour);

                if (currentTime >= TransitionEndTime)
                    EndActiveTransition();
            }
        }

        public void EndActiveTransition()
        {
            if (FromCamera != null)
            {
                //FromCamera.depth = fromCameraDepthOriginal;
                FromCamera.targetTexture = fromCameraTargetTextureOriginal;
                FromCamera.enabled = false;
                FromCamera = null;
            }
            if (ToCamera != null)
            {
                if (transitionEffectBehaviour != null)
                    Destroy(transitionEffectBehaviour as Component);
                ToCamera = null;
            }
            IsTransitionEffectActive = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (renderTexture != null)
            {
                renderTexture.Release();
                renderTexture = null;
            }
        }
    }
}



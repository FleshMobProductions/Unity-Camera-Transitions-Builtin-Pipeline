using UnityEngine;

namespace FMPUtils
{
    /// <summary>
    /// Base Class for Singletons. Will initialize the static instance variable
    /// If a subclass needs an Awake() logic, put it instead into AwakeOverride() and don't implement Awake(), 
    /// for AwakeOverride to be called, the implementing class has to implement ISingletonFMP. 
    /// Implementation example: public class SingletonClass : SingletonFMP<SingletonClass>, ISingletonFMP. 
    /// With the way unity works, a null check has to be made when retrieving singleton instances in methods like 
    /// OnDisable/OnDestroy etc. when it is expected that the app is quitting at this point, because 
    /// the singleton might already be decomissioned by then
    /// </summary>
    public class SingletonFMP<T> : MonoBehaviour where T : MonoBehaviour {
        // Used to prevent the error: Some objects were not cleaned up when closing the scene. (Did you spawn new GameObjects from OnDestroy?)
        // Taken from https://answers.unity.com/questions/1274772/some-objects-were-not-cleaned-up-when-closing-the-4.html
        private static bool isApplicationQuitting;
        protected static T instance;

        public static T Instance {
            get {
                if (isApplicationQuitting)
                {
                    Debug.LogWarning("Application already quitting, cannot access singleton anymore");
                    return null;
                }
                if (instance == null){
                    var singletonComponent = FindObjectOfType<T>();
                    if (singletonComponent == null){
                        var go = new GameObject(string.Format("{0}Singleton", typeof(T).ToString()));
                        singletonComponent = go.AddComponent<T>();
                    }
                    Initialize(singletonComponent);
                }
                return instance;
            }
        }

        private static void Initialize(T component){
            if (component == null){
                return;
            }
            if (instance == null){
                DontDestroyOnLoad(component.gameObject);
                instance = component;
                // Following approach throws null reference exceptions in the OnDisable code in certain components: 
                //Application.quitting += SetApplicationQuittingTrue;
                // Awake will be called right after AddComponent, 
                // however, we don't want to mess with the awake function since it has to call initialize, 
                // So we take a custom function
                var iSingleton = component as ISingletonFMP;
                if (iSingleton != null)
                {
                    iSingleton.AwakeOverride();
                }
            } else if (instance != component){
                Destroy(component.gameObject);
            } 
            // else the instance is already this object, in which case we don't have to do anything
        }

        /// <summary>
        /// Don't use Awake() in singleton implementations, instead implement AwakeOverride()
        /// If it is used, make sure to call base.Awake() first
        /// </summary>
        protected virtual void Awake() {
            Initialize(GetComponent<T>());
        }

        ///// <summary>
        ///// Clear the reference when the application quits. Override when necessary and call base.OnApplicationQuit() last.
        ///// </summary>
        protected virtual void OnApplicationQuit()
        {
            isApplicationQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            isApplicationQuitting = true;
        }

    }
}
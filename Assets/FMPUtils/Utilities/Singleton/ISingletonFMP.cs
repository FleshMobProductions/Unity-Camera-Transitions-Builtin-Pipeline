using UnityEngine;

namespace FMPUtils
{
    public interface ISingletonFMP {
        /// <summary>
        /// Use this function instead of the regular Awake and avoid the regular Awake for singletons
        /// This method is called once when the singleton is initialized
        /// </summary>
        void AwakeOverride();
    }
}
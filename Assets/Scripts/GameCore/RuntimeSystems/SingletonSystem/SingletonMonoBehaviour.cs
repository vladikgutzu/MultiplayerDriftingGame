using UnityEngine;

namespace GameCore.RuntimeSystems.SingletonSystem
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour
        where T : MonoBehaviour
    {
        public static T I;

        protected abstract T GetCurrentInstance();
        
        protected virtual void Awake()
        {
            T current = GetCurrentInstance();
            
            if (I == null)
            {
                I = current;
            }
            else if (I != current)
            {
                Destroy(gameObject);
            }
        }
    }
}

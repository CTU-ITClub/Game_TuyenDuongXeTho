using UnityEngine;

namespace VGDSystem.DesignPattern
{
    public abstract class BaseSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T _instance = null;

        public static T Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindFirstObjectByType<T>();
                    if (!_instance)
                    {
                        GameObject gObj = new GameObject();
                        _instance = gObj.AddComponent<T>();
                        gObj.name = "Singleton_" + typeof(T).ToString();
                        DontDestroyOnLoad(gObj);
                    }
                }
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            CreateInstance();
        }

        protected virtual void CreateInstance()
        {
            if (!_instance)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);

            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

}

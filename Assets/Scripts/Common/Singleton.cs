using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            // 애플리케이션이 종료 중이면 null 반환
            if (_applicationIsQuitting)
            {
                Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed. Won't create again.");
                return null;
            }

            // 인스턴스가 없으면 찾거나 생성
            if (_instance == null)
            {
                _instance = (T)FindFirstObjectByType(typeof(T));

                // 씬에 여러 개가 있으면 경고
                if (FindObjectsByType(typeof(T), FindObjectsSortMode.InstanceID).Length > 1)
                {
                    Debug.LogError($"[Singleton] Multiple instances of {typeof(T)} found!");
                    return _instance;
                }

                // 씬에 없으면 새로 생성
                if (_instance == null)
                {
                    GameObject singleton = new GameObject();
                    _instance = singleton.AddComponent<T>();
                    singleton.name = $"[Singleton] {typeof(T)}";
                    DontDestroyOnLoad(singleton);
                }
            }

            return _instance;
        }
    }

    // 인스턴스 존재 여부 확인 (생성하지 않고)
    public static bool HasInstance => _instance != null;

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"[Singleton] Duplicate instance of {typeof(T)} destroyed.");
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour
{
    private Queue<T> pool = new Queue<T>();
    private List<T> activeObjects = new List<T>();
    private List<T> tempObjects = new List<T>();
    private GameObject prefab;
    private Transform parent;
    private bool isInitialized = false;


    public List<T> GetActiveObjects() => activeObjects;
    public int ActiveCount => activeObjects.Count;
    public int PooledCount => pool.Count;
    public int TotalCount => ActiveCount + PooledCount;

    public ObjectPool(GameObject prefab, int initialSize, Transform parent = null)
    {
        Initialize(prefab, initialSize, parent);
    }

    public void Initialize(GameObject prefab, int initialSize, Transform parent = null)
    {
        if (isInitialized)
        {
            Debug.LogWarning("ObjectPool is already initialized!");
            return;
        }

        this.prefab = prefab;
        this.parent = parent;

        // 초기 풀 생성
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }

        isInitialized = true;
    }

    private T CreateNewObject()
    {
        GameObject obj = Object.Instantiate(prefab, parent);
        T component = obj.GetComponent<T>();
        obj.SetActive(false);
        pool.Enqueue(component);
        return component;
    }

    private T CreateTempObject()
    {
        // 풀에 넣지 않고 임시로 사용하는 게임오브젝트를 생성합니다.
        GameObject obj = Object.Instantiate(prefab, parent);
        T component = obj.GetComponent<T>();
        obj.SetActive(false);
        tempObjects.Add(component);
        return component;
    }

    public T GetOrCreate()
    {
        T obj = null;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = CreateTempObject();
        }
        activeObjects.Add(obj);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(T obj)
    {
        if (activeObjects.Remove(obj))
        {
            obj.gameObject.SetActive(false);

            if(tempObjects.Contains(obj))
            {
                // 임시 오브젝트인 경우, 풀에 넣지 않고 제거합니다.
                tempObjects.Remove(obj);
                Object.Destroy(obj.gameObject);
            }
            else
            {
                // 일반 오브젝트인 경우, 풀에 다시 넣습니다.
                pool.Enqueue(obj);
            }
        }
    }

    public bool CanActiveMore(int maxActiveCount)
    {
        return ActiveCount < maxActiveCount;
    }

    public void Clear()
    {
        // 모든 오브젝트 정리
        foreach (var obj in activeObjects)
        {
            if (obj != null)
                Object.Destroy(obj.gameObject);
        }

        while (pool.Count > 0)
        {
            var obj = pool.Dequeue();
            if (obj != null)
                Object.Destroy(obj.gameObject);
        }

        activeObjects.Clear();
        pool.Clear();
        isInitialized = false;
    }

}
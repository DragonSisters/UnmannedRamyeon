using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour
{
    private Queue<T> pool = new Queue<T>();
    private List<T> activeObjects = new List<T>();
    private GameObject prefab;
    private Transform parent;
    private bool isInitialized = false;
    private int maxActiveCount;


    public List<T> GetActiveObjects() => activeObjects;
    public int ActiveCount => activeObjects.Count;
    public int PooledCount => pool.Count;
    public int TotalCount => ActiveCount + PooledCount;

    public ObjectPool(GameObject prefab, int initialSize, int maxActiveCount, Transform parent = null)
    {
        Initialize(prefab, initialSize, maxActiveCount, parent);
    }

    public void Initialize(GameObject prefab, int initialSize, int maxActiveCount, Transform parent = null)
    {
        if (isInitialized)
        {
            Debug.LogWarning("ObjectPool is already initialized!");
            return;
        }

        this.prefab = prefab;
        this.parent = parent;
        this.maxActiveCount = maxActiveCount;

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

    public T GetOrCreate()
    {
        T obj = pool.Count > 0 ? pool.Dequeue() : CreateNewObject();
        activeObjects.Add(obj);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(T obj)
    {
        if (activeObjects.Remove(obj))
        {
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public bool IsAvailableToCreate()
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
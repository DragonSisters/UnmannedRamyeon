using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 손님의 등장과 퇴장을 관리합니다.
/// </summary>
public class ConsumerManager : Singleton<ConsumerManager>
{
    [Header("Consumer 프리팹들이 생성될 부모 오브젝트")]
    [SerializeField] private Transform spawnParent;
    [Header("Consumer 프리팹들을 직접 드래그하세요")]
    [SerializeField] private List<GameObject> consumerPrefabs = new List<GameObject>();
    [Header("Pool 사이즈")]
    [SerializeField] private int poolSize = 10;
    [Header("최대 소환 갯수")]
    [SerializeField] private int maxActiveCount = 3;
    [Header("N~M초마다 등장합니다.")]
    [SerializeField] private float minSpawnInterval = 1f;
    [SerializeField] private float maxSpawnInterval = 3f;

    private Dictionary<GameObject, ObjectPool<Consumer>> pools;

    private Coroutine spawnCoroutine;
    private Coroutine despawnCoroutine;


    public void OnClickedConsumer()
    {
        foreach (var consumer in pools)
        {
            var activeObjs = consumer.Value.GetActiveObjects();
            foreach (var obj in activeObjs)
            {
                if(obj.IsClicked && ((IClickableSprite)obj != SpriteClickHandler.Instance.CurrentClickedSprite))
                {
                    obj.OnSpriteDeselected();
                }
            }
        }
    }

    public void InitializePools()
    {
        // 모든 오브젝트 정리
        if(pools != null)
        {
            foreach (var pool in pools.Values)
            {
                pool.Clear();
            }
        }

        pools = new Dictionary<GameObject, ObjectPool<Consumer>>();

        foreach (GameObject prefab in consumerPrefabs)
        {
            pools[prefab] = new ObjectPool<Consumer>(
                prefab, poolSize, spawnParent);
        }
    }

    public void StartSpawn()
    {
        if (IsAvailableSpawn())
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
            }
            if (despawnCoroutine != null)
            {
                StopCoroutine(despawnCoroutine);
            }

            spawnCoroutine = StartCoroutine(SpawnRoutine());
            despawnCoroutine = StartCoroutine(DespawnRoutine());
        }
    }

    public void StopSpawn()
    {
        // 모든 손님을 없앱니다.
        CheckAndDespawnCustomers(true);
        // 모든 코루틴을 멈춥니다.
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        if (despawnCoroutine != null)
        {
            StopCoroutine(despawnCoroutine);
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (IsAvailableSpawn())
        {
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));

            SpawnRandomObject();
            
        }
    }

    private IEnumerator DespawnRoutine()
    {
        while(IsAvailableSpawn())
        {
            CheckAndDespawnCustomers();
            yield return new WaitUntil(() => Time.frameCount % 30 == 0);
        }
    }

    private bool IsAvailableSpawn()
    {
        // @charotiti9 TODO: 나중에 여러가지 조건이 생기면 여기서 검사합니다.
        // ex. 게임이 시작하지 않았다거나... 끝났다거나...
        foreach (var prefab in consumerPrefabs)
        {
            if (prefab == null)
            {
                Debug.LogError("손님 프리팹이 null입니다. ConsumerManager 인스펙터를 확인해주세요.");
                return false;
            }
        }

        return true;
    }

    private void SpawnRandomObject()
    {
        if (consumerPrefabs.Count == 0)
        {
            return;
        }

        GameObject prefab = consumerPrefabs[Random.Range(0, consumerPrefabs.Count)];
        if(!pools[prefab].CanActiveMore(maxActiveCount))
        {
            return;
        }

        Consumer obj = pools[prefab].GetOrCreate();
        obj.OnSpawn();
    }

    /// <summary>
    /// 스폰되어있는 손님들을 디스폰합니다.
    /// </summary>
    /// <param name="skipCheck">조건없이 모두 디스폰할 것인지 선택합니다</param>
    private void CheckAndDespawnCustomers(bool skipCheck = false)
    {
        foreach (var pool in pools.Values)
        {
            var activeObjects = pool.GetActiveObjects();

            // 역순으로 순회하여 안전한 제거
            for (int i = activeObjects.Count - 1; i >= 0; i--)
            {
                var obj = activeObjects[i];
                if (obj.ShouldDespawn() || skipCheck)
                {
                    obj.OnDespawn();
                    pool.Return(obj);
                }
            }
        }
    }
}

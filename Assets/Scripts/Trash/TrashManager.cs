using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TrashManager : Singleton<TrashManager>
{
    public Texture2D CleaningCursorIcon => cleaningCursorIcon;
    [SerializeField] private Texture2D cleaningCursorIcon;
    [SerializeField] private List<GameObject> trashPrefabs;
    [SerializeField] private Transform trashParent;
    [SerializeField] private int poolSize = 10;

    [Header("스폰 위치 설정")]
    [SerializeField] private Transform spawnAreaMax;
    [SerializeField] private Transform spawnAreaMin;

    [Header("스폰 간격 설정")]
    [SerializeField] private float minSpawnInterval = 3f;
    [SerializeField] private float maxSpawnInterval = 10f;
    [SerializeField] private int maxActiveLimit = 5;
    private float GetRandomInterval => Random.Range(minSpawnInterval, maxSpawnInterval);

    [Header("스폰 확률 설정")]
    [SerializeField] private float spawnStartDelay = 30f; // 30초 후 스폰 시작
    [SerializeField, Range(0f, 1f)] private float spawnProbability = 0.7f; // 70% 확률

    private ObjectPool<Trash> trashPool;
    private Coroutine spawnCoroutine;
    private Coroutine despawnCoroutine;
    private float startTime;
    private float navMeshSampleRadius = 3f; // NavMesh 샘플링 반경

    public void Initialize()
    {
        // 모든 오브젝트 정리
        if (trashPool != null)
        {
            trashPool.Clear();
        }

        foreach (GameObject prefab in trashPrefabs)
        {
            trashPool = new ObjectPool<Trash>(
                prefab, poolSize, trashParent);
        }
    }

    public void StartSpawn()
    {
        if (!IsAvailableSpawn())
        {
            return;
        }

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        if (despawnCoroutine != null)
        {
            StopCoroutine(despawnCoroutine);
        }

        startTime = GameManager.Instance.GameStartTime;

        spawnCoroutine = StartCoroutine(SpawnRoutine());
        despawnCoroutine = StartCoroutine(DespawnRoutine());
    }

    public void StopSpawn()
    {
        CheckAndDespawnTrash(true);

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
            yield return new WaitForSeconds(GetRandomInterval);

            float elapsedTime = Time.time - startTime;
            if (elapsedTime < spawnStartDelay)
            {
                continue; // 30초 전에는 스폰하지 않음
            }

            float randomProbability = Random.value;
            if (randomProbability > spawnProbability)
            {
                // 쓰레기 스폰 확률에 의해 스폰하지 않습니다
                continue;
            }

            SpawnRandomObject();
        }
    }

    private IEnumerator DespawnRoutine()
    {
        while (IsAvailableSpawn())
        {
            CheckAndDespawnTrash();
            yield return new WaitUntil(() => Time.frameCount % 30 == 0);
        }
    }

    /// <summary>
    /// 스폰되어있는 쓰레기들을 디스폰합니다.
    /// </summary>
    /// <param name="skipCheck">조건없이 모두 디스폰할 것인지 선택합니다</param>
    private void CheckAndDespawnTrash(bool skipCheck = false)
    {
        var activeObjects = trashPool.GetActiveObjects();

        // 역순으로 순회하여 안전한 제거
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            var obj = activeObjects[i];
            if (obj.ShouldDespawn() || skipCheck)
            {
                obj.OnDespawn();
                trashPool.Return(obj);
            }
        }
    }

    private void SpawnRandomObject()
    {
        if (trashPrefabs.Count == 0)
        {
            return;
        }

        GameObject prefab = trashPrefabs[Random.Range(0, trashPrefabs.Count)];

        if (!trashPool.CanActiveMore(maxActiveLimit))
        {
            return;
        }

        var obj = trashPool.GetOrCreate();
        var position = new Vector2(
            Random.Range(spawnAreaMin.position.x, spawnAreaMax.position.x),
            Random.Range(spawnAreaMin.position.y, spawnAreaMax.position.y)
        );

        // navMesh가 닿는 범위 안에 있는지 검사하고 닿지 않는다면 가장 가까운 곳으로 위치시키
        if (NavMesh.SamplePosition(position, out var hit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            position = hit.position;
        }
        else
        {
            Debug.LogWarning("스폰 위치가 NavMesh에 닿는 곳이 없습니다.");
        }

        obj.transform.position = position;
        obj.OnSpawn();
    }

    private bool IsAvailableSpawn()
    {
        foreach (var prefab in trashPrefabs)
        {
            if (prefab == null)
            {
                Debug.LogError($"쓰레기 프리팹이 올바르게 설정되지 않았습니다. {nameof(TrashManager)} 인스펙터를 확인해주세요.");
                return false;
            }
        }

        return true;
    }
}
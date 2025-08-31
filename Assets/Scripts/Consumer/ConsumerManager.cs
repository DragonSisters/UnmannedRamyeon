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
    [SerializeField] private GameObject CommonConsumerPrefab;
    [SerializeField] private GameObject RecipeConsumerPrefab;
    [Header("Pool 사이즈")]
    [SerializeField] private int poolSize = 20;

    [Header("스폰 기본 간격 (최대/최소")]
    [SerializeField] private float minSpawnInterval = 1f;
    [SerializeField] private float maxSpawnInterval = 3f;

    [Header("스폰 속도 변화 곡선 (X: 게임 시간 진행 비율, Y: 스폰 간격 배율)")]
    // Y: 스폰 대기 시간에 곱하는 간격 배율 (값이 작을수록 간격이 작아져서 빠른 스폰)
    [SerializeField] private AnimationCurve spawnSpeedCurve = new AnimationCurve();
    [SerializeField] private float spawnIntervalLimit = 0.5f;

    // 최대 소환 개수 변화 곡선 - 일반 손님 레시피 손님 따로
    private AnimationCurve activeCountCurve = new AnimationCurve();
    private AnimationCurve recipeActiveCountCurve = new AnimationCurve();

    [Header("최대 소환 갯수")]
    [SerializeField] private int minCommonActiveLimit = 1;
    [SerializeField] private int maxEasyCommonActiveLimit = 8;
    [SerializeField] private int maxHardCommonActiveLimit = 15;
    [SerializeField] private int minRecipeActiveLimit = 1;
    [SerializeField] private int maxEasyRecipeActiveLimit = 1;
    [SerializeField] private int maxHardRecipeActiveLimit = 1;
    private int maxCommonActiveLimit;
    private int maxRecipeActiveLimit;
    private int currentActiveLimit;
    private int currentRecipeActiveLimit;
    /// <summary>
    /// 현재 주문 가능한 손님 수를 반환합니다.
    /// </summary>
    private int recipeOrderableCount
    {
        get 
        {
            int count = 0;

            // 만약 5명까지 소환가능하다면, 그 중 3명이 주문중이라면 2명을 더 소환할 수 있습니다.
            // 그 3명중 1명이라도 주문을 완료하면 그만큼 다시 소환할 수 있습니다.
            foreach (var consumer in GetAllActiveConsumerToList())
            {
                if (consumer is RecipeConsumer recipeConsumer)
                {
                    if (consumer.State == ConsumerState.Issue ||
                        consumer.State == ConsumerState.Enter)
                    {
                        count++;
                    }
                }
            }

            // 주문하고 있는 사람이 아무도 없으면 무조건 Limit까지 소환 가능합니다.
            if (count == 0)
            {
                return pools[RecipeConsumerPrefab].ActiveCount + maxRecipeActiveLimit;
            }
            // 주문하고 있는 사람이 한 명이라도 있다면 남은 수만큼 소환 가능합니다.
            else
            {
                return maxRecipeActiveLimit - count;
            }
        }
    }

    private Dictionary<GameObject, ObjectPool<Consumer>> pools;

    private Coroutine spawnCommonCoroutine;
    private Coroutine spawnRecipeCoroutine;
    private Coroutine despawnCoroutine;

    private float startTime;
    private float gameDuration;

    public void InitializeConsumerManagerSetting()
    {
        // 모든 오브젝트 정리
        if (pools != null)
        {
            foreach (var pool in pools.Values)
            {
                pool.Clear();
            }
        }

        pools = new Dictionary<GameObject, ObjectPool<Consumer>>();
        pools[CommonConsumerPrefab] = new ObjectPool<Consumer>(
                CommonConsumerPrefab, poolSize, spawnParent);
        pools[RecipeConsumerPrefab] = new ObjectPool<Consumer>(
            RecipeConsumerPrefab, poolSize, spawnParent);
    }

    public void StartSpawn(bool isHardMode)
    {
        if(isHardMode)
        {
            maxCommonActiveLimit = maxHardCommonActiveLimit;
            maxRecipeActiveLimit = maxHardRecipeActiveLimit;
        }
        else
        {
            maxCommonActiveLimit = maxEasyCommonActiveLimit;
            maxRecipeActiveLimit = maxEasyRecipeActiveLimit;
        }

        activeCountCurve = AnimationCurve.Linear(0, minCommonActiveLimit, 1, maxCommonActiveLimit);
        recipeActiveCountCurve = AnimationCurve.Linear(0, minRecipeActiveLimit, 1, maxRecipeActiveLimit);

        if (IsAvailableSpawn())
        {
            if (spawnCommonCoroutine != null)
            {
                StopCoroutine(spawnCommonCoroutine);
            }
            if (spawnRecipeCoroutine != null)
            {
                StopCoroutine(spawnRecipeCoroutine);
            }
            if (despawnCoroutine != null)
            {
                StopCoroutine(despawnCoroutine);
            }

            startTime = GameManager.Instance.GameStartTime;
            gameDuration = GameManager.Instance.GameDuration;

            spawnCommonCoroutine = StartCoroutine(SpawnCommonConsumerRoutine());
            spawnRecipeCoroutine = StartCoroutine(SpawnRecipeConsumerRoutine());
            despawnCoroutine = StartCoroutine(DespawnRoutine());
        }
    }

    public void StopSpawn()
    {
        // 모든 손님을 없앱니다.
        CheckAndDespawnCustomers(true);
        // 모든 코루틴을 멈춥니다.
        if (spawnCommonCoroutine != null)
        {
            StopCoroutine(spawnCommonCoroutine);
        }
        if (spawnRecipeCoroutine != null)
        {
            StopCoroutine(spawnRecipeCoroutine);
        }
        if (despawnCoroutine != null)
        {
            StopCoroutine(despawnCoroutine);
        }
    }

    public void DeselectOtherConsumers()
    {
        foreach (var consumer in pools)
        {
            var activeObjs = consumer.Value.GetActiveObjects();
            foreach (var obj in activeObjs)
            {
                var apearanceGameobject = obj.GetComponentInChildren<ConsumerAppearance>();
                if (apearanceGameobject == null)
                {
                    Debug.LogError($"손님에게 {nameof(ConsumerAppearance)}가 생성되지 않았는데 찾아오려고 했습니다.");
                }
                if (apearanceGameobject.IsClicked && ((IClickableSprite)apearanceGameobject != SpriteClickHandler.Instance.CurrentClickedSprite))
                {
                    apearanceGameobject.OnSpriteDeselected();
                }
            }
        }
    }

    public List<Consumer> GetAllActiveConsumerToList()
    {
        var list = new List<Consumer>();
        foreach (var pool in pools.Values)
        {
            list.AddRange(pool.GetActiveObjects());
        }
        return list;
    }

    private IEnumerator SpawnCommonConsumerRoutine()
    {
        while (IsAvailableSpawn())
        {
            float elapsedTime = Time.time - startTime;
            float t = Mathf.Clamp01(elapsedTime / gameDuration);

            // 곡선에 적용합니다.
            float speedMultiplier = spawnSpeedCurve.Evaluate(t);
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval) * speedMultiplier;
            currentActiveLimit = Mathf.RoundToInt(activeCountCurve.Evaluate(t));

            yield return new WaitForSeconds(waitTime);

            SpawnCommonConsumer();
        }
    }

    private IEnumerator SpawnRecipeConsumerRoutine()
    {
        while (IsAvailableSpawn())
        {
            float elapsedTime = Time.time - startTime;
            float t = Mathf.Clamp01(elapsedTime / gameDuration);
            currentRecipeActiveLimit = Mathf.RoundToInt(recipeActiveCountCurve.Evaluate(t));

            SpawnRecipeConsumer();
            yield return null;
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
        return true;
    }

    private void SpawnCommonConsumer()
    {
        if (!pools[CommonConsumerPrefab].CanActiveMore(currentActiveLimit))
        {
            return;
        }
        Consumer obj = pools[CommonConsumerPrefab].GetOrCreate();
        obj.OnSpawn();
    }

    private void SpawnRecipeConsumer()
    {
        // 레시피 손님은 주문 가능한 손님 수가 남아있을 때 Limit보다 더 소환할 수 있습니다.
        if (!pools[RecipeConsumerPrefab].CanActiveMore(currentRecipeActiveLimit + recipeOrderableCount))
        {
            return;
        }
        Consumer obj = pools[RecipeConsumerPrefab].GetOrCreate();
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

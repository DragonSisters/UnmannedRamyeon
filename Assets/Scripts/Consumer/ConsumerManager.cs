using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 손님의 등장과 퇴장을 관리합니다.
/// </summary>
public class ConsumerManager : Singleton<ConsumerManager>
{
    [Header("Consumer 프리팹들을 직접 드래그하세요")]
    [SerializeField] private List<GameObject> consumerPrefabs = new List<GameObject>();

    /// <summary>
    /// 생성된 컴포넌트들
    /// </summary>
    private Dictionary<int, Consumer> consumerInstances = new Dictionary<int, Consumer>();

    // @charotiti9 TODO: 일단 간단하게 n초마다 등장하고 퇴장하게 만들어봅니다. 나중에 수정합니다.
    [Header("N초마다 등장합니다.")]
    [SerializeField] private float appearTime = 3f;

    [Header("최대 소환 갯수")]
    [SerializeField] private int maxNumber = 3;
    private int currentNumber;

    // 고유 ID 생성을 위한 카운터
    private int nextConsumerID = 0;

    /// <summary>
    /// 업데이트 대신 사용할 코루틴
    /// </summary>
    private Coroutine updateCoroutine;

    private void Start()
    {
        // @charotiti9 TODO: 임시로 여기서 생성을 시작하지만,
        // 나중에는 게임 순환시스템에서 신호가 오면 시작해야할 것입니다.
        StartSpawn();
    }

    public void StartSpawn()
    {
        if (IsAvailableSpawn())
        {
            updateCoroutine = StartCoroutine(UpdateIEnumerator());
        }
    }

    public void StopSpawn()
    {
        // 모든 코루틴을 멈춥니다.
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
        }
    }

    private IEnumerator UpdateIEnumerator()
    {
        ClearExistingInstances();
        while (IsAvailableSpawn())
        {
            if (currentNumber >= maxNumber)
            {
                Debug.Log($"현재 개체수: {currentNumber}");
            }
            else
            {
                // @charotiti9 TODO: 지금은 랜덤으로 생성합니다. 나중에 규칙을 추가적용합니다.
                var randomPrefab = Random.Range(0, consumerPrefabs.Count);
                CreateConsumerInstance(consumerPrefabs[randomPrefab]);
            }

            yield return new WaitForSeconds(appearTime);
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

    /// <summary>
    /// 기존 인스턴스들 정리
    /// </summary>
    private void ClearExistingInstances()
    {
        foreach (var kvp in consumerInstances)
        {
            if (kvp.Value != null)
            {
                DestroyImmediate(kvp.Value.gameObject);
            }
        }
        consumerInstances.Clear();
    }

    private int CreateConsumerInstance(GameObject prefab)
    {
        currentNumber++;

        // 프리팹 인스턴스 생성
        GameObject instance = Instantiate(prefab);

        // Consumer 컴포넌트 확인
        Consumer consumer = instance.GetComponent<Consumer>();
        if (consumer == null)
        {
            Debug.LogError($"Prefab {prefab.name}프리팹이 consumer 컴포넌트를 가지고 있지 않습니다.");
            DestroyImmediate(instance);
            return -1; // 실패시 -1 반환
        }

        // 고유 ID 생성
        int consumerID = nextConsumerID++;
        consumer.id = consumerID;

        // 이름 설정 (ID 포함)
        instance.name = $"{prefab.name}_Instance_{consumerID}";

        // 딕셔너리에 추가
        consumerInstances.Add(consumerID, consumer);

        Debug.Log($"손님{consumerID}가 생성되었습니다.");
        return consumerID; // 생성된 ID 반환
    }


    /// ID로 Consumer 제거
    /// </summary>
    /// <param name="consumerID">제거할 Consumer의 ID</param>
    public void DestroyConsumer(int consumerID)
    {
        if (!consumerInstances.ContainsKey(consumerID))
        {
            Debug.LogWarning($"Consumer ID {consumerID}를 찾을 수 없습니다.");
            return;
        }

        Consumer consumer = consumerInstances[consumerID];
        if (consumer != null)
        {
            currentNumber--;
            // 딕셔너리에서 제거
            consumerInstances.Remove(consumerID);
            DestroyImmediate(consumer.gameObject);
            Debug.Log($"손님{consumerID}가 파괴되었습니다.");
        }
    }

    /// <summary>
    /// 특정 ID의 Consumer 가져오기
    /// </summary>
    /// <param name="consumerID">찾을 Consumer ID</param>
    /// <returns>Consumer 컴포넌트 또는 null</returns>
    public Consumer GetConsumer(int consumerID)
    {
        if (consumerInstances.ContainsKey(consumerID))
        {
            return consumerInstances[consumerID];
        }
        return null;
    }

    /// <summary>
    /// 현재 활성화된 모든 Consumer ID 목록 가져오기
    /// </summary>
    /// <returns>Consumer ID 목록</returns>
    public List<int> GetAllConsumerIDs()
    {
        return new List<int>(consumerInstances.Keys);
    }
}

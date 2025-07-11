using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 손님의 등장과 퇴장을 관리합니다.
/// </summary>
public class ConsumerManager : MonoBehaviour
{
    // @charotiti9 TODO: 일단 간단하게 n초마다 등장하고 퇴장하게 만들어봅니다. 나중에 수정합니다.
    public float appearTime = 100f;

    /// <summary>
    /// 최대 소환갯수를 정합니다.
    /// </summary>
    public int maxNumber = 3;
    private int currentNumber;

    /// <summary>
    /// 등장할 손님 목록
    /// </summary>
    public List<ConsumerScriptableObject> consumers = new();
    /// <summary>
    /// 테스트하고 싶은 손님이 있다면 이 손님만 등장하게 할게요
    /// </summary>
    public ConsumerScriptableObject testConsumer;

    /// <summary>
    /// 업데이트 대신 사용할 코루틴
    /// </summary>
    private Coroutine updateCoroutine;
    /// <summary>
    /// 손님들의 행동 코루틴
    /// </summary>
    private Dictionary<int, Coroutine> consumerCoroutines;
    /// <summary>
    /// 증분할 손님 id
    /// </summary>
    private int consumerId;

    private void Start()
    {
        consumerCoroutines = new Dictionary<int, Coroutine>();
        // @charotiti9 TODO: 임시로 여기서 생성을 시작하지만,
        // 나중에는 게임 순환시스템에서 신호가 오면 시작해야할 것입니다.
        StartSpawn();
    }

    private void StartSpawn()
    {
        if(IsAvailableSpawn())
        {
            updateCoroutine = StartCoroutine(UpdateIEnumerator());
        }
    }

    private void StopSpawn()
    {
        // 모든 코루틴을 멈춥니다.
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
        }
        if(consumerCoroutines.Count > 0)
        {
            foreach (var consumerCoroutine in consumerCoroutines)
            {
                StopCoroutine(consumerCoroutine.Value);
            }
            consumerCoroutines.Clear();
        }
    }

    private bool IsAvailableSpawn()
    {
        // @charotiti9 TODO: 지금은 무조건 스폰시키지만,
        // 나중에 여러가지 조건이 생기면 여기서 검사합니다.
        // ex. 게임이 시작하지 않았다거나... 끝났다거나...
        return true;
    }

    private IEnumerator UpdateIEnumerator()
    {
        while (IsAvailableSpawn())
        {
            // @charotiti9 TODO: 랜덤 생성으로 간단히 테스트합니다.
            if (consumers.Count == 0)
            {
                Debug.LogError("추가된 손님이 하나도 없습니다. 생성할 손님 목록에 손님 스크립터블 오브젝트를 추가해주세요.");
                yield break;
            }
            var selectedConsumer = consumers[Random.Range(0, consumers.Count)];
            if (testConsumer != null) // 테스트하고 싶은 손님이 있으시군요.
            {
                selectedConsumer = testConsumer;
            }

            // @charotiti9 TODO: 나중에 풀링으로 바꿉니다. 지금은 테스트만...
            if(currentNumber < maxNumber)
            {
                OnCustomerEnter(selectedConsumer);
            }

            yield return new WaitForSeconds(appearTime);
        }
    }

    private void OnCustomerEnter(ConsumerScriptableObject consumer)
    {
        currentNumber++;
        GameObject consumerObject = Instantiate(consumer.consumerPrefab);
        consumerObject.AddComponent<SpriteRenderer>().sprite = consumer.appearance;
        // @charotiti9 TODO: 등장대사를 외친다. 지금은 print로 간단히 처리
        var line = consumer.dialogues.Where(w => w.state == ConsumerState.Enter).Select(s => s.line).ToList();
        print(string.Join(", ", line));

        var consumerScript = consumerObject.GetComponent<Consumer>();
        consumerScript.OnCustomerEnter();

        // 행동을 시작해요
        var currentConsumerId = consumerId++; // id를 저장해서 나중에 목록에서의 삭제를 용이하게 만듭니다.
        var currentConsumerCoroutine = StartCoroutine(UpdateCustomerBehavior(consumer, currentConsumerId, consumerScript));
        consumerCoroutines.Add(currentConsumerId, currentConsumerCoroutine);
    }

    private IEnumerator UpdateCustomerBehavior(ConsumerScriptableObject consumer, int id, Consumer consumerScript)
    {
        // 체류시간 계산
        var duration = 0f;
        while (duration < consumer.durationTime)
        {
            duration += Time.deltaTime;

            // @charotiti9 TODO: 각종 대사를 외쳐요. 나중에 손님 상태에 따라서 말하는 대사를 변경해야합니다.
            var state = consumerScript.state;
            var line = consumer.dialogues.Where(w => w.state == state).Select(s => s.line).ToList();
            print(string.Join(", ", line));

            yield return null;
        }

        OnCustomerExit(consumer, consumerScript);

        // 코루틴 목록에서도 삭제
        if (consumerCoroutines.ContainsKey(id))
        {
            consumerCoroutines.Remove(id);
        }
    }

    private void OnCustomerExit(ConsumerScriptableObject consumer, Consumer consumerScript)
    {
        currentNumber--;
        // @charotiti9 TODO: 퇴장대사를 외친다. 지금은 print로 간단히 처리
        var line = consumer.dialogues.Where(w => w.state == ConsumerState.Exit).Select(s => s.line).ToList();
        print(string.Join(", ", line));

        consumerScript.OnCustomerExit();

        // @charotiti9 TODO: 나중에 객체 비활성화로 바꿔야합니다.
        Destroy(consumerScript.gameObject);
    }
}

using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// 모든 손님에게 상속되어야합니다.
/// </summary>
public class Consumer : MonoBehaviour
{
    [SerializeField] internal ConsumerScriptableObject consumerScriptableObject;
    internal int id = -1;
    internal ConsumerState state;

    internal virtual void OnEnable()
    {
        StartCoroutine(EnterAfterValidate());
    }

    internal virtual void OnDisable()
    {
        OnCustomerExit();
        StopCoroutine(UpdateCustomerBehavior());
    }

    /// <summary>
    /// id가 부과되고 활동할 수 있을 때까지 대기합니다.
    /// </summary>
    /// <returns></returns>
    internal IEnumerator EnterAfterValidate()
    {
        yield return new WaitUntil(() => id != -1);

        OnCustomerEnter();
        StartCoroutine(UpdateCustomerBehavior());
    }

    /// <summary>
    /// 손님이 들어올 때 해야하는 행동
    /// </summary>
    internal virtual void OnCustomerEnter()
    {
        state = ConsumerState.Enter;

        gameObject.AddComponent<SpriteRenderer>().sprite = consumerScriptableObject.appearance;
        // @charotiti9 TODO: 등장대사를 외친다. 지금은 print로 간단히 처리
        var line = consumerScriptableObject.dialogues.Where(w => w.state == ConsumerState.Enter).Select(s => s.line).ToList();
        print($"손님{id}: {string.Join(", ", line)}");
    }

    /// <summary>
    /// 손님이 떠날 때 해야하는 행동
    /// </summary>
    internal virtual void OnCustomerExit()
    {
        state = ConsumerState.Exit;

        // @charotiti9 TODO: 퇴장대사를 외친다. 지금은 print로 간단히 처리
        var line = consumerScriptableObject.dialogues.Where(w => w.state == ConsumerState.Exit).Select(s => s.line).ToList();
        print($"손님{id}: {string.Join(", ", line)}");
    }

    /// <summary>
    /// 손님이 머무는 동안 해야하는 행동(update)
    /// </summary>
    internal virtual IEnumerator UpdateCustomerBehavior()
    {
        state = ConsumerState.Usual;

        // 체류시간 계산
        var duration = 0f;
        while (duration < consumerScriptableObject.durationTime)
        {
            duration += Time.deltaTime;

            // @charotiti9 TODO: 각종 대사를 외쳐요. 나중에 손님 상태에 따라서 말하는 대사를 변경해야합니다.
            var line = consumerScriptableObject.dialogues.Where(w => w.state == state).Select(s => s.line).ToList();
            print($"손님{id}: {string.Join(", ", line)}");

            yield return null;
        }

        // 체류시간이 지나면 삭제합니다.
        if (id == -1)
        {
            Debug.LogError("Id가 설정되지 않았습니다.");
            yield break;
        }
        ConsumerManager.Instance.DestroyConsumer(id);
    }
}

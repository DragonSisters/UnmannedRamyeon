using System.Collections;
using UnityEngine;

/// <summary>
/// 클릭을 해서 주의를 주어야할 필요성이 있는 손님의 동작을 이곳에서 정리합니다.
/// </summary>
public class WarningConsumer : Consumer
{
    /// <summary>
    /// 주의를 주어야하는 기간. 짧을수록 어렵다.
    /// </summary>
    [SerializeField] private float warningDuration = 2f;
    /// <summary>
    /// 기간안에 클릭되었는지 여부
    /// </summary>
    private bool isWarningTiming { get { return Time.time - (spawnedTime + consumerScriptableObject.UsualTime) < warningDuration; } }

    internal override IEnumerator UpdateCustomerBehavior()
    {
        // 체류시간 계산을 위해 부모의 함수도 함께 돌립니다.
        StartCoroutine(base.UpdateCustomerBehavior());

        // 일반 상태 지속
        yield return new WaitForSeconds(consumerScriptableObject.UsualTime);

        state = ConsumerState.Issue;
        while (isWarningTiming)
        {
            // @charotiti9 TODO: 클릭되었는지 여부를 검사해야합니다. 지금은 그냥 비워둡니다.

            yield return null;
        }

        // @charotiti9 TODO: 클릭되었는지 여부를 검사해야합니다. 지금은 그냥 비워둡니다.
        if (isWarningTiming)
        {
            state = ConsumerState.Smile;
        }
        else
        {
            state = ConsumerState.Upset;
        }
    }
}

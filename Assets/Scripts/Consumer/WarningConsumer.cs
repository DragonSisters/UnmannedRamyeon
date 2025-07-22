using System.Collections;
using UnityEngine;

/// <summary>
/// 클릭을 해서 주의를 주어야할 필요성이 있는 손님의 동작을 이곳에서 정리합니다.
/// </summary>
public class WarningConsumer : Consumer, IClickableSprite
{
    /// <summary>
    /// 주의를 주어야하는 기간. 짧을수록 어렵다.
    /// </summary>
    [SerializeField] private float warningDuration = 2f;
    /// <summary>
    /// 기간안에 클릭되었는지 여부
    /// </summary>
    private bool isWarningTiming { get { return Time.time - (spawnedTime + consumerScriptableObject.UsualTime) < warningDuration; } }

    /// <summary>
    /// 클릭할 수 있는 상태인지 여부
    /// </summary>
    public bool IsClickable => isClickable;
    private bool isClickable;

    internal override void OnEnter()
    {
        // 초기화
        isClickable = false;
    }
    internal override void OnExit() { }

    internal override IEnumerator OnUpdate()
    {
        // 일반 상태 지속
        yield return new WaitForSeconds(consumerScriptableObject.UsualTime);

        state = ConsumerState.Issue;

        // 이슈 상태 지속 = 클릭 가능한 상태
        isClickable = true;
        yield return new WaitUntil(() => !isWarningTiming|| IsIssueSolved); // 이슈가 해결되었다면 바로 넘어갑니다.
        isClickable = false;

        // 클릭되었는지 여부를 통해 판단합니다
        if (IsIssueSolved)
        {
            state = ConsumerState.Smile;
        }
        else
        {
            state = ConsumerState.Upset;
        }
    }
    public void OnSpriteClicked()
    {
        Debug.Log($"Clicked: {gameObject.name}");

        IsIssueSolved = true;
    }

}

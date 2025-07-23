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
    [SerializeField] private int issueResolvedBonus = 5;
    [SerializeField] private int issueUnresolvedPenalty = 20;

    /// <summary>
    /// 최소/최대 일반상태 유지 시간
    /// </summary>
    [SerializeField] private float minUsualTime = 2f, maxUsualTime = 4f;
    private float usualTime;

    /// <summary>
    /// 스폰된 시간
    /// </summary>
    private float? spawnedTime = null;

    /// <summary>
    /// 기간안에 클릭되었는지 여부
    /// </summary>
    private bool isWarningTiming { get { return Time.time - (spawnedTime + usualTime) < warningDuration; } }

    /// <summary>
    /// 클릭할 수 있는 상태인지 여부
    /// </summary>
    public bool IsClickable => isClickable;
    private bool isClickable;

    internal override void OnEnter()
    {
        // 초기화
        isClickable = false;
        spawnedTime = Time.time;
        usualTime = Random.Range(minUsualTime, maxUsualTime);
    }
    internal override void OnExit() { }

    internal override IEnumerator OnUpdate()
    {
        // 일반 상태 지속
        yield return new WaitForSeconds(usualTime);

        state = ConsumerState.Issue;

        // 이슈 상태 지속 = 클릭 가능한 상태
        isClickable = true;
        // 기분이 내려가기 시작합니다
        moodScript.StartDecrease();
        yield return new WaitUntil(() => !isWarningTiming|| IsIssueSolved); // 이슈가 해결되었다면 바로 넘어갑니다.
        moodScript.StopDecrease();
        isClickable = false;

        // 클릭되었는지 여부를 통해 판단합니다
        if (IsIssueSolved)
        {
            state = ConsumerState.IssueSolved;
            // 이슈가 해결되면 약간 증가시켜줍니다 (보상)
            moodScript.IncreaseMood(issueResolvedBonus);
        }
        else
        {
            state = ConsumerState.IssueUnsolved;
            // 이슈가 해결되지 않으면 만족도가 많이 떨어집니다
            moodScript.DecreaseMood(issueUnresolvedPenalty);
        }
    }
    public void OnSpriteClicked()
    {
        Debug.Log($"Clicked: {gameObject.name}");

        IsIssueSolved = true;
    }
}

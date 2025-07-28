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
    [SerializeField] private int issueResolvedBonus = 5;
    [SerializeField] private int issueUnresolvedPenalty = 20;

    /// <summary>
    /// 최소/최대 일반상태 유지 시간
    /// </summary>
    [SerializeField] private float minUsualTime = 2f, maxUsualTime = 4f;

    /// <summary>
    /// 스폰된 시간
    /// </summary>
    private float? spawnedTime = null;

    internal override void OnEnter()
    {
        spawnedTime = Time.time;
    }
    internal override void OnExit() { }

    internal override IEnumerator OnUpdate()
    {
        var usualTime = Random.Range(minUsualTime, maxUsualTime);

        yield return new WaitForSeconds(usualTime);

        // 이슈상태 시작
        SetState(ConsumerState.Issue);

        // 기분이 내려가기 시작합니다
        moodScript.StartDecrease();

        yield return new WaitUntil(() => 
        (Time.time - (spawnedTime + usualTime) > warningDuration) // 주의를 주어야하는 기간이라면 기다립니다.
        || IsIssueSolved); // 이슈가 해결되었다면 바로 넘어갑니다.

        moodScript.StopDecrease();

        // 클릭되었는지 여부를 통해 판단합니다
        if (IsIssueSolved)
        {
            SetState(ConsumerState.IssueSolved);
            // 이슈가 해결되면 약간 증가시켜줍니다 (보상)
            moodScript.IncreaseMood(issueResolvedBonus);
        }
        else
        {
            SetState(ConsumerState.IssueUnsolved);
            // 이슈가 해결되지 않으면 만족도가 많이 떨어집니다
            moodScript.DecreaseMood(issueUnresolvedPenalty);
        }
    }

    internal override void OnClick()
    {
        Debug.Log($"Clicked: {gameObject.name}");

        IsIssueSolved = true;
    }
}

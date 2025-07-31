using System.Collections;
using UnityEngine;

public class ThiefConsumer : Consumer
{
    /// <summary>
    /// 최소/최대 일반상태 유지 시간
    /// </summary>
    [SerializeField] private float minUsualTime = 2f, maxUsualTime = 4f;
    [SerializeField] private int issueResolvedBonus = 5;
    [SerializeField] private int issueUnresolvedPenalty = 20;
    [SerializeField] internal float issueDuration = 20;
    /// <summary>
    /// 주의를 주어야하는 기간. 짧을수록 어렵다.
    /// </summary>
    [SerializeField] private float warningDuration = 2f;

    internal override void HandleChildEnter() { }

    internal override void HandleChildExit() { }

    // @charotiti9 TODO: 도둑은 임시구현으로 두고, 추후에 구현합니다. 현재는 스크립트 분리만 해둡니다.
    internal override void HandleChildClick() { }

    internal override IEnumerator HandleChildUpdate()
    {
        var usualTime = Random.Range(minUsualTime, maxUsualTime);

        yield return new WaitForSeconds(usualTime);

        // 이슈상태 시작
        SetState(ConsumerState.Issue);

        // 기분이 내려가기 시작합니다
        moodScript.StartDecrease();

        yield return new WaitUntil(() =>
        (Time.time - (spawnedTime + usualTime) > issueDuration) // 주의를 주어야하는 기간이라면 기다립니다.
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
}

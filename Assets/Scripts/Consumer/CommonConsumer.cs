using System.Collections;
using UnityEngine;

/// <summary>
/// 클릭을 해서 주의를 주어야할 필요성이 있는 손님의 동작을 이곳에서 정리합니다.
/// </summary>
public class CommonConsumer : Consumer
{
    internal override void HandleChildEnter() { }

    internal override void HandleChildExit() { }

    internal override void HandleChildClick() { }

    internal override IEnumerator HandleChildUpdate() 
    {
        yield break;
    }


    // @charotiti9 TODO: 불이 나는 등의 무드가 떨어질 일이 생기면 
    // Consumer에 IsAllIssueState(임시명) 등의 변수를 HandleChildUpdate에서 보고 TickMoodPenalty를 적용시킬 것.
    // 그리고 해결되면 Consumer에서 IsAllIssueState && IsIssueSolved == true될테니 자동으로 회복됨
    private IEnumerator TickMoodPenalty()
    {
        var issueStartedTime = Time.time;

        // 이슈상태 시작
        SetState(ConsumerState.Issue);

        // 기분이 내려가기 시작합니다
        moodScript.StartDecrease();

        yield return new WaitUntil(() =>
        (Time.time - (spawnedTime + issueStartedTime) > issueDuration) // 이슈가 해결되지 않았다면 이슈기간동안 기다립니다.
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

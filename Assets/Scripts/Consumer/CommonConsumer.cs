using System.Collections;
using UnityEngine;

/// <summary>
/// 클릭을 해서 주의를 주어야할 필요성이 있는 손님의 동작을 이곳에서 정리합니다.
/// </summary>
public class CommonConsumer : Consumer
{
    [SerializeField] internal int issueResolvedBonus = 5;
    [SerializeField] internal int issueUnresolvedPenalty = 20;
    [SerializeField] internal float issueDuration = 20;
    /// <summary>
    /// 주의를 주어야하는 기간. 짧을수록 어렵다.
    /// </summary>
    [SerializeField] private float warningDuration = 2f;

    internal bool IsIssueSolved;

    /// <summary>
    /// 스폰된 시간
    /// </summary>
    internal float? spawnedTime = null;

    internal override void HandleChildEnter()
    {
        spawnedTime = Time.time;
        IsIssueSolved = false;
    }

    internal override void HandleChildExit() { }

    internal override void HandleChildClick() 
    {
        // 이슈상태라면 재료는 보이지 않고 자식컴포넌트의 함수를 실행합니다.
        if (State == ConsumerState.Issue)
        {
            return;
        }

        // 이슈상태가 아니라면 재료가 보이도록 합니다.
        consumerUI.ActivateIngredientUI(true);
    }
    internal override void HandleChildUnclicked()
    {
        // 다른 스프라이트가 클릭되었다면 재료가 사라집니다.
        consumerUI.ActivateIngredientUI(false);
    }

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

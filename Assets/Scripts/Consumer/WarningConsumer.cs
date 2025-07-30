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

    internal override void HandleChildEnter()
    {
        spawnedTime = Time.time;
        issueDuration = warningDuration;
    }

    internal override void HandleChildExit() { }

    internal override void HandleChildClick()
    {
        Debug.Log($"Clicked: {gameObject.name}");

        IsIssueSolved = true;
    }
}

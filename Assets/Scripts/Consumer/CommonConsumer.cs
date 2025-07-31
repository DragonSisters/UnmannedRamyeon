using System.Collections;
using UnityEngine;

/// <summary>
/// 클릭을 해서 주의를 주어야할 필요성이 있는 손님의 동작을 이곳에서 정리합니다.
/// </summary>
public class CommonConsumer : Consumer
{
    internal override void HandleChildEnter()
    {
        spawnedTime = Time.time;
    }

    internal override void HandleChildExit() { }

    internal override void HandleChildClick() { }

    internal override IEnumerator HandleChildUpdate() 
    {
        yield break;
    }
}

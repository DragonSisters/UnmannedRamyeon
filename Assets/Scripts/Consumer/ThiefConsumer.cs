using System.Collections;
using UnityEngine;

public class ThiefConsumer : Consumer
{
    internal override void HandleChildEnter() { }

    internal override void HandleChildExit() { }

    // @charotiti9 TODO: 도둑은 임시구현으로 두고, 추후에 구현합니다. 현재는 스크립트 분리만 해둡니다.
    internal override void HandleChildClick() { }

    internal override IEnumerator HandleChildUpdate()
    {
        yield break;
    }
}

using UnityEngine;

public class ConsumerSpeech : MonoBehaviour
{
    private ConsumerUI consumerUI;

    public void Speech(ConsumerScriptableObject consumerScriptableObject, ConsumerState state)
    {
        // Invalid 초기화일 때는 대사를 건너뜁니다.
        if (state == ConsumerState.Invalid)
        {
            throw new System.Exception($"초기화되지 않은 상태로 손님이 말하려고 했습니다.");
        }
        var line = consumerScriptableObject.GetDialogueFromState(state);
        print($"손님{gameObject.name}: {string.Join(", ", line)}");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumerSpeech : MonoBehaviour
{
    private ConsumerUI consumerUI;
    private const float SPEECH_INTERVAL = 0.1f;
    private const float SPEECH_END_WAIT_TIME = 3f;

    public void Initialize(in ConsumerUI consumerUI) // in키워드 = call by ref
    {
        this.consumerUI = consumerUI;
    }

    public IEnumerator Speech(ConsumerScriptableObject consumerScriptableObject, ConsumerState state)
    {
        // Invalid 초기화일 때는 대사를 건너뜁니다.
        if (state == ConsumerState.Invalid)
        {
            throw new System.Exception($"초기화되지 않은 상태로 손님이 말하려고 했습니다.");
        }
        var line = consumerScriptableObject.GetDialogueFromState(state);

        consumerUI.ActivateSpeechBubbleUI(true);

        // 한글자씩 말하게 합니다.
        var queue = new Queue<char>(line);
        var newLine = "";
        while (queue.Count > 0) 
        {
            newLine += queue.Dequeue();
            consumerUI.SetSpeechBubbleText($"{newLine}");
            yield return new WaitForSeconds(SPEECH_INTERVAL);
        }
       
        // 다 말하면 n초 기다리고 사라집니다.
        yield return new WaitForSeconds(SPEECH_END_WAIT_TIME);
        consumerUI.ActivateSpeechBubbleUI(false);
    }
}

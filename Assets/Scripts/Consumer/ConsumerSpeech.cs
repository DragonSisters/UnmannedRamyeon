using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumerSpeech : MonoBehaviour
{
    private ConsumerUI consumerUI;
    private const float SPEECH_INTERVAL = 0.1f;
    private const float SPEECH_END_WAIT_TIME = 3f;

    Coroutine SpeechCoroutine;

    private bool isSpeaking;

    public void Initialize(in ConsumerUI consumerUI) // in키워드 = call by ref
    {
        this.consumerUI = consumerUI;
        SpeechCoroutine = null;
    }

    public void StopSpeech()
    {
        if (SpeechCoroutine != null)
        {
            StopCoroutine(SpeechCoroutine);
        }
        consumerUI.ActivateSpeechBubbleUI(false);
        isSpeaking = false;
    }

    public void StartSpeechFromSituation(
        ConsumerScriptableObject consumerScriptableObject, 
        ConsumerSituation situation,
        bool isRandom,
        bool hasFormat,
        int index = -1,
        string format = "")
    {
        // 말하고 있는게 있다면 멈추고 말하게 합니다.
        if(isSpeaking)
        {
            StopSpeech();
        }
        // Invalid 초기화일 때는 대사를 건너뜁니다.
        if (situation == ConsumerSituation.Invalid)
        {
            throw new System.Exception($"초기화되지 않은 상태로 손님이 말하려고 했습니다.");
        }
        if (!isRandom && index >= 0)
        {
            throw new System.Exception($"랜덤이 아닌데 인덱스를 설정하지 않았습니다.");
        }
        if (hasFormat && string.IsNullOrEmpty(format))
        {
            throw new System.Exception($"format이 있는데 매개변수를 설정하지 않았습니다.");
        }

        var line = isRandom ?
            consumerScriptableObject.GetRandomDialogueFromSituation(situation, format)
            : consumerScriptableObject.GetDialogueFromSituation(situation, index, format);

        // 해당 state에 대사가 있었을 경우에만 재생합니다.
        if (!string.IsNullOrEmpty(line))
        {
            SpeechCoroutine = StartCoroutine(Speech(line));
        }
    }

    public void StartSpeechFromState(
        ConsumerScriptableObject consumerScriptableObject, 
        ConsumerState state, 
        bool isRandom, 
        bool hasFormat,
        int index = -1,
        string format = "")
    {
        // 말하고 있는게 있다면 멈추고 말하게 합니다.
        if (isSpeaking)
        {
            StopSpeech();
        }
        // Invalid 초기화일 때는 대사를 건너뜁니다.
        if (state == ConsumerState.Invalid)
        {
            throw new System.Exception($"초기화되지 않은 상태로 손님이 말하려고 했습니다.");
        }
        if (!isRandom && index >= 0) 
        {
            throw new System.Exception($"랜덤이 아닌데 인덱스를 설정하지 않았습니다.");
        }
        if(hasFormat && string.IsNullOrEmpty(format))
        {
            throw new System.Exception($"format이 있는데 매개변수를 설정하지 않았습니다.");
        }

        var line = isRandom ? 
            consumerScriptableObject.GetRandomDialogueFromState(state, format) 
            : consumerScriptableObject.GetDialogueFromState(state, index, format);

        // 해당 state에 대사가 있었을 경우에만 재생합니다.
        if (!string.IsNullOrEmpty(line))
        {
            SpeechCoroutine = StartCoroutine(Speech(line));
        }
    }

    private IEnumerator Speech(string line)
    {
        isSpeaking = true;

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

        isSpeaking = false;
    }
}

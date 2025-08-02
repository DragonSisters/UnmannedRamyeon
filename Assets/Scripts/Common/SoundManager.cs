using System;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    // AudioSource
    public AudioSource BgmAudio;
    public AudioSource EffectAudio;

    /// <summary>
    /// audio clip 을 여러 개 담아 놓을 변수
    /// Effect 를 세부적으로 나누고 싶으면 더 나눠도 됩니다. 작은 볼륨의 게임이라 일단은 하나로 만들었습니다.
    /// </summary>
    public AudioClip[] BgmClips;
    public AudioClip[] EffectClips;

    private int startIndex = 0;

    void Start()
    {
        PlayBgmSound(startIndex);
    }

    public void PlayBgmSound(int index)
    {
        BgmAudio.clip = BgmClips[index];
        BgmAudio.Play();
    }

    public void PlayEffectSound(int index)
    {
        EffectAudio.PlayOneShot(EffectClips[index]);
    }
}

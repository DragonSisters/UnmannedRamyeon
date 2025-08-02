using System;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    // AudioSource
    public AudioSource BgmAudio;
    public AudioSource EffectAudio;

    // audio clip 을 여러 개 담아 놓을 변수
    public AudioClip[] BgmClips;
    public AudioClip[] EffectClips;

    private int startIndex = 0;

    void Start()
    {
        PlayBgmSound(startIndex);
    }

    private void PlayBgmSound(int index)
    {
        BgmAudio.PlayOneShot(BgmClips[index]);
    }
}

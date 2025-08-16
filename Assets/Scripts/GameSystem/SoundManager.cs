using System;
using System.Collections.Generic;
using UnityEngine;

public enum BgmSoundType
{
    Start,
    InGame,
    End
}

public enum EffectSoundType
{
    Click,
    Success,
    Fail,
    CoinGain,
    CoinLoss,
    WrongIngredientPick,
    WrongIngredientCorrected
}

[Serializable]
public class BgmSoundEntry
{
    public BgmSoundType Type;
    public AudioClip Clip;
}

[Serializable]
public class EffectSoundEntry
{
    public EffectSoundType Type;
    public AudioClip Clip;
}


public class SoundManager : Singleton<SoundManager>
{
    /// <summary>
    /// AudioSource
    /// </summary>
    public AudioSource EffectAudio;
    public AudioSource BgmAudio;

    /// <summary>
    /// audio clip 을 여러 개 담아 놓을 변수
    /// Effect 를 세부적으로 나누고 싶으면 더 나눠도 됩니다. 작은 볼륨의 게임이라 일단은 하나로 만들었습니다.
    /// </summary>
    [Header("Effect Clips")]
    public List<EffectSoundEntry> EffectSoundEntries = new();
    [Header("BGM Clips")]
    public List<BgmSoundEntry> BgmSoundEntries = new();

    private Dictionary<EffectSoundType, AudioClip> effectSoundDict = new();
    private Dictionary<BgmSoundType, AudioClip> bgmSoundDict = new();

    private void Awake()
    {
        InitializeDictionary();
    }

    void Start()
    {
        PlayBgmSound(BgmSoundType.Start);
    }

    private void InitializeDictionary()
    {
        foreach(var entry in BgmSoundEntries)
        {
            if (!bgmSoundDict.ContainsKey(entry.Type)) bgmSoundDict.Add(entry.Type, entry.Clip);
        }

        foreach (var entry in EffectSoundEntries)
        {
            if (!effectSoundDict.ContainsKey(entry.Type)) effectSoundDict.Add(entry.Type, entry.Clip);
        }
    }

    public void PlayEffectSound(EffectSoundType type)
    {
        if (effectSoundDict.TryGetValue(type, out var clip))
        {
            EffectAudio.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"EffectSoundType {type}에 해당하는 사운드가 없습니다.");
        }
    }

    public void PlayBgmSound(BgmSoundType type)
    {
        if (bgmSoundDict.TryGetValue(type, out var clip))
        {
            BgmAudio.clip = clip;
            BgmAudio.Play();
        }
        else
        {
            Debug.LogWarning($"BgmSoundType {type}에 해당하는 사운드가 없습니다.");
        }
    }
}

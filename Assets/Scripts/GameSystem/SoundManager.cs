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
    WrongIngredientCorrected,
    GameStart
}

public enum ContinousSoundType
{
    Invalid,
    TrashCleaning
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

[Serializable]
public class ContinousSoundEntry
{
    public ContinousSoundType Type;
    public AudioClip Clip;
}


public class SoundManager : Singleton<SoundManager>
{
    /// <summary>
    /// AudioSource
    /// </summary>
    public AudioSource EffectAudio;
    public AudioSource BgmAudio;
    public AudioSource ContinousAudio;

    /// <summary>
    /// Audio Volume
    /// </summary>
    [Range(0f, 1f)] private float bgmVolume = 0.5f;     // BGM 볼륨
    [Range(0f, 1f)] private float effectVolume = 0.5f;  // 효과음(Effect + Continuous) 볼륨

    /// <summary>
    /// audio clip 을 여러 개 담아 놓을 변수
    /// </summary>
    [Header("Effect Clips")]
    public List<EffectSoundEntry> EffectSoundEntries = new();
    [Header("BGM Clips")]
    public List<BgmSoundEntry> BgmSoundEntries = new();
    [Header("Continous Clips")]
    public List<ContinousSoundEntry> ContinousSoundEntries = new();

    private Dictionary<EffectSoundType, AudioClip> effectSoundDict = new();
    private Dictionary<BgmSoundType, AudioClip> bgmSoundDict = new();
    private Dictionary<ContinousSoundType, AudioClip> continousSoundDict = new();

    private ContinousSoundType curContinousSoundType = ContinousSoundType.Invalid;

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

        foreach (var entry in ContinousSoundEntries)
        {
            if (!continousSoundDict.ContainsKey(entry.Type)) continousSoundDict.Add(entry.Type, entry.Clip);
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

    public void PlayContinousSound(ContinousSoundType type)
    {
        // 연속되는 사운드가 이미 재생 중이면 중복 재생 방지
        if (ContinousAudio.isPlaying && curContinousSoundType == type)
        {
            return;
        }

        curContinousSoundType = type;
        if (continousSoundDict.TryGetValue(type, out var clip))
        {
            ContinousAudio.clip = clip;
            ContinousAudio.Play();
        }
        else
        {
            Debug.LogWarning($"ContinousSoundType {type}에 해당하는 사운드가 없습니다.");
        }
    }

    public void StopContinousSound(ContinousSoundType type)
    {
        // 연속 사운드가 재생 중이고, 현재 타입이 일치하는 경우에만 중지
        if(ContinousAudio.isPlaying && curContinousSoundType == type)
        {
            ContinousAudio.Stop();
            curContinousSoundType = ContinousSoundType.Invalid; // 현재 연속 사운드 타입 초기화
        }
    }

    public void SetEffectVolume(float volume)
    {
        effectVolume = Mathf.Clamp01(volume);
        EffectAudio.volume = effectVolume;
        ContinousAudio.volume = effectVolume;
    }

    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        BgmAudio.volume = bgmVolume;
    }
}

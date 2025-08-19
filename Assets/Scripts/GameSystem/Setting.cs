using UnityEngine;
using UnityEngine.UI;

public class Setting : MonoBehaviour
{
    [SerializeField] private GameObject panel_Setting;
    [SerializeField] private Slider slider_bgm;
    [SerializeField] private Slider slider_effect;

    // 세팅 아이콘 버튼 (btn_Setting) 에 연결된 함수입니다.
    public void OnSettingClick()
    {
        panel_Setting.SetActive(true);
        SoundManager.Instance.PlayEffectSound(EffectSoundType.Click);
    }

    // 돌아가기 버튼 (btn_Return) 에 연결된 함수입니다.
    public void OnReturnClick()
    {
        panel_Setting.SetActive(false);
        SoundManager.Instance.PlayEffectSound(EffectSoundType.Click);
    }

    // Bgm 슬라이더 (slider_Bgm) 에 연결된 함수입니다.
    public void OnBgmVolumeChanged()
    {
        SoundManager.Instance.SetBgmVolume(slider_bgm.value);
    }

    // Effect 슬라이더 (slider_Effect) 에 연결된 함수입니다.
    public void OnEffectVolumeChanged()
    {
        SoundManager.Instance.SetEffectVolume(slider_effect.value);
    }

    // 슬라이더 오브젝트들 (slider_Bgm / slider_Effect) 에 EventTrigger - OnPointerUp 으로 연결되는 함수입니다.
    public void OnPointerUp()
    {
        SoundManager.Instance.PlayEffectSound(EffectSoundType.Click);
    }
}

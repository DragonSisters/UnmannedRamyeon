using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [Header("시작 화면 관련 변수들")]
    [SerializeField] private GameObject startCanvas;
    [SerializeField] private Animation titleAnimation;
    [SerializeField] private Texture2D cursorIcon;
    public Texture2D CursorIcon => cursorIcon;
    [Header("컷신 관련 변수들")]
    [SerializeField] private GameObject storyCanvas;
    [SerializeField] private GameObject[] storyCutscenes;
    [SerializeField] private GameObject btn_start;

    [Header("인게임 화면 관련 변수들")]
    [SerializeField] private GameObject inGameCanvas;
    [SerializeField] private Timer timer;
    [SerializeField] private Button btn_easyMode;
    [SerializeField] private Button btn_hardMode;
    [SerializeField] public PotUIController PotUIController;

    [Header("EndCanvas 관련 변수들")]
    [SerializeField] private GameObject endCanvas;
    [SerializeField] private GameObject img_Success;
    [SerializeField] private TMP_Text txt_MoneySuccess;
    [SerializeField] private GameObject img_Fail;
    [SerializeField] private TMP_Text txt_MoneyFail;

    public void SetCursor(Texture2D icon = null)
    {
        if (icon == null) icon = cursorIcon;
        Cursor.SetCursor(icon, Vector2.zero, CursorMode.Auto);
    }

    public void ResetCursor()
    {
        Cursor.SetCursor(cursorIcon, Vector2.zero, CursorMode.Auto);
    }

    public void SetModeButtons()
    {
        btn_easyMode.onClick.AddListener(() => GameManager.Instance.SetMode(false));
        btn_hardMode.onClick.AddListener(() => GameManager.Instance.SetMode(true));
    }

    // 이지 모드, 하드 모드 버튼에 연결된 함수입니다
    public void OnDifficultyButtonClick()
    {
        SoundManager.Instance.PlayEffectSound(EffectSoundType.GameStart);
        StartCoroutine(ShowStoryCoroutine());
    }

    // 이지 모드, 하드 모드 버튼 후에 나오는 스토리 컷씬에서 시작하기 버튼에 연결된 함수입니다
    public void OnStartButtonClick()
    {
        SoundManager.Instance.PlayEffectSound(EffectSoundType.Click);
        storyCanvas.SetActive(false);
        GameManager.Instance.StartGame();
    }

    // EndCanvas 의 다시 시작 버튼에 연결되어 있는 함수입니다.
    public void OnRestartButtonClick()
    {
        SoundManager.Instance.PlayEffectSound(EffectSoundType.Click);

        ControlEndCanvas(false);
        ControlStartCanvas(true);

        SoundManager.Instance.PlayBgmSound(BgmSoundType.Start);
    }

    public IEnumerator ShowStoryCoroutine()
    {
        if (!titleAnimation.isPlaying)
        {
            titleAnimation.Play();
        }

        yield return new WaitUntil(() => !titleAnimation.isPlaying);

        startCanvas.SetActive(false);
        storyCanvas.SetActive(true);

        for (int i = 0; i < storyCutscenes.Length; i++)
        {
            storyCutscenes[i].SetActive(true);

            // 마우스 클릭을 기다립니다
            yield return new WaitUntil(() => Input.GetMouseButtonUp(0));
            yield return null;
        }

        btn_start.SetActive(true);
    }

    public void HideCutsceneCanvas()
    {
        storyCanvas.SetActive(false);
        for (int i = 0; i < storyCutscenes.Length; i++)
        {
            storyCutscenes[i].SetActive(false);
        }
        btn_start.SetActive(false);
    }
    
    public void ControlStartCanvas(bool isActive)
    {
        startCanvas.SetActive(isActive);
    }

    public void ControlInGameCanvas(bool isActive)
    {
        inGameCanvas.SetActive(isActive);
    }

    public void ControlEndCanvas(bool isActive)
    {
        endCanvas.SetActive(isActive);
        if(isActive)
        {
            if (FinanceManager.Instance.IsSuccess)
            {
                img_Success.SetActive(true);
                img_Fail.SetActive(false);
                txt_MoneySuccess.text = $"{FinanceManager.Instance.CurrentMoney.ToString()}원";
                SoundManager.Instance.PlayBgmSound(BgmSoundType.Success);
            }
            else
            {
                img_Fail.SetActive(true);
                img_Success.SetActive(false);
                txt_MoneyFail.text = $"{FinanceManager.Instance.CurrentMoney.ToString()}원";
                SoundManager.Instance.PlayBgmSound(BgmSoundType.Fail);
            }
        }
    }

    public void ExecuteTimer()
    {
        timer.ExecuteTimer();
    }
}

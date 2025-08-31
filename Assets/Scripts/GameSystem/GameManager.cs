using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    [Header("시작 화면 관련 변수들")]
    [SerializeField] private GameObject startCanvas;
    [SerializeField] private Animation startGameAnimation;
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
    [SerializeField] private float gameDuration = 180;
    public float GameDuration => gameDuration;
    [SerializeField] private float earlyStageTime;
    public float EarlyStageTime => earlyStageTime;
    private float easyEarlyStageTime = 60;
    private float hardEarlyStageTime = 0;
    public float GameStartTime => gameStartTime;
    private float gameStartTime;

    public bool IsGameStarted => isGameStarted;
    private bool isGameStarted;
    public bool IsHardMode => isHardMode;
    private bool isHardMode = false;

    [Header("EndCanvas 관련 변수들")]
    [SerializeField] private GameObject endCanvas;
    [SerializeField] private GameObject img_Success;
    [SerializeField] private TMP_Text txt_MoneySuccess;
    [SerializeField] private GameObject img_Fail;
    [SerializeField] private TMP_Text txt_MoneyFail;

    #region 방향성 변경에 따른 제어 변수
    // 참고: https://github.com/DragonSisters/UnmannedRamyeonObsidian/blob/main/00_공지/방향성에%20대한%20논의%20회의록.md
    // * 일반 손님이 재료를 잘못가져오는 기능을 넣을지 여부
    public readonly bool CommonConsumerWrongPickFeature = false;
    // * 쓰레기를 치우는 기능을 넣을지 여부
    public readonly bool UseTrashFeature = false;
    // * RecipeConsumer 가 타이머를 사용하는 기능을 넣을지 여부
    public readonly bool UseRecipeConsumerTimer = false;
    // * CommonConsumer 가 나갈 때 코인에 영향을 미치는지에 대한 여부
    public readonly bool UseCommonConsumerCoin = false;
    #endregion

    private void Start()
    {
        SetCursor(cursorIcon);
        SetModeButtons();
        HideCutsceneCanvas();
        ConsumerManager.Instance.InitializeConsumerManagerSetting();
        TrashManager.Instance.Initialize();
    }

    public void SetCursor(Texture2D icon)
    {
        Cursor.SetCursor(icon, Vector2.zero, CursorMode.Auto);
    }

    public void ResetCursor()
    {
        Cursor.SetCursor(cursorIcon, Vector2.zero, CursorMode.Auto);
    }

    private void SetModeButtons()
    {
        btn_easyMode.onClick.AddListener(() => isHardMode = false);
        btn_hardMode.onClick.AddListener(() => isHardMode = true);  
    }

    // 이지 모드, 하드 모드 버튼에 연결된 함수입니다
    public void OnDifficultyButtonClick()
    {
        SoundManager.Instance.PlayEffectSound(EffectSoundType.GameStart);
        StartCoroutine(nameof(ShowStoryCoroutine));
    }

    // 이지 모드, 하드 모드 버튼 후에 나오는 스토리 컷씬에서 시작하기 버튼에 연결된 함수입니다
    public void OnStartButtonClick()
    {
        SoundManager.Instance.PlayEffectSound(EffectSoundType.Click);
        storyCanvas.SetActive(false);
        StartGame();
    }

    private void HideCutsceneCanvas()
    {
        storyCanvas.SetActive(false);
        for (int i = 0; i < storyCutscenes.Length; i++)
        {
            storyCutscenes[i].SetActive(false);
        }
        btn_start.SetActive(false);
    }

    private IEnumerator ShowStoryCoroutine()
    {
        if (!startGameAnimation.isPlaying)
        {
            startGameAnimation.Play();
        }

        yield return new WaitUntil(() => !startGameAnimation.isPlaying);

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

    // inGameUI 를 활성화합니다
    private void StartGame()
    {
        isGameStarted = true;
        gameStartTime = Time.time;
        HideCutsceneCanvas();
        inGameCanvas.SetActive(true);
        StartCoroutine(UpdateGame());

        earlyStageTime = isHardMode ? hardEarlyStageTime : easyEarlyStageTime;

        FinanceManager.Instance.OnGameEnter();
        ConsumerManager.Instance.StartSpawn(isHardMode);
        TrashManager.Instance.StartSpawn();
        IngredientManager.Instance.CreateIngredientObjOnPosition();
        MoveManager.Instance.OnGameEnter();
        timer.ExecuteTimer();
        SoundManager.Instance.PlayBgmSound(BgmSoundType.InGame);
    }

    private IEnumerator UpdateGame()
    {
        while (isGameStarted)
        {
            SpriteDragHandler.Instance.UpdateHandler();
            SpriteClickHandler.Instance.UpdateHandler();
            yield return null;
        }
    }

    public void EndGame()
    {
        isGameStarted = false;
        inGameCanvas.SetActive(false);

        FinanceManager.Instance.OnGameEnd();
        // 씬에 나온 손님들 모두를 없애고, 스폰루틴을 중지합니다.
        ConsumerManager.Instance.StopSpawn();
        TrashManager.Instance.StopSpawn();
        // 생성했던 재료들을 모두 비활성화합니다.
        IngredientManager.Instance.OnGameEnd();

        // 콤보 초기화
        ComboManager.Instance.ResetCombo();

        // 게임 결과에 따라 성공/실패 화면을 불러옵니다.
        endCanvas.SetActive(true);
        if (FinanceManager.Instance.IsSuccess)
        {
            img_Success.SetActive(true);
            txt_MoneySuccess.text = $"{FinanceManager.Instance.CurrentMoney.ToString()}원";
            SoundManager.Instance.PlayBgmSound(BgmSoundType.Success);
        }
        else
        {
            img_Fail.SetActive(true);
            txt_MoneyFail.text = $"{FinanceManager.Instance.CurrentMoney.ToString()}원";
            SoundManager.Instance.PlayBgmSound(BgmSoundType.Fail);
        }
    }

    public void OnRestartButtonClick()
    {
        SoundManager.Instance.PlayEffectSound(EffectSoundType.Click);

        endCanvas.SetActive(false);
        startCanvas.SetActive(true);

        SoundManager.Instance.PlayBgmSound(BgmSoundType.Start);
    }
}

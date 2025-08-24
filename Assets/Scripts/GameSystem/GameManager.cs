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

    [Header("인게임 화면 관련 변수들")]
    [SerializeField] private GameObject inGameCanvas;
    [SerializeField] private Timer timer;
    [SerializeField] private Button btn_easyMode;
    [SerializeField] private Button btn_hardMode;
    [SerializeField] private float gameDuration = 180;
    public float GameDuration => gameDuration;
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

    private void Start()
    {
        SetCursor(cursorIcon);
        SetModeButtons();
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

    // Start 버튼에 연결된 함수입니다
    public void OnStartButtonClick()
    {
        SoundManager.Instance.PlayEffectSound(EffectSoundType.GameStart);
        StartCoroutine(nameof(StartGameEffect));
    }

    private IEnumerator StartGameEffect()
    {
        if (!startGameAnimation.isPlaying)
        {
            startGameAnimation.Play();
        }

        yield return new WaitUntil(() => !startGameAnimation.isPlaying);

        StartGame();
    }

    // inGameUI 를 활성화합니다
    private void StartGame()
    {
        isGameStarted = true;
        gameStartTime = Time.time;
        startCanvas.SetActive(false);
        inGameCanvas.SetActive(true);
        StartCoroutine(UpdateGame());

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

        // 게임 결과에 따라 성공/실패 화면을 불러옵니다.
        // @anditsoon TODO: 게임 결과창에 최종 금액 나오게 하기 -> 추후 아트 받아서 적용 후 구현하겠습니다
        endCanvas.SetActive(true);
        if (FinanceManager.Instance.IsSuccess)
        {
            img_Success.SetActive(true);
            txt_MoneySuccess.text = $"{FinanceManager.Instance.CurrentMoney.ToString()}원";
        }
        else
        {
            img_Fail.SetActive(true);
            txt_MoneyFail.text = $"{FinanceManager.Instance.CurrentMoney.ToString()}원";
        }

        SoundManager.Instance.PlayBgmSound(BgmSoundType.End);
    }

    public void OnRestartButtonClick()
    {
        SoundManager.Instance.PlayEffectSound(EffectSoundType.Click);

        endCanvas.SetActive(false);
        startCanvas.SetActive(true);

        SoundManager.Instance.PlayBgmSound(BgmSoundType.Start);
    }
}

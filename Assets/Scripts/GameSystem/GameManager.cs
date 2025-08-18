using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : Singleton<GameManager>
{
    [Header("시작 화면 관련 변수들")]
    [SerializeField] private GameObject startCanvas;
    [SerializeField] private GameObject btn_Start;
    [SerializeField] private GameObject img_Start;
    [SerializeField] private Texture2D cursorIcon;
    public Texture2D CursorIcon => cursorIcon;
    private const float START_DELAY_TIME = 1f;

    [Header("인게임 화면 관련 변수들")]
    [SerializeField] private GameObject inGameCanvas;
    [SerializeField] private float gameDuration = 180;
    public float GameDuration => gameDuration;
    public float GameStartTime => gameStartTime;
    private float gameStartTime;

    public bool IsGameStarted => isGameStarted;
    private bool isGameStarted;

    [Header("EndCanvas 관련 변수들")]
    [SerializeField] private GameObject endCanvas;
    [SerializeField] private GameObject img_Success;
    [SerializeField] private TMP_Text txt_MoneySuccess;
    [SerializeField] private GameObject img_Fail;
    [SerializeField] private TMP_Text txt_MoneyFail;

    private void Start()
    {
        SetCursor(cursorIcon);
        ConsumerManager.Instance.InitializeConsumerManagerSetting();
        TrashManager.Instance.Initialize();
    }

    public void SetCursor(Texture2D icon)
    {
        Cursor.SetCursor(icon, Vector2.zero, CursorMode.Auto);
    }

    // Start 버튼에 연결할 함수입니다
    public void OnStartButtonClick()
    {
        StartCoroutine(nameof(UnableStartUI));
    }

    // 시작 화면에서 버튼이 사라지고, 게임이 시작된다는 UI (img_Start) 가 나옵니다.
    // @anditsoon TODO: 지금은 임시로 1초 간 나타났다 사라지게 만들었지만, 추후 날아오는 효과라던가 깜박이는 효과 등을 추가할 예정입니다.
    // 1초 뒤 게임 시작 UI 가 사라지고 인게임 UI 가 나타납니다.
    private IEnumerator UnableStartUI()
    {
        SoundManager.Instance.PlayBgmSound(BgmSoundType.InGame);
        btn_Start.SetActive(false);
        img_Start.SetActive(true);
        yield return new WaitForSeconds(START_DELAY_TIME);
        img_Start.SetActive(false);
        StartGame();
    }

    // inGameUI 를 활성화합니다
    // 활성화되며 캔버스에 붙어있는 TimerUI 가 자동으로 실행됩니다
    private void StartGame()
    {
        isGameStarted = true;
        gameStartTime = Time.time;
        startCanvas.SetActive(false);
        inGameCanvas.SetActive(true);
        StartCoroutine(UpdateGame());

        FinanceManager.Instance.OnGameEnter();
        ConsumerManager.Instance.StartSpawn();
        TrashManager.Instance.StartSpawn();
        IngredientManager.Instance.CreateIngredientObjOnPosition();
        MoveManager.Instance.OnGameEnter();
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
}

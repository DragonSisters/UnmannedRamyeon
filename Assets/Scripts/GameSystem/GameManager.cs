using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
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

    #region 방향성 변경에 따른 제어 변수
    // 참고: https://github.com/DragonSisters/UnmannedRamyeonObsidian/blob/main/00_공지/방향성에%20대한%20논의%20회의록.md
    // * 일반 손님이 재료를 잘못가져오는 기능을 넣을지 여부
    public readonly bool CommonConsumerWrongPickFeature = false;
    // * 쓰레기를 치우는 기능을 넣을지 여부
    public readonly bool UseTrashFeature = false;
    // * RecipeConsumer 가 타이머를 사용하는 기능을 넣을지 여부
    public readonly bool UseRecipeConsumerTimer = true;
    // * CommonConsumer 가 나갈 때 코인에 영향을 미치는지에 대한 여부
    public readonly bool UseCommonConsumerCoin = false;
    #endregion

    private void Start()
    {
        UIManager.Instance.SetCursor();
        UIManager.Instance.SetModeButtons();
        UIManager.Instance.HideCutsceneCanvas();
        ConsumerManager.Instance.InitializeConsumerManagerSetting();
        TrashManager.Instance.Initialize();
    }

    public void SetMode(bool isHardMode)
    {
        if(isHardMode)
        {
            this.isHardMode = true;
        }
        else
        {
            this.isHardMode = false;
        }
    }

    // inGameUI 를 활성화합니다
    public void StartGame()
    {
        isGameStarted = true;
        gameStartTime = Time.time;
        UIManager.Instance.HideCutsceneCanvas();
        UIManager.Instance.ControlInGameCanvas(true);
        StartCoroutine(UpdateGame());

        earlyStageTime = isHardMode ? hardEarlyStageTime : easyEarlyStageTime;

        FinanceManager.Instance.OnGameEnter();
        ConsumerManager.Instance.StartSpawn(isHardMode);
        TrashManager.Instance.StartSpawn();
        IngredientManager.Instance.CreateIngredientObjOnPosition();
        MoveManager.Instance.OnGameEnter();
        UIManager.Instance.ExecuteTimer();
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
        UIManager.Instance.ControlInGameCanvas(false);
        UIManager.Instance.ResetCursor();

        FinanceManager.Instance.OnGameEnd();
        // 씬에 나온 손님들 모두를 없애고, 스폰루틴을 중지합니다.
        ConsumerManager.Instance.StopSpawn();
        TrashManager.Instance.StopSpawn();
        // 생성했던 재료들을 모두 비활성화합니다.
        IngredientManager.Instance.OnGameEnd();

        // 콤보 초기화
        ComboManager.Instance.ResetCombo();

        // 게임 결과에 따라 성공/실패 화면을 불러옵니다.
        UIManager.Instance.ControlEndCanvas(true);
    }
}

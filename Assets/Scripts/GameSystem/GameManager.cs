using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : Singleton<GameManager>
{
    [Header("시작 화면 관련 게임 오브젝트")]
    [SerializeField] private GameObject startCanvas;
    [SerializeField] private GameObject btn_Start;
    [SerializeField] private GameObject img_Start;

    [Header("인게임 화면 관련 게임 오브젝트")]
    [SerializeField] private GameObject inGameCanvas;

    [Header("EndCanvas 관련 게임 오브젝트")]
    [SerializeField] private GameObject endCanvas;
    [SerializeField] private GameObject img_Success;
    [SerializeField] private GameObject img_Fail;

    public bool IsGameStarted => isGameStarted;
    private bool isGameStarted;

    // Start 버튼에 연결할 함수입니다
    public void OnStartButtonClick()
    {
        StartCoroutine(nameof(UnableStartUI));
        // @charotiti9 TODO: 손님 풀을 미리 만들어두어야합니다. 지금은 자리가 마땅치 않아서 게임 시작 버튼을 누르면 생성하도록 만들었습니다.
        // 추후 더 괜찮은 자리가 나오면 자리를 옮겨줍시다.
        ConsumerManager.Instance.InitializePools();
    }

    // 시작 화면에서 버튼이 사라지고, 게임이 시작된다는 UI (img_Start) 가 나옵니다.
    // @anditsoon TODO: 지금은 임시로 1초 간 나타났다 사라지게 만들었지만, 추후 날아오는 효과라던가 깜박이는 효과 등을 추가할 예정입니다.
    // 1초 뒤 게임 시작 UI 가 사라지고 인게임 UI 가 나타납니다.
    private IEnumerator UnableStartUI()
    {
        btn_Start.SetActive(false);
        img_Start.SetActive(true);
        yield return new WaitForSeconds(1f);
        img_Start.SetActive(false);
        StartGame();
    }

    // inGameUI 를 활성화합니다
    // 활성화되며 캔버스에 붙어있는 TimerUI 가 자동으로 실행됩니다
    private void StartGame()
    {
        isGameStarted = true;
        inGameCanvas.SetActive(true);
        StartCoroutine(UpdateGame());

        ConsumerManager.Instance.StartSpawn();
    }

    private IEnumerator UpdateGame()
    {
        while (isGameStarted)
        {
            SpriteClickHandler.Instance.UpdateHandler();
            yield return null;
        }
    }

    public void EndGame()
    {
        isGameStarted = false;
        // 씬에 나온 손님들 모두를 없애고, 스폰루틴을 중지합니다.
        ConsumerManager.Instance.StopSpawn();

        endCanvas.SetActive(true);
        // @anditsoon TODO: 현재는 성공 화면이 자동으로 나오게 해 놓았으나, 나중에는 게임 결과에 따라 성공/실패 화면이 구분되어 나오게 구현해야 합니다.
        img_Success.SetActive(true);
    }
}

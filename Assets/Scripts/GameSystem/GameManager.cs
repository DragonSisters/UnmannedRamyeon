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
    [SerializeField] private GameObject consumerManager;

    [Header("EndCanvas 관련 게임 오브젝트")]
    [SerializeField] private GameObject endCanvas;
    [SerializeField] private GameObject img_Success;
    [SerializeField] private GameObject img_Fail;

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
        inGameCanvas.SetActive(true);
        consumerManager.SetActive(true);
    }

    public void EndGame()
    {
        // @anditsoon TODO: 현재는 consumerManager 만 끄고 있으나, 나중에는 씬에 나온 손님들 모두를 없애야 합니다.
        consumerManager.SetActive(false);
        endCanvas.SetActive(true);
        // @anditsoon TODO: 현재는 성공 화면이 자동으로 나오게 해 놓았으나, 나중에는 게임 결과에 따라 성공/실패 화면이 구분되어 나오게 구현해야 합니다.
        img_Success.SetActive(true);
    }
}

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : Singleton<GameManager>
{
    [Header("���� ȭ�� ���� ���� ������Ʈ")]
    [SerializeField] private GameObject startCanvas;
    [SerializeField] private GameObject btn_Start;
    [SerializeField] private GameObject img_Start;

    [Header("�ΰ��� ȭ�� ���� ���� ������Ʈ")]
    [SerializeField] private GameObject inGameCanvas;
    [SerializeField] private GameObject consumerManager;

    [Header("EndCanvas ���� ���� ������Ʈ")]
    [SerializeField] private GameObject endCanvas;
    [SerializeField] private GameObject img_Success;
    [SerializeField] private GameObject img_Fail;

    // Start ��ư�� ������ �Լ��Դϴ�
    public void OnStartButtonClick()
    {
        StartCoroutine(nameof(UnableStartUI));
    }

    // ���� ȭ�鿡�� ��ư�� �������, ������ ���۵ȴٴ� UI (img_Start) �� ���ɴϴ�.
    // @anditsoon TODO: ������ �ӽ÷� 1�� �� ��Ÿ���� ������� ���������, ���� ���ƿ��� ȿ������� �����̴� ȿ�� ���� �߰��� �����Դϴ�.
    // 1�� �� ���� ���� UI �� ������� �ΰ��� UI �� ��Ÿ���ϴ�.
    private IEnumerator UnableStartUI()
    {
        btn_Start.SetActive(false);
        img_Start.SetActive(true);
        yield return new WaitForSeconds(1f);
        img_Start.SetActive(false);
        StartGame();
    }

    // inGameUI �� Ȱ��ȭ�մϴ�
    // Ȱ��ȭ�Ǹ� ĵ������ �پ��ִ� TimerUI �� �ڵ����� ����˴ϴ�
    private void StartGame()
    {
        inGameCanvas.SetActive(true);
        consumerManager.SetActive(true);
    }

    public void EndGame()
    {
        // @anditsoon TODO: ����� consumerManager �� ���� ������, ���߿��� ���� ���� �մԵ� ��θ� ���־� �մϴ�.
        consumerManager.SetActive(false);
        endCanvas.SetActive(true);
        // @anditsoon TODO: ����� ���� ȭ���� �ڵ����� ������ �� ��������, ���߿��� ���� ����� ���� ����/���� ȭ���� ���еǾ� ������ �����ؾ� �մϴ�.
        img_Success.SetActive(true);
    }
}

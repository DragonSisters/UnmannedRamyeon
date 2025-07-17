using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] private Image img_timerGuage;

    // @anditsoon TODO: �׽�Ʈ ������ 5�ʷ� �� ��������, ���߿� ���� ���� �÷��� ���� 300 (300�� = 5��) �� �Ǿ�� �մϴ�.
    [SerializeField] private float time = 5;
    private float curTime;

    // GameManager �� ���� inGameCanvas �� Ȱ��ȭ�Ǹ� �ڵ����� ����˴ϴ�.
    void Start()
    {
        StartCoroutine(nameof(StartTimer));
    }

    private IEnumerator StartTimer()
    {
        curTime = time;
        while(curTime > 0)
        {
            curTime -= Time.deltaTime;
            img_timerGuage.fillAmount = curTime / time;
            yield return null;

            if(curTime <= 0)
            {
                curTime = 0;
                GameManager.Instance.EndGame();
                yield break;
            }
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] private Image img_timerGuage;

    // @anditsoon TODO: 테스트 용으로 5초로 해 놓았으나, 나중에 실제 게임 플레이 때는 300 (300초 = 5분) 이 되어야 합니다.
    [SerializeField] private float time = 5;
    private float curTime;

    // GameManager 에 의해 inGameCanvas 가 활성화되면 자동으로 실행됩니다.
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

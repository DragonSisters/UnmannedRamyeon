using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] private Image img_timerGuage;

    private float gameDuration;
    private float curLeftTime;

    // GameManager 에 의해 inGameCanvas 가 활성화되면 자동으로 실행됩니다.
    void Start()
    {
        StartCoroutine(nameof(StartTimer));
        gameDuration = GameManager.Instance.GameDuration;
    }

    private IEnumerator StartTimer()
    {
        curLeftTime = gameDuration;
        while(curLeftTime > 0)
        {
            curLeftTime -= Time.deltaTime;
            img_timerGuage.fillAmount = curLeftTime / gameDuration;
            yield return null;

            if(curLeftTime <= 0)
            {
                curLeftTime = 0;
                GameManager.Instance.EndGame();
                yield break;
            }
        }
    }
}

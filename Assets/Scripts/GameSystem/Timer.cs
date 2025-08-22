using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] private TMP_Text txt_timer;

    private float gameDuration;
    private float curLeftTime;

    public void ExecuteTimer()
    {
        gameDuration = GameManager.Instance.GameDuration;
        StartCoroutine(nameof(StartTimer));
    }

    private IEnumerator StartTimer()
    {
        curLeftTime = gameDuration;
        while(curLeftTime > 0)
        {
            curLeftTime -= Time.deltaTime;

            int minutes = Mathf.FloorToInt(curLeftTime / 60);
            int seconds = Mathf.FloorToInt(curLeftTime % 60);

            txt_timer.text = minutes.ToString("00") + ":" + seconds.ToString("00");

            yield return null;

            if(curLeftTime <= 0)
            {
                curLeftTime = 0;
                txt_timer.text = "00 : 00";
                GameManager.Instance.EndGame();
                yield break;
            }
        }
    }
}

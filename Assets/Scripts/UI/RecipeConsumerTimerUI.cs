using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RecipeConsumerTimerUI : MonoBehaviour
{
    [SerializeField] private Image consumerTimerBackground;
    [SerializeField] private Image consumerTimerFill;
    private float elapsedTime = 0;
    private float fillAmount = 0;

    public IEnumerator FillTimerRoutine(float stayTime)
    {
        while (elapsedTime < stayTime)
        {
            elapsedTime += Time.deltaTime;
            fillAmount = Mathf.Clamp01(1 - elapsedTime / stayTime);
            consumerTimerFill.fillAmount = fillAmount;
            yield return null;
        }

        if(elapsedTime > stayTime)
        {
            elapsedTime = 0;
            DeactivateTimer();
        }
    }

    public void FillTimer(float fillAmount)
    {
        fillAmount = Mathf.Clamp01(fillAmount);
        consumerTimerFill.fillAmount = fillAmount;
    }

    public void ActivateTimer()
    {
        consumerTimerBackground.gameObject.SetActive(true);
        consumerTimerFill.gameObject.SetActive(true);
    }

    public void DeactivateTimer()
    {
        consumerTimerFill.fillAmount = 1;
        consumerTimerBackground.gameObject.SetActive(false);
        consumerTimerFill.gameObject.SetActive(false);
    }
}

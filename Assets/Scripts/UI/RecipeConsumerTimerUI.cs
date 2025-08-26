using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RecipeConsumerTimerUI : MonoBehaviour
{
    [SerializeField] private Image consumerTimerBackground;
    [SerializeField] private Image consumerTimerFill;
    [SerializeField] private Color startFillColor;
    [SerializeField] private Color closeToEndFillColor;
    [SerializeField] private Animation shakeAnimation;
    private float elapsedTime = 0;
    private float fillAmount = 0;
    private float closeToEnd = 0.35f;
    private bool isCloseToEnd = false;

    public IEnumerator FillTimerRoutine(float stayTime)
    {
        while (elapsedTime < stayTime)
        {
            elapsedTime += Time.deltaTime;
            fillAmount = Mathf.Clamp01(1 - elapsedTime / stayTime);

            if(fillAmount < closeToEnd && !isCloseToEnd)
            {
                isCloseToEnd = true;
                consumerTimerFill.color = closeToEndFillColor;
                
                if(!shakeAnimation.isPlaying)
                {
                    shakeAnimation.Play();
                }
            }

            consumerTimerFill.fillAmount = fillAmount;
            yield return null;
        }

        if(elapsedTime > stayTime)
        {
            DeactivateTimer();
        }
    }

    public void ActivateTimer()
    {
        elapsedTime = 0;
        consumerTimerFill.fillAmount = 1;
        isCloseToEnd = false;
        consumerTimerFill.color = startFillColor;
        consumerTimerBackground.gameObject.SetActive(true);
        consumerTimerFill.gameObject.SetActive(true);
    }

    public void DeactivateTimer()
    {
        consumerTimerBackground.gameObject.SetActive(false);
        consumerTimerFill.gameObject.SetActive(false);
    }
}

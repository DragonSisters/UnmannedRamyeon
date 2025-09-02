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
    private float stayTime = 10f;
    public float StayTime => stayTime;
    private float elapsedTime = 0;
    private float fillAmount = 0;
    private float closeToEnd = 0.35f;
    private bool isCloseToEnd = false;
    private Coroutine timerCoroutine; // 코루틴 참조 저장

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
                SoundManager.Instance.PlayLoopSound(LoopSoundType.TimeDue);
                
                if(!shakeAnimation.isPlaying)
                {
                    shakeAnimation.Play();
                }
            }

            consumerTimerFill.fillAmount = fillAmount;
            yield return null;
        }
    }

    public void ActivateTimer()
    {
        // 이전 코루틴이 있다면 정지
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        elapsedTime = 0;
        consumerTimerFill.fillAmount = 1;
        isCloseToEnd = false;
        consumerTimerFill.color = startFillColor;
        consumerTimerBackground.gameObject.SetActive(true);
        consumerTimerFill.gameObject.SetActive(true);
        timerCoroutine = StartCoroutine(FillTimerRoutine(stayTime));
    }

    public void DeactivateTimer()
    {
        // 코루틴 명시적 정지
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        shakeAnimation.Stop();
        SoundManager.Instance.StopLoopSound();
        consumerTimerBackground.gameObject.SetActive(false);
        consumerTimerFill.gameObject.SetActive(false);
    }
}

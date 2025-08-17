using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public enum MoodState
{
    Invalid = -1,

    Angry = 0,
    Bad = 30,
    Good = 50,
    Happy = 70,
}

public class ConsumerMood : MonoBehaviour
{
    private const int MAX_MOOD = 100;
    private const int MIN_MOOD = 0;
    private const int DECREASE_AMOUNT = 5; // 서서히 감소할 양
    private const float DECREASE_INTERVAL = 0.5f; // n초에 1번씩 줄어듦

    private const float ANGRY_RATIO = 0.5f;
    private const float BAD_RATIO = 0.8f;
    private const float GOOD_RATIO = 0.9f;
    private const float HAPPY_RATIO = 1f;

    private Coroutine decreaseCoroutine;

    public MoodState Mood
    {
        get
        {
            return Enum.GetValues(typeof(MoodState))
            .Cast<MoodState>()
            .Where(m => (int)m >= 0 && currentAmount >= (int)m)
            .OrderByDescending(m => (int)m)
            .FirstOrDefault();
        }
    }
    public int CurrentAmount => currentAmount;
    public int currentAmount;

    public void Initialize()
    {
        currentAmount = MAX_MOOD;
    }

    /// <summary>
    /// 정해진 양만큼 만족도를 저하시킵니다
    /// </summary>
    /// <param name="decreaseAmount"></param>
    public void DecreaseMood(int decreaseAmount)
    {
        currentAmount -= decreaseAmount;
        Debug.Log($"손님 기분 저하: {Mood}, {currentAmount}");
    }

    /// <summary>
    /// 정해진 양만큼 만족도를 증가시킵니다
    /// </summary>
    /// <param name="increaseAmount"></param>
    public void IncreaseMood(int increaseAmount)
    {
        currentAmount += increaseAmount;
        Debug.Log($"손님 기분 증가: {Mood}, {currentAmount}");
    }

    public void StartDecrease()
    {
        decreaseCoroutine = StartCoroutine(DecreaseMoodGradually());
    }
    public void StopDecrease()
    {
        if (decreaseCoroutine != null)
        {
            StopCoroutine(decreaseCoroutine);
        }
    }

    /// <summary>
    /// 손님의 기분을 서서히 낮춥니다.
    /// </summary>
    private IEnumerator DecreaseMoodGradually()
    {
        while (currentAmount > MIN_MOOD)
        {
            DecreaseMood(DECREASE_AMOUNT);

            if (currentAmount < MIN_MOOD)
            {
                currentAmount = MIN_MOOD;
            }

            yield return new WaitForSeconds(DECREASE_INTERVAL);
        }
    }

    public float GetMoodRatio()
    {
        switch(Mood)
        {
            case MoodState.Angry:
                return ANGRY_RATIO;
            case MoodState.Bad:
                return BAD_RATIO;
            case MoodState.Good:
                return GOOD_RATIO;
            case MoodState.Happy:
                return HAPPY_RATIO;
        }

        return 0;
    }
}

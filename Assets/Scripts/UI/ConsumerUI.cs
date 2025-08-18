using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class ConsumerUI : MonoBehaviour
{
    public GameObject TargetIngredientUI;
    public GameObject SpeechBubbleUI;
    public GameObject MoodFeedbackUI;

    [SerializeField] private IngredientUI[] ingredientUis;
    [SerializeField] private TMP_Text speechBubbleText;
    [SerializeField] private Image speechBubbleImage;
    [SerializeField] private Animation moodAnimation;
    [SerializeField] private List<Sprite> moodTextures = new();

    // IngredientUI의 이벤트를 ConsumerIngredientHandler에게 중계해주는 이벤트입니다.
    public delegate void ClickEventTransferHandler(int index);
    public event ClickEventTransferHandler TransferClickEvent;

    public void ActivateIngredientUI(bool isActive)
    {
        TargetIngredientUI.SetActive(isActive);
    }

    public void SetSpeechBubbleUI(bool isActive)
    {
        // speechBubbleText가 설정되지 않았다면 무조건 비활성화합니다.
        if(string.IsNullOrEmpty(speechBubbleText.text))
        {
            Debug.LogWarning($"speechBubbleText가 설정되지 않았기 때문에 말풍선을 비활성화 처리했습니다.");
            SpeechBubbleUI.SetActive(false);
        }

        SpeechBubbleUI.SetActive(isActive);
    }

    public void SetSpeechBubbleText(string message)
    {
        speechBubbleText.text = message;
    }

    public void SetSpeechBubbleColor(Color color)
    {
        speechBubbleImage.color = color;
    }

    public void InitializeIngredintUI(List<IngredientScriptableObject> ingredients)
    {
        for (int i = 0; i < ingredients.Count; i++)
        {
            ingredientUis[i].Initialize(i);
            ingredientUis[i].SetIngredientImage(ingredients[i].Icon);
            ingredientUis[i].OnClicked += TransferIngredientClick;
        }
    }

    public void ActivateFeedbackUIs(int index, bool isCorrect)
    {
        ingredientUis[index].ActivateFeedbackUI(isCorrect);
    }

    public void DeactivateAllFeedbackUIs()
    {
        foreach (var ui in ingredientUis)
        {
            ui.DeactivateFeedbackUI();
        }
    }

    public void SetMoodFeedbackUI(MoodState moodState)
    {
        // MoodState enum 값을 moodTextures의 인덱스로 변환
        int moodIndex = moodState switch
        {
            MoodState.Angry => 0,
            MoodState.Bad => 1,
            MoodState.Good => 2,
            MoodState.Happy => 3,
            _ => 0 // 예외 또는 기본값 처리
        };

        moodIndex = Mathf.Clamp(moodIndex, 0, moodTextures.Count - 1);

        var rawImage = MoodFeedbackUI.GetComponent<Image>();

        if (rawImage == null)
        {
            Debug.LogError("MoodFeedbackUI에 Image 컴포넌트가 없습니다.");
            return;
        }

        if (moodTextures.Count > 0)
        {
            rawImage.sprite = moodTextures[moodIndex];
            ActivateMoodFeedbackUI(true);
        }
    }

    public void ActivateMoodFeedbackUI(bool isActive)
    {
        if(isActive)
        {
            moodAnimation.Play();
        }
        MoodFeedbackUI.SetActive(isActive);
    }

    private void TransferIngredientClick(int index)
    {
        TransferClickEvent?.Invoke(index);
    }
}


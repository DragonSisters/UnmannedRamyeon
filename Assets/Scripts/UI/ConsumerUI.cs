using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class ConsumerUI : MonoBehaviour
{
    public GameObject TargetIngredientUI;
    public GameObject SpeechBubbleUI;

    [SerializeField] private IngredientUI[] ingredientUis;
    [SerializeField] private TMP_Text speechBubbleText;
    [SerializeField] private Image speechBubbleImage;

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

    private void TransferIngredientClick(int index)
    {
        TransferClickEvent?.Invoke(index);
    }
}


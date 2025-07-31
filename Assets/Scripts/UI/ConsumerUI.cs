using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ConsumerUI : MonoBehaviour
{
    public GameObject TargetIngredientUI;
    public GameObject SpeechBubbleUI;

    [SerializeField] private IngredientUI[] ingredientUis;
    [SerializeField] private TMP_Text speechBubbleText;
    [SerializeField] private Sprite speechBubbleSprite;

    // IngredientUI의 이벤트를 ConsumerIngredientHandler에게 중계해주는 이벤트입니다.
    public delegate void ClickEventTransferHandler(int index);
    public event ClickEventTransferHandler TransferClickEvent;

    public void ActivateIngredientUI(bool isActive)
    {
        TargetIngredientUI.SetActive(isActive);
    }

    public void ActivateSpeechBubbleUI(bool isActive)
    {
        SpeechBubbleUI.SetActive(isActive);
    }

    public void SetSpeechBubbleText(string message)
    {
        speechBubbleText.text = message;
    }

    public void InitializeIngredintUI(List<IngredientScriptableObject> ingredients)
    {
        for (int i = 0; i < ingredients.Count; i++)
        {
            ingredientUis[i].Initialize(i);
            ingredientUis[i].SetIngredientImage(ingredients[i].Icon);
            ingredientUis[i].OnClicked += ForwardIngredientClick;
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

    private void ForwardIngredientClick(int index)
    {
        TransferClickEvent?.Invoke(index);
    }
}

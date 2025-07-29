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

    public void ActivateIngredientUI(bool isActive)
    {
        TargetIngredientUI.SetActive(isActive);
    }

    public void ActivateSpeechBubbleUI(bool isActive)
    {
        SpeechBubbleUI.SetActive(isActive);
    }

    public void InitializeIngredintUI(List<IngredientScriptableObject> ingredients)
    {
        for (int i = 0; i < ingredients.Count; i++)
        {
            ingredientUis[i].SetIngredientImage(ingredients[i].Icon);
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
}

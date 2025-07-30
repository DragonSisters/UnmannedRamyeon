using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ConsumerUI : MonoBehaviour
{
    public GameObject TargetIngredientUI;
    public GameObject SpeechBubbleUI;

    [SerializeField] private Image[] ingredientImages;
    [SerializeField] private Image[] correctOrWrongImages;
    [SerializeField] private TMP_Text speechBubbleText;
    [SerializeField] private Sprite correctSprite;
    [SerializeField] private Sprite wrongSprite;
    [SerializeField] private Sprite speechBubbleSprite;

    public void ActivateIngredientUI(bool isActive)
    {
        TargetIngredientUI.SetActive(isActive);
    }

    public void UpdateIngredientImages(List<IngredientScriptableObject> ingredients)
    {
        for(int ingredientNum = 0; ingredientNum < ingredients.Count; ingredientNum++)
        {
            ingredientImages[ingredientNum].sprite = ingredients[ingredientNum].Icon;
        }
    }

    public void DisplayIngredientFeedback(bool isCorrect, int index)
    {
        Image selectedImage = correctOrWrongImages[index];
        selectedImage.gameObject.SetActive(true);

        if(isCorrect)
        {
            selectedImage.sprite = correctSprite;
        }
        else
        {
            selectedImage.sprite = wrongSprite;
        }
    }

    public void DeactivateAllFeedbackUIs()
    {
        foreach(Image image in correctOrWrongImages)
        {
            image.gameObject.SetActive(false);
        }
    }

    public void SpeakNextIngredient(IngredientScriptableObject ingredient)
    {
        speechBubbleText.text = StringUtil.KoreanParticle($"음~ {ingredient.Name}을/를 가져와볼까~"); 
    }

    public void OrderByRecipeOnUI(string name)
    {
        SpeechBubbleUI.SetActive(true);
        SpeechBubbleUI.GetComponentInChildren<TMP_Text>().text = StringUtil.KoreanParticle($"{name} 주세요!");
    }
}

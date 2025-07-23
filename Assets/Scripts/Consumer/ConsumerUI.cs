using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ConsumerUI : MonoBehaviour
{
    [SerializeField] private Image[] ingredientImages;
    [SerializeField] private Image[] correctOrWrongImages;
    [SerializeField] private Sprite correctSprite;
    [SerializeField] private Sprite wrongSprite;

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
}

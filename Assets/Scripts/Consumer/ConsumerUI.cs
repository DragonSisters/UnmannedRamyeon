using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ConsumerUI : MonoBehaviour
{
    [SerializeField] private Consumer consumer;
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

    public void DisplayIngredientFeedback()
    {

    }
}

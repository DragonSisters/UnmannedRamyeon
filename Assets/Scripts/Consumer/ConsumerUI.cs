using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ConsumerUI : MonoBehaviour
{
    [SerializeField] private Image[] ingredientImages;
    [SerializeField] private Image[] CorrectOrWrongImages;
    [SerializeField] private Sprite[] CorrectOrWrongSprites;

    public void UpdateIngredientImages(List<IngredientScriptableObject> ingredients)
    {
        for(int ingredientNum = 0; ingredientNum < ingredients.Count; ingredientNum++)
        {
            ingredientImages[ingredientNum].sprite = ingredients[ingredientNum].IconSprite;
        }
    }
}

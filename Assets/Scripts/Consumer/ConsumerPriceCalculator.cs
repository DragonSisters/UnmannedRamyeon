using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ConsumerPriceCalculator : MonoBehaviour
{
    ConsumerIngredientHandler ingredientHandler;
    ConsumerMood consumerMood;

    public void Initialize()
    {
        ingredientHandler = gameObject.GetOrAddComponent<ConsumerIngredientHandler>();
        consumerMood = gameObject.GetOrAddComponent<ConsumerMood>();
    }

    public int GetFinalPrice()
    {
        float price = 0;
        
        List<IngredientScriptableObject> approvedIngredients = ingredientHandler.approvedIngredients;

        foreach(IngredientScriptableObject ingredient in approvedIngredients)
        {
            price += ingredient.Price;
        }

        price *= consumerMood.GetMoodRatio();

        Debug.Log($"최종 금액은 {price}입니다");

        return (int)price;
    }
}

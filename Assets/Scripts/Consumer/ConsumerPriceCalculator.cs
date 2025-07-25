using UnityEngine;
using System.Collections.Generic;

public class ConsumerPriceCalculator : MonoBehaviour
{
    [SerializeField] ConsumerIngredientHandler ingredientHandler;

    public int GetFinalPrice()
    {
        float price = 0;
        ingredientHandler = gameObject.GetComponent<ConsumerIngredientHandler>();
        List<IngredientScriptableObject> approvedIngredients = ingredientHandler.approvedIngredients;

        foreach(IngredientScriptableObject ingredient in approvedIngredients)
        {
            price += ingredient.Price;
        }

        price *= gameObject.GetComponent<ConsumerMood>().GetMoodRatio();

        Debug.Log($"최종 금액은 {price}입니다");

        return (int)price;
    }
}

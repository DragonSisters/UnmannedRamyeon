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

        List<(IngredientScriptableObject ingredient, int index, bool isCorrect)> ownedIngredients 
            = ingredientHandler.OwnedIngredients;

        foreach(var ingredientInfo in ownedIngredients)
        {
            // 지적한 ingredient만 값에 들어갑니다
            if (ingredientInfo.isCorrect)
            {
                price += ingredientInfo.ingredient.Price;
            }
        }

        price *= consumerMood.GetMoodRatio();

        Debug.Log($"최종 금액은 {price}입니다");

        return (int)price;
    }
}

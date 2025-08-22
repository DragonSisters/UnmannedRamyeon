using UnityEngine;
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

    public void GetFinalPrice(out int gain, out int loss)
    {
        gain = 0;
        loss = 0;

        foreach (var ingredientInfo in ingredientHandler.OwnedIngredients)
        {
            // 지적한 ingredient만 값에 들어갑니다
            if (ingredientInfo.IsCorrect)
            {
                gain += ingredientInfo.Ingredient.Price;
            }
            else
            {
                loss += ingredientInfo.Ingredient.Price;
            }
        }

        gain = Mathf.RoundToInt((float)gain * consumerMood.GetMoodRatio());

        Debug.Log($"최종 금액은 {gain}입니다. 잃은 금액은 {loss}입니다.");
    }
}

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 클릭을 해서 주의를 주어야할 필요성이 있는 손님의 동작을 이곳에서 정리합니다.
/// </summary>
public class RecipeConsumer : Consumer, IClickableSprite
{
    [SerializeField] private List<RecipeScriptableObject> allRecipes;
    private RecipeScriptableObject myRecipe;
    [SerializeField] private float recipeOrderDuration = 2f;

    internal override void HandleChildEnter()
    {
    }

    internal override void HandleChildExit()
    {
    }

    internal override IEnumerator HandleChildUpdate()
    {
        yield break;
    }

    public override void SetIngredientLists()
    {
        myRecipe = GetRandomRecipe();
        List<IngredientScriptableObject> ingredients = myRecipe.Ingredients;
        ingredientHandler.SetAllIngredientLists(ingredients);
    }

    // 레시피 중 하나를 선택
    public RecipeScriptableObject GetRandomRecipe()
    {
        if (allRecipes == null || allRecipes.Count == 0)
        {
            throw new ArgumentException("리스트가 비어 있습니다.");
        }
        int index = UnityEngine.Random.Range(0, allRecipes.Count);

        return allRecipes[index];
    }

    public override IEnumerator HandleChildOrderOnUI()
    {
        consumerUI.OrderByRecipeOnUI(myRecipe.name);
        SetState(ConsumerState.Issue);
        yield return new WaitForSeconds(recipeOrderDuration);
        consumerUI.SetSpeechBubbleUI(false);
    }

    internal override void HandleChildClick()
    {
        //ingredients 들 클릭 활성화
        IngredientManager.Instance.IsIngredientSelectMode = true;
    }

    internal override void HandleChildUnclicked()
    {
        IngredientManager.Instance.IsIngredientSelectMode = false;
    }
}

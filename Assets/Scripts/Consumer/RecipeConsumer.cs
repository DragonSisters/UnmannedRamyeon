using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 클릭을 해서 주의를 주어야할 필요성이 있는 손님의 동작을 이곳에서 정리합니다.
/// </summary>
public class RecipeConsumer : Consumer
{
    [SerializeField] List<RecipeScriptableObject> allRecipes;

    internal override void HandleChildClick()
    {
    }

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
        RecipeScriptableObject recipe = GetRandomRecipe();
        List<IngredientScriptableObject> ingredients = recipe.Ingredients;
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
}

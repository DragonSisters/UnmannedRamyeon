using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 클릭을 해서 주의를 주어야할 필요성이 있는 손님의 동작을 이곳에서 정리합니다.
/// </summary>
public class RecipeConsumer : Consumer
{
    // 레시피 선택 관련 변수
    [SerializeField] private List<RecipeScriptableObject> allRecipes;
    public RecipeScriptableObject MyRecipe => myRecipe;
    private RecipeScriptableObject myRecipe;

    List<IngredientScriptableObject> recipeIngredients = new List<IngredientScriptableObject>();
    [SerializeField] private float recipeOrderDuration = 2f;
    public bool IsAllIngredientSelected = false;

    internal override void HandleChildEnter()
    {
        StartCoroutine(EnterCoroutine());
        SetState(ConsumerState.Issue);
    }

    internal override void HandleChildExit()
    {
        IngredientManager.Instance.DeleteRecipeConsumer();
    }

    internal override IEnumerator HandleChildIssue()
    {
        HandleOrderOnUI();
        yield return new WaitUntil(() => IsAllIngredientSelected);

        bool isAllIngredientCorrect = true;
        foreach (IngredientInfo ingredient in ingredientHandler.AttemptIngredients)
        {
            if (ingredient.Index < 0)
            {
                isAllIngredientCorrect = false;
            }
        }

        if (isAllIngredientCorrect)
        {
            SoundManager.Instance.PlayEffectSound(EffectSoundType.Success);
            SetState(ConsumerState.Order);
        }
        else
        {
            SoundManager.Instance.PlayEffectSound(EffectSoundType.Fail);
            SetState(ConsumerState.Leave);
        }
    }

    internal override IEnumerator HandleChildUpdate()
    {
        yield break;
    }

    private IEnumerator EnterCoroutine()
    {
        var waitingPoint = MoveManager.Instance.RandomShoutPoint;
        while (!moveScript.IsCloseEnough(waitingPoint))
        {
            moveScript.MoveTo(waitingPoint);
            yield return null;
        }

        yield return new WaitForSeconds(recipeOrderDuration);
    }

    public override void SetIngredientLists()
    {
        myRecipe = GetRandomRecipe();
        recipeIngredients = myRecipe.Ingredients;
        ingredientHandler.SetAllIngredientLists(recipeIngredients);
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

    public override IEnumerator HandleOrderOnUI()
    {
        StartCoroutine(speechScript.StartSpeechFromSituation(currentConsumerScriptableObject, ConsumerSituation.RecipeOrder, false, true, true, true, -1, $"{myRecipe.Name}"));
        yield return new WaitForSeconds(recipeOrderDuration);
    }

    internal override void HandleChildClick()
    {
        //ingredients 들 클릭 활성화
        IngredientManager.Instance.IsIngredientSelectMode = true;

        // IngredientManager 에 내 정보 보냄
        IngredientManager.Instance.ReceiveRecipeConsumer(this);
    }

    internal override void HandleChildUnclicked()
    {
        //ingredients 들 클릭 비활성화
        IngredientManager.Instance.IsIngredientSelectMode = false;

        // IngredientManager 에 내 정보 삭제 요청
        IngredientManager.Instance.DeleteRecipeConsumer();
    }

}

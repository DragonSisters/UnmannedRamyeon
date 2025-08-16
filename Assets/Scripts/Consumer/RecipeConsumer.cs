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
    private float recipeOrderDuration = 2f;

    private float stayTime = 15f;

    public bool IsAllIngredientSelected = false;
    public int CurrPickCount { get; private set; } = 0;

    internal override void HandleChildEnter()
    {
        StartCoroutine(EnterCoroutine());

        appearanceScript.SetClickable(false);

        SetState(ConsumerState.Issue);
    }

    internal override void HandleChildExit()
    {
        ResetPickCount();
        ingredientHandler.ResetAllIngredientLists();
        IngredientManager.Instance.RemoveRecipeConsumer(this);
    }

    internal override IEnumerator HandleChildIssue()
    {
        float elapsedTime = 0f;

        while(!IsAllIngredientSelected && elapsedTime < stayTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if(!IsAllIngredientSelected)
        {
            SoundManager.Instance.PlayEffectSound(EffectSoundType.Fail);
            ResetPickCount();
            ingredientHandler.ResetAllIngredientLists();
            if(IngredientManager.Instance.IsCurrentRecipeConsumer(this)) IngredientManager.Instance.IsIngredientSelectMode = false;
            appearanceScript.SetClickable(false);
            SetState(ConsumerState.Leave);
            yield break;
        }

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
            ResetPickCount();
            ingredientHandler.ResetAllIngredientLists();
            if (IngredientManager.Instance.IsCurrentRecipeConsumer(this)) IngredientManager.Instance.IsIngredientSelectMode = false;
            SetState(ConsumerState.Leave);
        }

        appearanceScript.SetClickable(false);
        IsAllIngredientSelected = false;
    }

    internal override IEnumerator HandleChildUpdate()
    {
        yield break;
    }

    private IEnumerator EnterCoroutine()
    {
        var shoutPoint = MoveManager.Instance.RandomShoutPoint;
        moveScript.MoveToNearesetPoint(shoutPoint, out var nearestPoint);
        yield return new WaitUntil(() => moveScript.MoveStopIfCloseEnough(nearestPoint));
        StartCoroutine(HandleOrderOnUI());
        appearanceScript.SetClickable(true);
    }

    public override void SetIngredientLists()
    {
        myRecipe = GetRandomRecipe();
        recipeIngredients = new List<IngredientScriptableObject>(myRecipe.Ingredients);
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
        ResetPickCount();
        ingredientHandler.AttemptIngredients.Clear();

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
        IngredientManager.Instance.RemoveRecipeConsumer(this);
    }

    public void AddPickCount()
    {
        CurrPickCount++;
    }

    public void ResetPickCount()
    {
        CurrPickCount = 0;
    }
}

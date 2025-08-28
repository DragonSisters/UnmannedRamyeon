using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 레시피로 주문하는 손님의 동작을 이곳에서 정리합니다.
/// </summary>
public class RecipeConsumer : Consumer
{
    [SerializeField] private RecipeConsumerTimerUI timerUI;

    // 레시피 선택 관련 변수
    [SerializeField] private List<RecipeScriptableObject> allRecipes;
    public RecipeScriptableObject MyRecipe => myRecipe;
    private RecipeScriptableObject myRecipe;
    List<IngredientScriptableObject> recipeIngredients = new List<IngredientScriptableObject>();

    public bool IsAllIngredientSelected = false;
    public int CurrPickCount { get; private set; } = 0;

    private float recipeOrderDuration = 2f;
    private float stayTime = 15f;
    private IngredientScriptableObject[] ingredientsInPot = new IngredientScriptableObject[4];

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
    }

    internal override IEnumerator HandleChildIssue()
    {
        yield return new WaitUntil(() => !moveScript.IsMoving);

        float elapsedTime = 0f;
        timerUI.ActivateTimer();
        StartCoroutine(timerUI.FillTimerRoutine(stayTime));

        while(!IsAllIngredientSelected && elapsedTime < stayTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 시간 안에 재료를 다 고르지 못했다.
        if(!IsAllIngredientSelected)
        {
            SoundManager.Instance.PlayEffectSound(EffectSoundType.Fail);
            ResetPickCount();
            ingredientHandler.ResetAllIngredientLists();
            appearanceScript.SetClickable(false);

            IngredientManager.Instance.OnRecipeConsumerFinished(this);

            SetState(ConsumerState.Leave);
            yield break;
        }

        timerUI.DeactivateTimer();

        bool isAllIngredientCorrect = true;
        foreach (IngredientInfo ingredient in ingredientHandler.AttemptIngredients)
        {
            if (ingredient.Index < 0)
            {
                isAllIngredientCorrect = false;
            }
        }

        // 시간 안에 재료를 다 골랐고, 그 재료가 다 맞다
        if (isAllIngredientCorrect)
        {
            SoundManager.Instance.PlayEffectSound(EffectSoundType.Success);
            SetState(ConsumerState.LineUp);
        }
        else // 시간 안에 재료를 다 골랐지만, 그 재료 중에 단 하나라도 틀린 재료가 있다.
        {
            SoundManager.Instance.PlayEffectSound(EffectSoundType.Fail);
            ResetPickCount();
            ingredientHandler.ResetAllIngredientLists();
            SetState(ConsumerState.Leave);
        }

        IngredientManager.Instance.OnRecipeConsumerFinished(this);

        appearanceScript.SetClickable(false);
        IsAllIngredientSelected = false;
    }

    internal override IEnumerator HandleChildUpdate()
    {
        yield break;
    }

    internal override void HandleChildClick()
    { 
        ResetPickCount();
        ingredientHandler.AttemptIngredients.Clear();
        ingredientHandler.OwnedIngredients.Clear();

        // IngredientManager 에 내 정보 보냄
        IngredientManager.Instance.ReceiveRecipeConsumer(this);
        
        //ingredients 들 클릭 활성화
        IngredientManager.Instance.IsIngredientSelectMode = true;
    }

    internal override void HandleChildUnclick()
    {
        IngredientManager.Instance.OnRecipeConsumerFinished(this);
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

    public void AddPickCount()
    {
        CurrPickCount++;
    }

    public void ResetPickCount()
    {
        CurrPickCount = 0;
    }

    public void AddIngredientsInPot(int index, IngredientScriptableObject ingredient)
    {
        ingredientsInPot[index] = ingredient;
    }

    public void RemoveIngredientsInPot(int index)
    {
        ingredientsInPot[index] = null;
        CurrPickCount--;
    }

    public void ClearIngredientsInPot()
    {
        Array.Clear(ingredientsInPot, 0, ingredientsInPot.Length);
    }

    public bool IsIngredientsInPot(IngredientScriptableObject ingredient)
    {
        foreach(IngredientScriptableObject scriptableObject in ingredientsInPot)
        {
            if (scriptableObject == ingredient) return true;
        }
        return false;
    }
}

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
    [Header("all Recipes 기준 쉬운 레시피의 경계 인덱스")]
    [SerializeField] private int easyRecipeThresholdIndex = 4;
    [Header("쉬운 레시피 순으로 정렬")]
    [SerializeField] private List<RecipeScriptableObject> allRecipes;
    public RecipeScriptableObject MyRecipe => myRecipe;
    private RecipeScriptableObject myRecipe;
    private List<IngredientScriptableObject> recipeIngredients = new List<IngredientScriptableObject>();

    public bool IsSubmit = false;
    public bool IsAllIngredientCorrect = false;
    public int CurrPickCount { get; private set; } = 0;

    private float recipeOrderDuration = 2f;
    private float stayTime = 15f;

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

        if (GameManager.Instance.UseRecipeConsumerTimer)
        {
            float elapsedTime = 0f;
            timerUI.ActivateTimer();
            StartCoroutine(timerUI.FillTimerRoutine(stayTime));

            while (!IsSubmit && elapsedTime < stayTime)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // 시간 안에 재료를 다 고르지 못했다.
            if (!IsSubmit)
            {
                SoundManager.Instance.PlayEffectSound(EffectSoundType.Fail);
                ResetPickCount();
                ingredientHandler.ResetAllIngredientLists();
                appearanceScript.SetClickable(false);

                IngredientManager.Instance.OnRecipeConsumerFinished(this);

                SetState(ConsumerState.Leave);
                timerUI.DeactivateTimer();
                yield break;
            }
        }

        // 제출 버튼을 누를 때까지 기다린다
        yield return new WaitUntil(() => IsSubmit);

        // 제출 버튼을 누른 시점에서 그 재료가 다 맞다
        if (IsAllIngredientCorrect)
        {
            // 레시피 손님은 성공 시 바로 돈을 줍니다. (성공했다는 느낌을 더 잘 주기 위해)
            FinanceManager.Instance.IncreaseCurrentMoney(GetRecipePrice());
            SetState(ConsumerState.LineUp);
        }
        else // 제출 버튼을 누른 시점에서 재료 중에 단 하나라도 틀린 재료가 있다.
        {
            SoundManager.Instance.PlayEffectSound(EffectSoundType.Fail);
            ResetPickCount();
            ingredientHandler.ResetAllIngredientLists();
            SetState(ConsumerState.Leave);
        }

        IngredientManager.Instance.OnRecipeConsumerFinished(this);

        appearanceScript.SetClickable(false);
        IsSubmit = false;
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
        Vector2 shoutPoint = MoveManager.Instance.ShoutPoint;
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

        int index = (Time.time - GameManager.Instance.GameStartTime > GameManager.Instance.EarlyStageTime) ?
            UnityEngine.Random.Range(0, allRecipes.Count) : UnityEngine.Random.Range(0, easyRecipeThresholdIndex);

        return allRecipes[index];
    }

    private int GetRecipePrice()
    {
        int totalPrice = 0;
        foreach (var ingredient in recipeIngredients)
        {
            totalPrice += ingredient.Price;
        }
        return totalPrice;
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

    public void ReducePickCount()
    {
        CurrPickCount--;
    }

    public void ResetPickCount()
    {
        CurrPickCount = 0;
    }
}

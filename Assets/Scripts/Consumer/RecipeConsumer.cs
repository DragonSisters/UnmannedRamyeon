using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

/// <summary>
/// 클릭을 해서 주의를 주어야할 필요성이 있는 손님의 동작을 이곳에서 정리합니다.
/// </summary>
public class RecipeConsumer : Consumer, IClickableSprite
{
    // 레시피 선택 관련 변수
    [SerializeField] private List<RecipeScriptableObject> allRecipes;
    private RecipeScriptableObject myRecipe;
    List<IngredientScriptableObject> recipeIngredients = new List<IngredientScriptableObject>();
    [SerializeField] private float recipeOrderDuration = 2f;

    // 레시피 베이스로 옳은 재료를 선택하는지 검증 관련 변수
    [SerializeField] private float selectTimeLimit = 3f;
    private int correctClickCount = 0;
    private bool isChecking = false;
    private bool isAllCorrect = false;
    public bool IsAllCorrect => isAllCorrect;
    private Coroutine checkCoroutine;

    // 기타 변수
    [SerializeField] private Vector2 waitingPoint = new Vector2(7, 2); // @anditsoon TODO: 나중에 키오스크 근처로 위치 조정합니다.

    internal override void HandleChildEnter()
    {
        StartCoroutine(EnterCoroutine());
    }

    internal override void HandleChildExit()
    {
        IngredientManager.Instance.DeleteRecipeCx();
    }

    internal override IEnumerator HandleChildUpdate()
    {
        yield break;
    }

    private IEnumerator EnterCoroutine()
    {
        while (!moveScript.IsCloseEnough(waitingPoint))
        {
            moveScript.MoveTo(waitingPoint);
            yield return null;
        }
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
        // @anditsoon TODO: 지금은 들어오자마자 주문을 외치고 있습니다. 나중에 주문하는 시점이 정해지면 거기서 호출해야 합니다.
        consumerUI.OrderByRecipeOnUI(myRecipe.Name);
        SetState(ConsumerState.Issue);
        yield return new WaitForSeconds(recipeOrderDuration);
        consumerUI.SetSpeechBubbleUI(false);
    }

    internal override void HandleChildClick()
    {
        //ingredients 들 클릭 활성화
        IngredientManager.Instance.IsIngredientSelectMode = true;

        // IngredientManager 에 내 정보 보냄
        IngredientManager.Instance.ReceiveRecipeCx(this);
    }

    internal override void HandleChildUnclicked()
    {
        //ingredients 들 클릭 비활성화
        IngredientManager.Instance.IsIngredientSelectMode = false;

        // IngredientManager 에 내 정보 삭제 요청
        IngredientManager.Instance.DeleteRecipeCx();
    }
}

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
    // 아이디 생성 관련 변수
    private int consumerId;
    public int ConsumerId
    {
        get => consumerId;
        private set => consumerId = value;
    }
    private static int nextId = 0; // static 써서 같은 ID 카운터 공유

    // 레시피 선택 관련 변수
    [SerializeField] private List<RecipeScriptableObject> allRecipes;
    private RecipeScriptableObject myRecipe;
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
    Coroutine moveToWaitingPoint;

    internal override void HandleChildEnter()
    {
        consumerId = GetNextId();
        Debug.LogError($"아이디는 {consumerId}입니다.");
        moveToWaitingPoint = StartCoroutine(MoveToWaitingArea());
    }

    internal override void HandleChildExit()
    {
        Debug.LogError($"저 나가욥");
        IngredientManager.Instance.DeleteRecipeCx(consumerId);
    }

    internal override IEnumerator HandleChildUpdate()
    {
        yield break;
    }

    private static int GetNextId()
    {
        return nextId++;
    }

    private IEnumerator MoveToWaitingArea()
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

    public override IEnumerator HandleOrderOnUI()
    {
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
        IngredientManager.Instance.ReceiveRecipeCx(consumerId, this);

        StartChecking();
    }

    internal override void HandleChildUnclicked()
    {
        //ingredients 들 클릭 비활성화
        IngredientManager.Instance.IsIngredientSelectMode = false;

        // IngredientManager 에 내 정보 삭제 요청
        IngredientManager.Instance.DeleteRecipeCx(consumerId);
    }

    private void StartChecking()
    {
        if (isChecking) return;

        correctClickCount = 0;
        isChecking = true;
        checkCoroutine = StartCoroutine(SelectIngredientRoutine());
    }

    public IEnumerator SelectIngredientRoutine()
    {
        yield return new WaitForSeconds(selectTimeLimit);

        isChecking = false;
    }
}

using System.Collections;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 모든 손님에게 상속되어야합니다. 손님의 상태와 재료 등 모든 손님이 가지고 있어야하는 값과 로직을 저장하고 있습니다.
/// </summary>
public abstract class Consumer : MonoBehaviour, IPoolable, IClickableSprite
{
    [SerializeField] internal ConsumerScriptableObject consumerScriptableObject;
    [SerializeField] internal ConsumerUI consumerUI;

    internal ConsumerMood moodScript;
    internal ConsumerMove moveScript;
    internal ConsumerSpeech speechScript;
    internal ConsumerPriceCalculator priceCalculator;
    internal ConsumerIngredientHandler ingredientHandler;

    private const float COOKING_WAITING_TIME = 5f;

    /// <summary>
    /// 손님의 상태. 값을 설정할 떄 SetState() 함수를 사용합니다.
    /// </summary>
    public ConsumerState State { get; private set; }

    /// <summary>
    /// 클릭할 수 있는 상태인지 반환합니다.
    /// </summary>
    public bool IsClickable => isClickable;
    private bool isClickable = true;
    /// <summary>
    /// 클릭 된 상태인지 반환합니다.
    /// </summary>
    public bool IsClicked => isClicked;
    private bool isClicked = false;

    private bool exitCompleted;
    /// <summary>
    /// 이슈상태 전에 진행중이던 상태를 저장해놓습니다. 이슈가 지나가면 다시 cached상태로 돌아가야합니다.
    /// </summary>
    private ConsumerState cachedStateBeforeIssue = ConsumerState.Invalid;

    /// <summary>
    /// 손님 상태를 설정할 때 무조건 이 함수를 사용하도록 합니다.
    /// </summary>
    internal void SetState(ConsumerState newState)
    {
        // 이슈와 관련된 상태가 아니라면 cache해놓습니다.
        if (newState != ConsumerState.Issue 
            && newState != ConsumerState.IssueSolved 
            && newState != ConsumerState.IssueUnsolved)
        {
            cachedStateBeforeIssue = newState;
        }

        State = newState;
        if (newState != ConsumerState.Invalid)
        {
            // 손님 상태가 변할 때 말하는 것은 모두 Random + Non-Format처리합니다.
            speechScript.StartSpeechFromState(consumerScriptableObject, newState, true, false);
        }
    }

    // 추상 함수
    internal abstract void HandleChildEnter();
    internal abstract void HandleChildExit();
    internal abstract IEnumerator HandleChildUpdate();
    internal abstract void HandleChildClick();
    internal abstract void HandleChildUnclicked();

    public void OnSpawn()
    {
        Initialize();

        SetState(ConsumerState.Enter);
        StartCoroutine(UpdateCustomerBehavior());
    }

    public void OnDespawn()
    {
        StopCoroutine(UpdateCustomerBehavior());
        StopCoroutine(HandleChildUpdate());
        FinanceManager.Instance.IncreaseCurrentMoney(priceCalculator.GetFinalPrice());
        consumerUI.DeactivateAllFeedbackUIs();
    }

    public bool ShouldDespawn()
    {
        // exit 상태에서 출구로 퇴장이 완료되었을 때 true를 반환합니다.
        return exitCompleted;
    }

    private void Initialize()
    {
        ingredientHandler = gameObject.GetOrAddComponent<ConsumerIngredientHandler>();
        ingredientHandler.Initialize();

        priceCalculator = gameObject.GetOrAddComponent<ConsumerPriceCalculator>();
        priceCalculator.Initialize();

        // 스프라이트 렌더러 추가
        var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sprite = consumerScriptableObject.Appearance;
        // 박스 콜라이더 추가
        var boxCollider = gameObject.GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        // 충돌되지 않도록 trigger on
        boxCollider.isTrigger = true;
        // 손님 기분 스크립트 추가
        moodScript = gameObject.GetComponent<ConsumerMood>();
        if (moodScript == null)
        {
            moodScript = gameObject.AddComponent<ConsumerMood>();
        }
        moodScript.Initialize();
        // 손님 이동 스크립트 추가
        moveScript = gameObject.GetComponent<ConsumerMove>();
        if (moveScript == null)
        {
            moveScript = gameObject.AddComponent<ConsumerMove>();
        }
        moveScript.Initialize();
        // 손님 말하기 스크립트 추가
        speechScript = gameObject.GetComponent<ConsumerSpeech>();
        if (speechScript == null)
        {
            speechScript = gameObject.AddComponent<ConsumerSpeech>();
        }
        speechScript.Initialize(consumerUI);
        consumerUI.TransferClickEvent += WrongIngredientSpeech;

        SetState(ConsumerState.Invalid);
        exitCompleted = false;
    }

    /// <summary>
    /// 손님이 머무는 동안 해야하는 행동(update)
    /// </summary>
    private IEnumerator UpdateCustomerBehavior()
    {
        StartCoroutine(HandleChildUpdate());

        while (!ShouldDespawn())
        {
            switch (State)
            {
                case ConsumerState.Invalid:
                    Debug.LogError($"손님({gameObject.name})의 상태가 설정되지 않았습니다. 확인해주세요.");
                    break;
                case ConsumerState.Enter:
                    OnCustomerEnter();
                    break;
                case ConsumerState.Exit:
                    yield return OnCustomerExit();
                    break;
                case ConsumerState.Search:
                    yield return SearchIngredient();
                    break;
                case ConsumerState.LineUp:
                    yield return LineUp();
                    break;
                case ConsumerState.Cooking:
                    yield return Cooking();
                    break;
                // 이하로는 외부조정중. 이슈는 모든 상태에서 올 수 있으므로 주의가 필요합니다.
                case ConsumerState.Issue:
                    break;
                case ConsumerState.IssueUnsolved:
                case ConsumerState.IssueSolved:
                    SetState(cachedStateBeforeIssue); // 이슈가 끝나면 다시 예전상태로 돌아갑니다.
                    break;
                default:
                    break;
            }

            yield return null;
        }
    }

    /// <summary>
    /// 손님이 들어올 때 해야하는 행동
    /// </summary>
    private void OnCustomerEnter()
    {
        HandleChildEnter();

        ChooseIngredients();

        StartCoroutine(HandleChildOrderOnUI());
    }

    /// <summary>
    /// 손님이 떠날 때 해야하는 행동
    /// </summary>
    private IEnumerator OnCustomerExit()
    {
        var exitPoint = MoveManager.Instance.RandomExitPoint;
        while (!moveScript.IsCloseEnough(exitPoint))
        {
            moveScript.MoveTo(exitPoint);
            yield return null;
        }

        HandleChildExit();
        exitCompleted = true;
    }

    public void ChooseIngredients()
    {
        ingredientHandler.ResetAllIngredientLists();
        SetIngredientLists();
        ingredientHandler.ChooseAllIngredients();
    }

    public virtual IEnumerator HandleChildOrderOnUI()
    {
        // 대부분의 손님의 경우: 처음에 주문한 재료를 보여준 뒤 다시 비활성화 합니다
        consumerUI.ActivateIngredientUI(true);
        yield return new WaitForSeconds(IngredientManager.UI_DURATION_ON_COLLECT);
        consumerUI.ActivateIngredientUI(false);

        SetState(ConsumerState.Search);
    }

    public virtual void SetIngredientLists()
    {
        ingredientHandler.SetAllIngredientLists();
    }

    private IEnumerator SearchIngredient()
    {
        if(ingredientHandler.IsIngredientSelectDone)
        {
            // 잠시 서서 기다리는 시간도 포함합니다.
            yield return new WaitForSeconds(IngredientManager.INGREDIENT_PICKUP_TIME);

            // 재료를 다 골랐다면 줄 서러 갑니다.
            SetState(ConsumerState.LineUp);
            yield break;
        }

        // 필요한 재료를 가져옵니다.
        var neededIngredientInfo = ingredientHandler.GetNeededIngredientInfo();
        var point = neededIngredientInfo.Ingredient.Point;

        // 해당 재료를 가지러 이동합니다.
        while (!moveScript.IsCloseEnough(point))
        {
            moveScript.MoveTo(point);
            yield return null;
        }

        // 잠시 서서 기다리는 시간도 포함합니다.
        yield return new WaitForSeconds(IngredientManager.INGREDIENT_PICKUP_TIME);

        ingredientHandler.AddOwnIngredient(neededIngredientInfo.Ingredient, neededIngredientInfo.Index, neededIngredientInfo.IsCorrect);
        
        // 재료를 얻으면 잠시동안 얻은 재료를 표시해줍니다
        consumerUI.ActivateIngredientUI(true);
        yield return new WaitForSeconds(IngredientManager.UI_DURATION_ON_COLLECT);
        consumerUI.ActivateIngredientUI(false);
    }

    private IEnumerator LineUp()
    {
        // 줄 서러 갑니다.
        var waitingLinePoint = moveScript.GetWaitingLinePoint();

        while (!moveScript.IsCloseEnough(waitingLinePoint)) 
        {
            moveScript.MoveTo(waitingLinePoint);
            yield return null;
        }

        var currentLineOrder = moveScript.LineOrder;
        // 줄이 줄어들면 이동합니다.
        while (currentLineOrder > 0)
        {
            var linePoint = moveScript.GetWaitingPointInLine();
            moveScript.MoveTo(linePoint);
            currentLineOrder = moveScript.LineOrder;
            yield return null;
        }

        // 자신의 차례가 되면 다음단계로 넘어갑니다.
        SetState(ConsumerState.Cooking);
    }

    private IEnumerator Cooking()
    {
        // 일정 시간이 지나면 완료됩니다.
        yield return new WaitForSeconds(COOKING_WAITING_TIME);

        // @charotiti9 TODO: 기다리는 동안 게이지가 올라가는 UI가 필요합니다.
        
        // 지금은 임의로 Exit으로 넘깁니다.
        SetState(ConsumerState.Exit);
    }

    public void OnSpriteClicked()
    {
        Debug.Log($"손님{gameObject.GetInstanceID()}가 클릭되었습니다.");

        // 모든 Consumer 검사하여 다른 손님은 Click해제
        ConsumerManager.Instance.DeselectOtherConsumers();

        isClicked = true;

        HandleChildClick();
    }

    public void OnSpriteDeselected()
    {
        isClicked = false;

        HandleChildUnclicked();
    }

    public void WrongIngredientSpeech(int tmp = 0)
    {
        speechScript.StartSpeechFromSituation(consumerScriptableObject, ConsumerSituation.WrongIngredientDetected, true, false);
    }
}

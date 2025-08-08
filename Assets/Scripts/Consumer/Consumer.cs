using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D.Animation;

/// <summary>
/// 모든 손님에게 상속되어야합니다. 손님의 상태와 재료 등 모든 손님이 가지고 있어야하는 값과 로직을 저장하고 있습니다.
/// </summary>
public abstract class Consumer : MonoBehaviour, IPoolable
{
    [SerializeField] internal List<ConsumerScriptableObject> consumerScriptableObjectList = new();
    internal ConsumerScriptableObject currentConsumerScriptableObject;
    [SerializeField] internal ConsumerUI consumerUI;

    internal GameObject appearanceGameObject;
    internal ConsumerAppearance appearanceScript;
    internal ConsumerMood moodScript;
    internal ConsumerMove moveScript;
    internal ConsumerSpeech speechScript;
    internal ConsumerPriceCalculator priceCalculator;
    internal ConsumerIngredientHandler ingredientHandler;

    private const float ORDER_WAITING_TIME = 1f;
    private const float COOKING_WAITING_TIME = 5f;

    /// <summary>
    /// 손님의 상태. 값을 설정할 떄 SetState() 함수를 사용합니다.
    /// </summary>
    public ConsumerState State { get; private set; }

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
        Debug.Log($"[SetState] {gameObject.name} 상태 변경: {State} → {newState}");

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
            // 이슈 상태일 때는 계속 말풍선이 떠 있게, 아닐 때는 몇 초 뒤 사라지게 처리합니다.
            bool isContinue = (newState == ConsumerState.Issue);
            StartCoroutine(speechScript.StartSpeechFromState(currentConsumerScriptableObject, newState, false, isContinue, true, false));
        }
    }

    // 추상 함수
    internal abstract void HandleChildEnter();
    internal abstract void HandleChildExit();
    internal abstract IEnumerator HandleChildIssue();
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
        consumerUI.DeactivateAllFeedbackUIs();
        //외형 삭제
        Destroy(appearanceGameObject);
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
        // 외형 프리팹으로 추가
        currentConsumerScriptableObject = consumerScriptableObjectList[UnityEngine.Random.Range(0, consumerScriptableObjectList.Count)];
        if(appearanceGameObject != null)
        {
            Debug.LogError($"이미 외형 프리팹이 있는데 또 추가하고 있습니다");
        }
        appearanceGameObject = Instantiate(currentConsumerScriptableObject.AppearancePrefab, transform);
        // 외형 제어용 스크립트 추가
        appearanceScript = appearanceGameObject.GetComponent<ConsumerAppearance>();
        if (appearanceScript == null)
        {
            appearanceScript = appearanceGameObject.AddComponent<ConsumerAppearance>();
        }
        appearanceScript.Initialize();
        // 클릭 이벤트 추가 (외형이 자식으로 빠지면서 이벤트로 추가합니다)
        appearanceScript.OnClick += OnSpriteClicked;
        appearanceScript.OnDeselect += OnSpriteDeselected;
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
        StartCoroutine(UpdateBehaviorByState());
        StartCoroutine(HandleChildUpdate());

        while (!ShouldDespawn())
        {
            appearanceScript.OnUpdate();
            yield return null;
        }        
    }

    private IEnumerator UpdateBehaviorByState()
    {
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
                case ConsumerState.Leave:
                    yield return OnLeave();
                    break;
                case ConsumerState.Order:
                    yield return Order();
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
                    yield return HandleChildIssue();
                    break;
                case ConsumerState.IssueUnsolved:
                    OnIssueUnsolved();
                    break;
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
        ChooseIngredients();
        HandleChildEnter();
    }

    /// <summary>
    /// 손님이 떠날 때 해야하는 행동
    /// </summary>
    private IEnumerator OnCustomerExit()
    {
        var exitPoint = MoveManager.Instance.RandomExitPoint;
        moveScript.MoveTo(exitPoint);
        yield return new WaitUntil(() => moveScript.MoveStopIfCloseEnough(exitPoint));

        FinanceManager.Instance.IncreaseCurrentMoney(priceCalculator.GetFinalPrice());

        HandleChildExit();
        exitCompleted = true;
    }

    private IEnumerator OnLeave()
    {
        var leavePoint = MoveManager.Instance.RandomLeavePoint;
        moveScript.MoveTo(leavePoint);
        yield return new WaitUntil(() => moveScript.MoveStopIfCloseEnough(leavePoint));

        HandleChildExit();
        exitCompleted = true;
    }

    private IEnumerator Order()
    {
        // 줄 서러 갑니다
        var waitingPoint = moveScript.GetOrderWaitingPoint(this);
        moveScript.MoveTo(waitingPoint);
        yield return new WaitUntil(() => moveScript.MoveStopIfCloseEnough(waitingPoint));

        // 내 차례가 될때까지 대기합니다.
        yield return new WaitUntil(() => moveScript.IsMyTurnToOrder);

        // 주문하러 갑니다
        var orderPoint = MoveManager.Instance.GetOrderPoint();
        moveScript.MoveTo(orderPoint);
        yield return new WaitUntil(() => moveScript.MoveStopIfCloseEnough(orderPoint));

        // 주문하는 시간
        yield return new WaitForSeconds(ORDER_WAITING_TIME);

        // 주문을 다하면 Order한 재료 UI를 잠시 보여줍니다.
        StartCoroutine(HandleOrderOnUI());

        // 줄을 줄입니다
        MoveManager.Instance.PopOrderLineQueue();

        SetState(ConsumerState.Search);
    }

    public virtual void OnIssueUnsolved()
    {
        StartCoroutine(speechScript.StartSpeechFromState(currentConsumerScriptableObject, ConsumerState.IssueUnsolved, false, false, true, false));

        SetState(ConsumerState.Leave);
    }

    public void ChooseIngredients()
    {
        ingredientHandler.ResetAllIngredientLists();
        SetIngredientLists();
    }

    public virtual IEnumerator HandleOrderOnUI()
    {
        // 대부분의 손님의 경우: 처음에 주문한 재료를 보여준 뒤 다시 비활성화 합니다
        consumerUI.ActivateIngredientUI(true);
        yield return new WaitForSeconds(IngredientManager.UI_DURATION_ON_COLLECT);
        consumerUI.ActivateIngredientUI(false);
    }

    public virtual void SetIngredientLists()
    {
        ingredientHandler.SetAllIngredientLists();
        ingredientHandler.ChooseAllIngredients();
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
        var attemptIngredientInfo = ingredientHandler.GetAttemptIngredientInfo();
        var point = attemptIngredientInfo.Ingredient.Point;

        // 해당 재료를 가지러 이동합니다.
        moveScript.MoveTo(point);
        yield return new WaitUntil(() => moveScript.MoveStopIfCloseEnough(point));

        // 잠시 서서 기다리는 시간도 포함합니다.
        yield return new WaitForSeconds(IngredientManager.INGREDIENT_PICKUP_TIME);

        ingredientHandler.AddOwnIngredient(attemptIngredientInfo.Ingredient, attemptIngredientInfo.Index, attemptIngredientInfo.IsCorrect);
        // UI를 업데이트 합니다.
        consumerUI.ActivateFeedbackUIs(attemptIngredientInfo.Index, attemptIngredientInfo.IsCorrect);

        // 재료를 얻으면 잠시동안 얻은 재료를 표시해줍니다
        consumerUI.ActivateIngredientUI(true);
        yield return new WaitForSeconds(IngredientManager.UI_DURATION_ON_COLLECT);
        consumerUI.ActivateIngredientUI(false);
    }

    private IEnumerator LineUp()
    {
        // 줄 서러 갑니다.
        var waitingLinePoint = moveScript.GetCookingWaitingPoint(this);
        moveScript.MoveTo(waitingLinePoint);
        yield return new WaitUntil(() => moveScript.MoveStopIfCloseEnough(waitingLinePoint));

        // 내 차례가 될때까지 대기합니다.
        yield return new WaitUntil(() => moveScript.IsMyTurnToCooking);

        // 요리하러 갑니다
        var cookingPoint = moveScript.GetCookingPoint();
        moveScript.MoveTo(cookingPoint);
        yield return new WaitUntil(() => moveScript.MoveStopIfCloseEnough(cookingPoint));

        // 줄을 줄입니다
        moveScript.GoToCooking();

        // 주문을 다했다면 다음 차례로 넘어갑니다.
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

    /// <summary>
    /// 자식이 해야할 일이 있어서 이벤트로 뺐습니다.
    /// </summary>
    public void OnSpriteClicked()
    {
        HandleChildClick();
    }

    public void OnSpriteDeselected()
    {
        HandleChildUnclicked();
    }

    public void WrongIngredientSpeech(int tmp = 0)
    {
        StartCoroutine(speechScript.StartSpeechFromSituation(currentConsumerScriptableObject, ConsumerSituation.WrongIngredientDetected, false, false, true, false));
    }
}

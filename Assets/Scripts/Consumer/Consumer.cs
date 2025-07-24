using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 손님에게 상속되어야합니다. 손님의 상태와 재료 등 모든 손님이 가지고 있어야하는 값과 로직을 저장하고 있습니다.
/// </summary>
public abstract class Consumer : MonoBehaviour, IPoolable
{
    [SerializeField] internal ConsumerScriptableObject consumerScriptableObject;
    [SerializeField] internal ConsumerIngredientHandler ingredientHandler;
    [SerializeField] internal ConsumerUI consumerUI;
    internal ConsumerMood moodScript;

    /// <summary>
    /// 손님의 상태. 값을 설정할 떄 SetState() 함수를 사용합니다.
    /// </summary>
    public ConsumerState State { get; private set; }
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

        // Invalid 초기화일 때는 대사를 건너뜁니다.
        if (State == ConsumerState.Invalid)
        {
            return;
        }
        var line = consumerScriptableObject.GetDialogueFromState(newState);
        print($"손님{gameObject.name}: {string.Join(", ", line)}");
    }

    internal bool IsIssueSolved;
    private bool exitCompleted;

    internal List<IngredientScriptableObject> ingredients = new();

    // 추상 함수
    internal abstract void OnEnter();
    internal abstract void OnExit();
    internal abstract IEnumerator OnUpdate();

    public void OnSpawn()
    {
        Initialize();
        ingredientHandler.Initialize();

        SetState(ConsumerState.Enter);
        StartCoroutine(UpdateCustomerBehavior());
    }

    public void OnDespawn()
    {
        StopCoroutine(UpdateCustomerBehavior());
        StopCoroutine(OnUpdate());
        consumerUI.DeactivateAllFeedbackUIs();
    }

    public bool ShouldDespawn()
    {
        // exit 상태에서 출구로 퇴장이 완료되었을 때 true를 반환합니다.
        return exitCompleted;
    }

    private void Initialize()
    {
        SetState(ConsumerState.Invalid);
        IsIssueSolved = false;
        exitCompleted = false;
        ingredients.Clear();

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
    }


    /// <summary>
    /// 손님이 머무는 동안 해야하는 행동(update)
    /// </summary>
    private IEnumerator UpdateCustomerBehavior()
    {
        StartCoroutine(OnUpdate());

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
                    OnCustomerExit();
                    break;
                case ConsumerState.Search:
                    CustomerSearchIngredient();
                    // @charotiti9 TODO: 이동시키기.
                    yield return new WaitForSeconds(ingredientHandler.IngredientPickUpTime);
                    break;
                case ConsumerState.LineUp:
                    // @charotiti9 TODO: 자신의 차례가 되었을 때 Cookig으로 상태 넘기기. 지금은 임의로 Cooking으로 넘깁니다.
                    SetState(ConsumerState.Cooking);
                    break;
                case ConsumerState.Cooking:
                    // @charotiti9 TODO: 일정 시간이 지나면 완료되고 Exit으로 넘어가게 만들기. 지금은 임의로 Exit으로 넘깁니다.
                    SetState(ConsumerState.Exit);
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

        OnEnter();
        SetState(ConsumerState.Search);
    }

    /// <summary>
    /// 손님이 떠날 때 해야하는 행동
    /// </summary>
    private void OnCustomerExit()
    {
        OnExit();

        // @charotiti9 TODO: 손님이 출구로 나갔다면 Despawn 되도록 합니다. 지금은 임시로 바로 Despawn합니다.
        exitCompleted = true;
    }

    private void CustomerSearchIngredient()
    {
        // 재료를 다 골랐다면 줄 서러 갑니다.
        if (ingredientHandler.IsIngredientSelectDone)
        {
            SetState(ConsumerState.LineUp);
        }
        // 아직 다 못골랐다면 계속 고릅니다.
        else
        {
            ingredientHandler.ChooseIngredient();
        }
    }
}

using System.Collections;

/// <summary>
/// 클릭을 해서 주의를 주어야할 필요성이 있는 손님의 동작을 이곳에서 정리합니다.
/// </summary>
public class CommonConsumer : Consumer
{
    internal override void HandleChildEnter()
    {
        appearanceScript.SetClickable(GameManager.Instance.ShowIngredientCommonConsumer);
        SetState(ConsumerState.Order);
    }

    internal override void HandleChildExit()
    {
        appearanceScript.SetClickable(false);
    }

    internal override IEnumerator HandleChildIssue()
    {
        yield break;
    }

    internal override void HandleChildClick() 
    {
        // 특정 상태에서는 재료가 보이지 않습니다.(ex. 주문하기 전)
        if (State == ConsumerState.Enter || State == ConsumerState.Order)
        {
            return;
        }

        // 이슈상태가 아니라면 재료가 보이도록 합니다.
        consumerUI.ActivateIngredientUI(true);
    }
    internal override void HandleChildUnclick()
    {
        // 다른 스프라이트가 클릭되었다면 재료가 사라집니다.
        consumerUI.ActivateIngredientUI(false);
    }

    internal override IEnumerator HandleChildUpdate() 
    {
        yield break;
    }
}

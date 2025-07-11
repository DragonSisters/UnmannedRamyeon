using System.Collections;
using UnityEngine;

/// <summary>
/// 모든 손님에게 상속되어야합니다.
/// </summary>
public class Consumer : MonoBehaviour
{
    internal ConsumerState state;

    /// <summary>
    /// 손님이 들어올 때 해야하는 행동
    /// </summary>
    public virtual void OnCustomerEnter()
    {
        state = ConsumerState.Enter;
        StartCoroutine(UpdateCustomerBehavior());
    }

    /// <summary>
    /// 손님이 떠날 때 해야하는 행동
    /// </summary>
    public virtual void OnCustomerExit()
    {
        state = ConsumerState.Exit;
        StopCoroutine(UpdateCustomerBehavior());
    }

    /// <summary>
    /// 손님이 머무는 동안 해야하는 행동(update)
    /// </summary>
    internal virtual IEnumerator UpdateCustomerBehavior()
    {
        state = ConsumerState.Usual;
        yield return null;
    }
}

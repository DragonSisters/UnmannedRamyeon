using System.Collections;

/// <summary>
/// 클릭을 해서 주의를 주어야할 필요성이 있는 손님의 동작을 이곳에서 정리합니다.
/// </summary>
public class WarningConsumer : Consumer
{
    /// <summary>
    /// 주의를 주어야하는 기간. 짧을수록 어렵다.
    /// </summary>
    private float warningDuration = 2f;
    /// <summary>
    /// 기간안에 클릭되었는지 여부
    /// </summary>
    private bool isClicked;

    internal override IEnumerator UpdateCustomerBehavior()
    {
        yield return base.UpdateCustomerBehavior();

        var duration = 0f;
        while (duration < warningDuration)
        {
            duration += UnityEngine.Time.deltaTime;

            // @charotiti9 TODO: 클릭되었는지 여부를 검사해야합니다. 지금은 그냥 비워둡니다.
            print($"대기중: {state}");

            yield return null;
        }

        // 클릭되었는지 여부에 따라서 상태가 변경됩니다.
        if (isClicked)
        {
            state = ConsumerState.Smile;
        }
        else
        {
            state = ConsumerState.Upset;
        }
    }
}

using UnityEngine;
using UnityEngine.AI;

public class ConsumerMove : MonoBehaviour
{
    private float moveSpeed;
    private NavMeshAgent agent;
    private const float RANGE_THRESHOLD = 0.5f;

    public bool IsMyTurnToOrder => orderTurn == 0;
    private int orderTurn = -1;

    public bool IsMyTurnToCooking => cookingTurn == 0;
    private int cookingTurn = -1;
    private int cookingLineIndex = -1;


    public void Initialize()
    {
        moveSpeed = MoveManager.Instance.RandomMoveSpeed;

        agent = gameObject.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = moveSpeed;
        agent.radius = 0.3f;
        agent.height = 0.1f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;

        gameObject.transform.position = MoveManager.Instance.RandomEnterPoint;
    }

    public Vector2 GetIngredientPoint(IngredientScriptableObject ingredient)
    {
        return ingredient.Point + MoveManager.Instance.RandomIngredientPointFactor;
    }

    public void MoveTo(Vector3 point)
    {
        agent.SetDestination(point);
    }

    public bool IsCloseEnough(Vector2 point)
    {
        float distance = Vector2.Distance(gameObject.transform.position, point);
        return distance <= RANGE_THRESHOLD;
    }

    public Vector2 GetOrderWaitingPoint(Consumer consumer)
    {
        MoveManager.Instance.PushAndGetOrderLine(consumer, out var waitingLinePoint, out var lineTurn);
        orderTurn = lineTurn;
        return waitingLinePoint;
    }


    public void ReduceOrderLine()
    {
        orderTurn--;
    }

    /// <summary>
    /// 처음 요리줄 서러 갔을 때의 position을 반환합니다.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetCookingWaitingPoint(Consumer consumer)
    {
        MoveManager.Instance.PushAndGetCookingLine(consumer, out var lineIndex, out var lineTurn, out var waitingPoint);
        cookingLineIndex = lineIndex;
        cookingTurn = lineTurn;
        return waitingPoint;
    }

    /// <summary>
    /// 줄이 줄어들면 줄어든 줄에 따라 이동하는 자리를 반환합니다.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetCookingPoint()
    {
        return MoveManager.Instance.GetPointInCookingLine(cookingLineIndex, cookingTurn);
    }
    /// <summary>
    /// 요리가 끝나면 줄을 줄입니다.
    /// </summary>
    public void ReduceCookingLine()
    {
        cookingTurn--;
    }

    public void GoToCooking()
    {
        MoveManager.Instance.PopCookingLineQueue(cookingLineIndex);
    }
}

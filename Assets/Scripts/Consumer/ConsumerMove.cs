using UnityEngine;
using UnityEngine.AI;

public class ConsumerMove : MonoBehaviour
{
    private float moveSpeed;
    private NavMeshAgent agent;
    private const float RANGE_THRESHOLD = 0.5f;

    public int OrderLineTurn
    {
        get
        {
            if (orderLineTurn == -1)
            {
                throw new System.Exception($"아직 초기화되지 않은 LineOrder를 가져오려고 했습니다.");
            }
            return orderLineTurn;
        }
    }
    private int orderLineTurn = -1;


    public int CookingLineIndex
    {
        get
        {
            if(cookingLineIndex == -1)
            {
                throw new System.Exception($"아직 초기화되지 않은 LineIndex를 가져오려고 했습니다.");
            }
            return cookingLineIndex;
        }
    }
    private int cookingLineIndex = -1;
    public int CookingLineTurn
    {
        get
        {
            if (cookingLineTurn == -1)
            {
                throw new System.Exception($"아직 초기화되지 않은 LineOrder를 가져오려고 했습니다.");
            }
            return cookingLineTurn;
        }
    }
    private int cookingLineTurn = -1;


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

    public Vector2 GetCookingLinePoint()
    {
        MoveManager.Instance.FindFewestCookingLinePoint(out var foundLineIndex, out var foundLineOrder, out var point);
        MoveManager.Instance.PushCookingLineQueue(foundLineIndex, foundLineOrder);
        cookingLineIndex = foundLineIndex;
        cookingLineTurn = foundLineOrder;
        return point;
    }

    public Vector2 GetWaitingPointInCookingLine()
    {
        MoveManager.Instance.CalculateWaitingPointInCookingLine(cookingLineIndex, cookingLineTurn, out var point, out var newLineOrder);
        MoveManager.Instance.PopCookingLineQueue(cookingLineIndex);
        cookingLineTurn = newLineOrder;
        return point;
    }
}

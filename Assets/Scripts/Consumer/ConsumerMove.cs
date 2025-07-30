using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class ConsumerMove : MonoBehaviour
{
    private float moveSpeed;
    private NavMeshAgent agent;
    private const float RANGE_THRESHOLD = 0.5f;

    public int LineIndex
    {
        get
        {
            if(lineIndex == -1)
            {
                throw new System.Exception($"아직 초기화되지 않은 LineIndex를 가져오려고 했습니다.");
            }
            return lineIndex;
        }
    }
    private int lineIndex = -1;
    public int LineOrder
    {
        get
        {
            if (lineOrder == -1)
            {
                throw new System.Exception($"아직 초기화되지 않은 LineOrder를 가져오려고 했습니다.");
            }
            return lineOrder;
        }
    }
    private int lineOrder = -1;


    public void Initialize()
    {
        moveSpeed = MoveManager.Instance.RandomMoveSpeed;
        gameObject.transform.position = MoveManager.Instance.RandomEnterPoint;

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

    public Vector2 GetWaitingLinePoint()
    {
        MoveManager.Instance.FindFewestLinePoint(out var foundLineIndex, out var foundLineOrder, out var point);
        MoveManager.Instance.PushLineQueue(foundLineIndex, foundLineOrder);
        lineIndex = foundLineIndex;
        lineOrder = foundLineOrder;
        return point;
    }

    public Vector2 GetWaitingPointInLine()
    {
        MoveManager.Instance.CalculateWaitingPointInLine(lineIndex, lineOrder, out var point, out var newLineOrder);
        MoveManager.Instance.PopLineQueue(lineIndex);
        lineOrder = newLineOrder;
        return point;
    }
}

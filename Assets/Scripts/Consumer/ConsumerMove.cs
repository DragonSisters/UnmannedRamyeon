using UnityEngine;
using UnityEngine.AI;

public class ConsumerMove : MonoBehaviour
{
    private Consumer consumer;
    private float moveSpeed;
    private NavMeshAgent agent;
    private bool isMoving = false;
    public bool IsMoving => isMoving;

    private float separationRadius = 1.0f;
    private float separationStrength = 2.0f;
    private float separationSpeed = 5.0f;

    private const float RANGE_THRESHOLD = 0.5f;
    private const float NAVMESH_THRESHOLD = 2f;
    private const int MIN_AVOIDANCE_PRIORITY = 30;
    private const int MAX_AVOIDANCE_PRIORITY = 70;
    public bool IsMyTurnToOrder => orderTurn == 0;
    private int orderTurn = -1;

    public bool IsMyTurnToCooking => cookingTurn == 0;
    private int cookingTurn = -1;
    private int cookingLineIndex = -1;


    public void Initialize()
    {
        consumer = gameObject.GetComponent<Consumer>();
        if(consumer == null)
        {
            Debug.LogError("Consumer 컴포넌트를 찾을 수 없습니다. ConsumerMove 스크립트가 Consumer 컴포넌트와 함께 사용되어야 합니다.");
            return;
        }
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
        // 에이전트마다 우선순위 부여 (중간값이 50이니 30~70으로 분포시킵니다)
        agent.avoidancePriority = Random.Range(MIN_AVOIDANCE_PRIORITY, MAX_AVOIDANCE_PRIORITY);

        gameObject.transform.position = MoveManager.Instance.RandomEnterPoint;
    }

    public void OnUpdate()
    {
        //  움직이지 않는 agent가 다른 에이전트를 위해 피해줍시다.
        if (!agent.hasPath)
        {
            AvoidNearbyAgents();
        }
    }

    public Vector2 GetIngredientPoint(IngredientScriptableObject ingredient)
    {
        return ingredient.Point + MoveManager.Instance.RandomIngredientPointFactor;
    }

    public void MoveToNearesetPoint(Vector2 point, out Vector2 nearestPoint)
    {
        isMoving = true;
        nearestPoint = GetNearestPointOnNavMesh(point);
        agent.SetDestination(nearestPoint);
    }

    /// <summary>
    /// targetPos 근처에서 가장 가까운 NavMesh 위의 위치를 반환.
    /// maxDistance 안에서 NavMesh가 없으면 false 반환.
    /// </summary>
    private Vector2 GetNearestPointOnNavMesh(Vector2 targetPos, int areaMask = NavMesh.AllAreas)
    {
        NavMeshHit hit;
        var nearestPos = targetPos;
        if (NavMesh.SamplePosition(targetPos, out hit, NAVMESH_THRESHOLD, areaMask))
        {
            nearestPos = hit.position;
        }
        else
        {
            Debug.LogError($"손님이 도달가능한 NavMesh가 없습니다!");
        }
        return nearestPos;
    }

    // Destination에 충분히 도달했는지 IsCloseEnough로 확인하고, 충분하다면 Destination을 취소합니다.
    public bool MoveStopIfCloseEnough(Vector3 point)
    {
        if (IsCloseEnough(point))
        {
            isMoving = false;
            agent.ResetPath();
            return true;
        }
        return false;
    }

    private bool IsCloseEnough(Vector2 point)
    {
        float distance = Vector2.Distance(gameObject.transform.position, point);
        return distance <= RANGE_THRESHOLD;
    }

    public void AvoidNearbyAgents()
    {
        var separation = Vector2.zero;
        int count = 0;
        var allAgents = ConsumerManager.Instance.GetAllActiveConsumerToList();
        foreach (var otherAgent in allAgents)
        {
            if (otherAgent == consumer)
            {
                continue; // 자기 자신은 제외
            }
            float distance = Vector2.Distance(otherAgent.transform.position, transform.position);
            if (distance < separationRadius && distance > 0.01f) // 0 방지
            {
                Vector2 direction = (Vector2)(transform.position - otherAgent.transform.position).normalized;
                separation += direction / distance; // 거리에 반비례하여 힘을 적용
                count++;
            }
        }

        if (count > 0)
        {
            separation /= count; // 평균화
            separation *= separationStrength; // 힘의 크기 조절
            var desired = (Vector2)agent.desiredVelocity + separation;

            // NaN 방지
            if (!float.IsNaN(desired.x) && !float.IsNaN(desired.y))
            {
                // 약간 밀어내기
                agent.velocity = Vector3.Lerp(agent.velocity, desired, Time.deltaTime * separationSpeed);
            }
        }
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

using UnityEngine;
using UnityEngine.AI;

public class ConsumerMove : MonoBehaviour
{
    private float moveSpeed;
    private NavMeshAgent agent;
    private const float RANGE_THRESHOLD = 0.5f;

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
}

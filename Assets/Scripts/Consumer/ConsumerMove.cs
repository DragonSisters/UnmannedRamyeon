using UnityEngine;
using UnityEngine.UIElements;

public class ConsumerMove : MonoBehaviour
{
    private float moveSpeed;

    public void Initialize()
    {
        moveSpeed = MoveManager.Instance.RandomMoveSpeed;
        gameObject.transform.position = MoveManager.Instance.RandomEnterPoint;
    }

    public Vector2 GetIngredientPoint(IngredientScriptableObject ingredient)
    {
        return ingredient.Point + MoveManager.Instance.RandomIngredientPointFactor;
    }

    public void MoveTo(Vector3 point)
    {
        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, point, moveSpeed * Time.deltaTime);
    }

    public bool IsCloseEnough(Vector2 point)
    {
        float distance = Vector2.Distance(gameObject.transform.position, point);
        return distance <= MoveManager.RANGE_THRESHOLD;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveManager : Singleton<MoveManager>
{
    [Header("입퇴장")]
    [SerializeField] private Transform enterPoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private Transform shoutMinPoint;
    [SerializeField] private Transform shoutMaxPoint;
    [SerializeField] private Transform leavePoint;

    private const float MOVE_SPEED = 2;
    private const float MOVE_SPEED_RANGE = 0.5f;
    private const float ENTER_POINT_RANGE = 0.5f;
    private const float EXIT_POINT_RANGE = 0.5f;
    private const float LEAVE_POINT_RANGE = 0.5f;

    public float RandomMoveSpeed => Random.Range(-MOVE_SPEED_RANGE, MOVE_SPEED_RANGE) + MOVE_SPEED;
    public Vector2 RandomEnterPoint
    {
        get
        {
            var randomX = Random.Range(-ENTER_POINT_RANGE, ENTER_POINT_RANGE);
            var randomY = Random.Range(-ENTER_POINT_RANGE, ENTER_POINT_RANGE);

            return (Vector2)enterPoint.position + new Vector2(randomX, randomY);
        }
    }
    public Vector2 RandomExitPoint
    {
        get
        {
            var randomX = Random.Range(-EXIT_POINT_RANGE, EXIT_POINT_RANGE);
            var randomY = Random.Range(-EXIT_POINT_RANGE, EXIT_POINT_RANGE);

            return (Vector2)exitPoint.position + new Vector2(randomX, randomY);
        }
    }

    public Vector2 RandomShoutPoint
    {
        get
        {
            var randomX = Random.Range(shoutMinPoint.position.x, shoutMaxPoint.position.x);
            var randomY = Random.Range(shoutMinPoint.position.y, shoutMaxPoint.position.y);

            return new Vector2(randomX, randomY);
        }
    }

    public Vector2 RandomLeavePoint
    {
        get
        {
            var randomX = Random.Range(-LEAVE_POINT_RANGE, LEAVE_POINT_RANGE);
            var randomY = Random.Range(-LEAVE_POINT_RANGE, LEAVE_POINT_RANGE);

            return (Vector2)leavePoint.position + new Vector2(randomX, randomY);
        }
    }
    [Header("재료")]
    [SerializeField] private const float INGREDIENT_POINT_RANGE = 0.2f;
    public Vector2 RandomIngredientPointFactor
    {
        get
        {
            var randomX = Random.Range(-INGREDIENT_POINT_RANGE, INGREDIENT_POINT_RANGE);
            var randomY = Random.Range(-INGREDIENT_POINT_RANGE, INGREDIENT_POINT_RANGE);

            return new Vector2(randomX, randomY);
        }
    }


    [Header("줄서기")]
    [SerializeField] private int lineCount = 4;
    [SerializeField] private float lineSpacingFactor = 1f;
    [SerializeField] private Transform orderLineStartingPoint;
    [SerializeField] private Transform[] cookingLineStartingPoint;

    private Queue<Consumer> orderLine = new();
    private List<Queue<Consumer>> cookingLines = new();
    private Dictionary<int, Queue<Consumer>> cookingLineList = new(); // lineIndex, lineCount
    private List<IngredientScriptableObject> ingredientScriptableObjects = new();

    public void OnGameEnter()
    {
        orderLine.Clear();
        cookingLines.Clear();
        cookingLineList.Clear();

        ingredientScriptableObjects = IngredientManager.Instance.IngredientScriptableObject;

        for (int i = 0; i < lineCount; i++)
        {
            cookingLines.Add(new Queue<Consumer>());
            cookingLineList.Add(i, cookingLines[i]);
        }
    }

    public Vector2 GetIngredientPoint(IngredientScriptableObject targetIngredient)
    {
        var point = Vector2.negativeInfinity;
        foreach (var ingredient in ingredientScriptableObjects)
        {
            if(ingredient.name == targetIngredient.name)
            {
                break;
            }
        }

        if(point == Vector2.negativeInfinity)
        {
            throw new System.Exception($"재료 {targetIngredient.name}를 찾을 수 없습니다. scriptableObject를 확인해주세요.");
        }

        return point;
    }

    /// <summary>
    /// 손님이 처음 계산줄에 설 때 어디 서야하는지 알려줍니다.
    /// </summary>
    /// <param name="lineTurn"></param>
    /// <returns></returns>
    public void PushAndGetOrderLine(Consumer consumer, out Vector2 waitingPosition, out int lineTurn)
    {
        orderLine.Enqueue(consumer);
        lineTurn = orderLine.Count - 1;
        var startingPoint = orderLineStartingPoint.position;
        waitingPosition = startingPoint;
        // 자신의 차례가 아니라면 좀 떨어져있습니다
        waitingPosition.x += lineTurn == 0 ? 0 : lineSpacingFactor;
    }

    /// <summary>
    /// 손님이 계산 줄에서 나갈 때 호출됩니다
    /// </summary>
    public void PopOrderLineQueue()
    {
        orderLine.Dequeue();

        // 다른 기다리는 consumer들은 대기순번을 1씩 내립니다
        foreach (var consumer in orderLine)
        {
            consumer.moveScript.ReduceOrderLine();
        }
    }

    public Vector2 GetOrderPoint()
    {
        return orderLineStartingPoint.position;
    }

    /// <summary>
    /// 본인이 계산할 차례인지를 보고, 서 있어야하는 위치를 반환합니다.
    /// </summary>
    /// <param name="lineTurn"></param>
    /// <returns></returns>
    public void PushAndGetCookingLine(Consumer consumer, out int lineIndex, out int lineTurn, out Vector2 waitingPoint)
    {
        lineIndex = FindLineWithFewestConsumers();
        cookingLineList[lineIndex].Enqueue(consumer);
        lineTurn = cookingLineList[lineIndex].Count - 1;

        waitingPoint = GetPointInCookingLine(lineIndex, lineTurn);
    }

    public void PopCookingLineQueue(int lineIndex)
    {
        cookingLineList[lineIndex].Dequeue();

        // 다른 기다리는 consumer들은 대기순번을 1씩 내립니다
        foreach (var consumer in cookingLineList[lineIndex])
        {
            consumer.moveScript.ReduceCookingLine();
        }
    }

    public Vector2 GetCookingPoint(int lineIndex)
    {
        return cookingLineStartingPoint[lineIndex].position;
    }

    /// <summary>
    /// 요리줄이 줄어들면 해당 줄에서 어디에 서있어야하는지 계산해줍니다.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetPointInCookingLine(int lineIndex, int lineTurn)
    {
        var startingPoint = cookingLineStartingPoint[lineIndex].position;
        var waitingPosition = startingPoint;

        // 자신의 차례가 아니라면 좀 떨어져있습니다
        waitingPosition.x += lineTurn == 0 ? 0 : lineSpacingFactor;

        return waitingPosition;
    }

    /// <summary>
    /// 가장 수가 적은 요리줄을 찾아줍니다
    /// </summary>
    private int FindLineWithFewestConsumers()
    {
        var minConsumerCount = int.MaxValue;
        var minLineIndex = cookingLineList.Count == 0 ? 0 : -1;
        for (int i = 0; i < cookingLineList.Count; i++)
        {
            var consumerCount = cookingLineList[i].Count;
            if (consumerCount < minConsumerCount)
            {
                minLineIndex = i;
                minConsumerCount = consumerCount;
            }
        }

        return minLineIndex;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveManager : Singleton<MoveManager>
{
    [Header("입퇴장")]
    [SerializeField] private Transform enterPoint;
    [SerializeField] private Transform exitPoint;

    private const float MOVE_SPEED = 2;
    private const float MOVE_SPEED_RANGE = 0.5f;
    private const float ENTER_POINT_RANGE = 0.5f;

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
    [SerializeField] private Transform[] lineStartingPoint;
    
    private List<(Queue line, Vector2 startingPoint)> lineList = new();
    private List<IngredientScriptableObject> ingredientScriptableObjects = new();

    public void OnGameEnter()
    {
        ingredientScriptableObjects = IngredientManager.Instance.IngredientScriptableObject;
        for (int i = 0; i < lineCount; i++)
        {
            lineList.Add((new Queue(), lineStartingPoint[i].position));
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
    /// 손님이 처음에 줄을 고를 때, 어디 서있어야 하는지 계산하여 반환해줍니다.
    /// </summary>
    /// <returns></returns>
    public void FindFewestLinePoint(out int lineIndex, out int lineOrder, out Vector2 point)
    {
        lineIndex = FindLineWithFewestConsumers();
        lineOrder = lineList[lineIndex].line.Count;
        CalculateWaitingPointInLine(lineIndex, lineOrder, out point, out var newLineOrder);
    }

    /// <summary>
    /// 줄이 줄어들면 해당 줄에서 어디에 서있어야하는지 계산해줍니다.
    /// </summary>
    /// <returns></returns>
    public void CalculateWaitingPointInLine(int lineIndex, int lineOrder, out Vector2 waitingPosition, out int newLineOrder)
    {
        var startingPoint = lineList[lineIndex].startingPoint;
        waitingPosition = startingPoint;

        // 자신이 몇번째로 서있는지에 따라 fator를 더해 뒤에 섭니다.
        for (int i = 0; i < lineOrder; i++)
        {
            waitingPosition.x += lineSpacingFactor;
        }

        // 새로 몇번째로 서있는지 갱신해둡니다
        newLineOrder = lineOrder - 1;
        if(newLineOrder < 0)
        {
            newLineOrder = 0;
        }
    }

    /// <summary>
    /// 가장 수가 적은 줄을 찾아줍니다
    /// </summary>
    private int FindLineWithFewestConsumers()
    {
        var minConsumerCount = int.MaxValue;
        var minLineIndex = lineList.Count == 0 ? 0 : -1;
        for (int i = 0; i < lineList.Count; i++)
        {
            var consumerCount = lineList[i].line.Count;
            if (consumerCount < minConsumerCount)
            {
                minLineIndex = i;
                minConsumerCount = consumerCount;
            }
        }

        return minLineIndex;
    }

}

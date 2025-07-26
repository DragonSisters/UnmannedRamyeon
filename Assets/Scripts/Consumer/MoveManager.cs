using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveManager : Singleton<MoveManager>
{
    [Header("입퇴장")]
    [SerializeField] private Transform enterPoint;
    [SerializeField] private Transform exitPoint;
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
    public Vector2 SelectWaitingLine()
    {
        var lineIndex = FindLineWithFewestConsumers();
        return CalculateWaitingPositionInLine(lineIndex);
    }

    /// <summary>
    /// 줄이 줄어들면 해당 줄에서 어디에 서있어야하는지 계산해줍니다.
    /// </summary>
    /// <returns></returns>
    public Vector2 CalculateWaitingPositionInLine(int lineIndex)
    {
        var startingPoint = lineList[lineIndex].startingPoint;
        var waitingPosition = startingPoint;

        // 손님이 줄서있는 수만큼 fator를 더해 뒤에 섭니다.
        for (int i = 0; i < lineList[lineIndex].line.Count; i++)
        {
            waitingPosition.x += lineSpacingFactor;
        }

        return waitingPosition;
    }

    /// <summary>
    /// 가장 수가 적은 줄을 찾아줍니다
    /// </summary>
    private int FindLineWithFewestConsumers()
    {
        var minConsumerCount = int.MaxValue;
        var minLineIndex = -1;
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

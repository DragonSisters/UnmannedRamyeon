using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineUpManager : Singleton<LineUpManager>
{
    [SerializeField] private int lineCount = 4;
    [SerializeField] private float lineSpacingFactor = 1f;
    [SerializeField] private Transform[] lineStartingPoint;
    List<(Queue line, Vector2 startingPoint)> lineList = new();

    // @charotiti9 TODO: 이동기능이 구현되면 GameManager에서 호출하도록 합시다
    public void OnGameStart()
    {
        for (int i = 0; i < lineCount; i++)
        {
            lineList.Add((new Queue(), lineStartingPoint[i].position));
        }
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
        for (int i = 0; i < lineList.Count - 1; i++)
        {
            var consumerCount = lineList[i].line.Count;
            if(consumerCount < minConsumerCount)
            {
                minLineIndex = i;
                minConsumerCount = consumerCount;
            }
        }

        return minLineIndex;
    }

}

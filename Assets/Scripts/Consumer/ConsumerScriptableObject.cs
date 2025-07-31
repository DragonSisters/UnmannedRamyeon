using System.Collections.Generic;
using UnityEngine;

public enum ConsumerState
{
    Invalid = -1, // 설정이 되지 않은, 정상이 아닌 상태입니다. 예외처리해야합니다.
    
    Enter, // 입장
    Exit, // 퇴장
    Search, // 재료 찾기
    LineUp, // 줄서기
    Cooking, // 라면 끓이기
    Issue, // 문제가 일어난 상태
    IssueUnsolved, // 이슈처리 실패
    IssueSolved, // 이슈처리 성공
}

[CreateAssetMenu(fileName = "Consumer", menuName = "Scriptable Objects/Consumer")]
public class ConsumerScriptableObject : ScriptableObject
{
    /// <summary>
    /// 외형
    /// </summary>
    [SerializeField] private Sprite appearance;

    [System.Serializable]
    public struct Dialogue
    {
        public ConsumerState state;
        public List<string> line;
    }
    /// <summary>
    /// 상황에 따른 대사
    /// </summary>
    public List<Dialogue> dialogues = new();

    /// <summary>
    /// 찾아오는 최소 날짜(단계)
    /// </summary>
    public int AppearDate => appearDate;
    [SerializeField] private int appearDate;

    public Sprite Appearance => appearance;

    public string GetRandomDialogueFromState(ConsumerState state)
    {
        if (TryGetStateLines(state, out var lines))
        {
            return lines[Random.Range(0, lines.Count)];
        }
        return "";
    }

    public string GetDialogueFromState(ConsumerState state, int index)
    {
        if(TryGetStateLines(state, out var lines))
        {
            if (index > lines.Count)
            {
                throw new System.Exception($"손님의 대사 중 {state}에서 {index}번째 대사는 존재하지 않습니다.");
            }
            return lines[index];
        }
        return "";
    }


    private bool TryGetStateLines(ConsumerState state, out List<string> lines)
    {
        foreach (var dialogue in dialogues)
        {
            if (dialogue.state == state)
            {
                lines = dialogue.line;
                return true;
            }
        }

        lines = null;
        return false;
    }
}

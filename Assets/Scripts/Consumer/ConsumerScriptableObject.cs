using System.Collections.Generic;
using UnityEngine;

public enum ConsumerState
{
    Invalid = -1, // 설정이 되지 않은, 정상이 아닌 상태입니다. 예외처리해야합니다.
    
    Enter, // 입장
    Exit, // 퇴장
    Leave, // 주문 실패해서 가게를 떠남
    Order, // 주문하기
    Search, // 재료 찾기
    LineUp, // 줄서기
    Cooking, // 라면 끓이기
    Issue, // 문제가 일어난 상태
    IssueUnsolved, // 이슈처리 실패
    IssueSolved, // 이슈처리 성공
}

public enum ConsumerSituation
{
    Invalid = -1, // 설정이 되지 않은, 정상이 아닌 상태입니다. 예외처리해야합니다.

    WrongIngredientDetected, // 틀린 재료를 지적당했을 때

}
[CreateAssetMenu(fileName = "Consumer", menuName = "Scriptable Objects/Consumer")]
public class ConsumerScriptableObject : ScriptableObject
{
    /// <summary>
    /// 외형
    /// </summary>
    [SerializeField] private Sprite appearance;

    [System.Serializable]
    public struct StateDialogue
    {
        public ConsumerState state;
        public List<string> line;
    }
    [System.Serializable]
    public struct SituationDialogue
    {
        public ConsumerSituation situation;
        public List<string> line;
    }
    /// <summary>
    /// 상황에 따른 대사
    /// </summary>
    public List<StateDialogue> stateDialogues = new();
    public List<SituationDialogue> situationDialogues = new();

    /// <summary>
    /// 찾아오는 최소 날짜(단계)
    /// </summary>
    public int AppearDate => appearDate;
    [SerializeField] private int appearDate;

    public Sprite Appearance => appearance;

    public string GetRandomDialogueFromState(ConsumerState state, string format = "")
    {
        if (TryGetStateLines(state, out var lines))
        {
            var randomLine = lines[Random.Range(0, lines.Count)];
            // format이 있다면 적용합니다
            randomLine = string.Format(randomLine, format);
            return randomLine;
        }
        return "";
    }

    public string GetDialogueFromState(ConsumerState state, int index, string format = "")
    {
        if(TryGetStateLines(state, out var lines))
        {
            if (index > lines.Count)
            {
                throw new System.Exception($"손님의 대사 중 {state}에서 {index}번째 대사는 존재하지 않습니다.");
            }
            var line = lines[index];
            // format이 있다면 적용합니다
            line = string.Format(line, format);
            return line;
        }
        return "";
    }

    public string GetRandomDialogueFromSituation(ConsumerSituation situation, string format = "")
    {
        if (TryGetSituationLines(situation, out var lines))
        {
            var randomLine = lines[Random.Range(0, lines.Count)];
            // format이 있다면 적용합니다
            randomLine = string.Format(randomLine, format);
            return randomLine;
        }
        return "";
    }

    public string GetDialogueFromSituation(ConsumerSituation situation, int index, string format = "")
    {
        if (TryGetSituationLines(situation, out var lines))
        {
            if (index > lines.Count)
            {
                throw new System.Exception($"손님의 대사 중 {situation}에서 {index}번째 대사는 존재하지 않습니다.");
            }
            var line = lines[index];
            // format이 있다면 적용합니다
            line = string.Format(line, format);
            return line;
        }
        return "";
    }

    private bool TryGetStateLines(ConsumerState state, out List<string> lines)
    {
        foreach (var dialogue in stateDialogues)
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

    private bool TryGetSituationLines(ConsumerSituation situation, out List<string> lines)
    {
        foreach (var dialogue in situationDialogues)
        {
            if (dialogue.situation == situation)
            {
                lines = dialogue.line;
                return true;
            }
        }

        lines = null;
        return false;
    }
}

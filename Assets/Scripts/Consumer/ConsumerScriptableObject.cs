using System.Collections.Generic;
using UnityEngine;

public enum ConsumerState
{
    Invalid = -1, // 설정이 되지 않은, 정상이 아닌 상태입니다. 예외처리해야합니다.
    
    Usual, // 아무것도 아닌 상태입니다.
    Enter, // 등장
    Exit, // 퇴장
    Upset, // 화남
    Smile, // 좋음
}

[CreateAssetMenu(fileName = "Consumer", menuName = "Scriptable Objects/Consumer")]
public class ConsumerScriptableObject : ScriptableObject
{
    /// <summary>
    /// 붙어야하는 손님 스크립트 프리팹
    /// </summary>
    public GameObject consumerPrefab;

    /// <summary>
    /// 외형
    /// </summary>
    public Sprite appearance;

    [System.Serializable]
    public struct Dialogue
    {
        public ConsumerState state;
        public string line;
    }
    /// <summary>
    /// 상황에 따른 대사
    /// </summary>
    public List<Dialogue> dialogues = new();

    /// <summary>
    /// 체류 시간
    /// </summary>
    public float durationTime;

    /// <summary>
    /// 찾아오는 최소 날짜(단계)
    /// </summary>
    public int appearDate;
}

using TMPro;
using UnityEngine;
using System.Collections;

public class GrandmaTalkUI : MonoBehaviour
{
    [SerializeField] private GameObject grandmaSpeechBubble;
    [SerializeField] private TMP_Text txt_grandmaSpeech;
    private float grandmaSpeechTime = 15f;

    [TextArea]
    [SerializeField]
    private string[] grandmaDialogues = new string[5]
    {
        "아이고~ 나 디스크 터졌을 때 다 키오스크로 바꿨어! 뭘 알바를 한다고 그랴~",
        "흠... 그래 저런 손님들은 너가 챙겨줘야 할지도?",
        "이제 제법 하는데?",
        "그래그래 이렇게만 해다오",
        "아이고 너한테 가게 맡겨놓고 난 여행가야겠다~"
    };

    private int lastStep = -1;

    // 외부에서 매출이 변경될 때 호출
    public IEnumerator UpdateGrandmaTalk(int currentMoney, int goalMoney)
    {
        if (goalMoney <= 0) yield break;

        // 전체 매출 목표를 5등분해서 현재 단계 계산
        int step = Mathf.Clamp((currentMoney * 5) / goalMoney, 0, 5);

        // 같은 단계면 말풍선 갱신 안 함
        if (step == lastStep) yield break;

        lastStep = step;
        ShowGrandmaTalk(grandmaDialogues[step]);

        yield return new WaitForSeconds(grandmaSpeechTime);

        HideGrandmaTalk();
    }

    private void ShowGrandmaTalk(string message)
    {
        if(grandmaSpeechBubble == null)
        {
            Debug.LogError($"{nameof(grandmaSpeechBubble)}을 찾을 수 없습니다");
            return;
        }
        grandmaSpeechBubble.SetActive(true);

        if (txt_grandmaSpeech == null)
        {
            Debug.LogError($"{nameof(txt_grandmaSpeech)}을 찾을 수 없습니다");
            return;
        }
        txt_grandmaSpeech.text = message;
    }

    public void HideGrandmaTalk()
    {
        if (grandmaSpeechBubble == null)
        {
            Debug.LogError($"{nameof(grandmaSpeechBubble)}을 찾을 수 없습니다");
            return;
        }
        grandmaSpeechBubble.SetActive(false);
    }

    public void ResetGrandmaTalk()
    {
        lastStep = -1;
        grandmaSpeechBubble.SetActive(false);
    }
}

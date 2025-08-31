using TMPro;
using UnityEngine;
using System.Collections;

public class GrandmaTalkUI : MonoBehaviour
{
    [SerializeField] private GameObject grandmaSpeechBubble;
    [SerializeField] private TMP_Text txt_grandmaSpeech;
    private float grandmaSpeechTime = 1f;
    private float typingTime = 0.05f;
    private int lastStep = -1;
    private Coroutine typingCoroutine;

    [TextArea]
    [SerializeField]
    private string[] grandmaDialogues = new string[5]
    {
        "아이고~ 나 디스크 터졌을 때 다 키오스크로 바꿨어! 뭘 알바를 한다고 그랴~",
        "흠... 그래 저런 손님들은 너가 챙겨줘야 할지도?",
        "이제 제법 하는데? 이렇게만 해다오",
        "손님들이 너 칭찬이 자자하다! 우리 손녀 최고~",
        "아이고 너한테 가게 맡겨놓고 난 여행가야겠다~"
    };

    public void Initialize()
    {
        FinanceManager.Instance.OnFinancialManagerStart += ResetGrandmaTalk;
        FinanceManager.Instance.OnCurrentMoneyUpdate += (() => StartCoroutine(UpdateGrandmaTalk()));
    }

    // 외부에서 매출이 변경될 때 호출
    public IEnumerator UpdateGrandmaTalk()
    {
        int currentMoney = (int) FinanceManager.Instance.CurrentMoney;
        int goalMoney = FinanceManager.Instance.GoalMoney;

        if (goalMoney <= 0) yield break;

        // 전체 매출 목표를 5등분해서 현재 단계 계산
        int step = Mathf.Clamp((currentMoney * 5) / goalMoney, 0, 5);

        // 같은 단계면 말풍선 갱신 안 함
        if (step == lastStep || step >= grandmaDialogues.Length) yield break;

        lastStep = step;
        yield return StartCoroutine(ShowGrandmaTalk(grandmaDialogues[step]));

        // 다 쓴 후 잠깐 유지
        yield return new WaitForSeconds(grandmaSpeechTime);

        HideGrandmaTalk();
    }

    private IEnumerator ShowGrandmaTalk(string message)
    {
        if(grandmaSpeechBubble == null)
        {
            Debug.LogError($"{nameof(grandmaSpeechBubble)}을 찾을 수 없습니다");
            yield break;
        }
        grandmaSpeechBubble.SetActive(true);

        if (txt_grandmaSpeech == null)
        {
            Debug.LogError($"{nameof(txt_grandmaSpeech)}을 찾을 수 없습니다");
            yield break;
        }
        // 코루틴 중복 실행 방지
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(message));
        yield return typingCoroutine;
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

    private IEnumerator TypeText(string message)
    {
        txt_grandmaSpeech.text = "";

        foreach (char c in message)
        {
            txt_grandmaSpeech.text += c;
            yield return new WaitForSeconds(typingTime); // 글자 하나 나오는 간격
        }
    }
}

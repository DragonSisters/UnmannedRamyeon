using TMPro;
using UnityEngine;

public class PriceUI : MonoBehaviour
{
    [SerializeField] private Animation anim;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text comboMultiplierText;

    public void SetValues(int money, float comboMultiplier)
    {
        if (money < 0)
        { 
            moneyText.color = Color.red; // 돈이 감소하는 경우는 빨간색
        }
        else
        {
            moneyText.color = Color.green; // 돈이 증가하는 경우는 초록색
        }

        var sign = money < 0 ? "" : "+"; // 돈이 감소하는 경우 자동으로 -가 들어옵니다.
        moneyText.text = string.Format("{0}{1}", sign, money.ToString());

        if (comboMultiplier > 1f)
        {
            comboMultiplierText.text = string.Format("x {0:f2}", comboMultiplier);
            comboMultiplierText.gameObject.SetActive(true);
        }
        else
        {
            comboMultiplierText.text = "";
            comboMultiplierText.gameObject.SetActive(false);
        }
    }

    public void PlayAnimation()
    {
        anim.Play();
    }
}

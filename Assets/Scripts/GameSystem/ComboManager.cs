using TMPro;
using UnityEngine;

public class ComboManager : Singleton<ComboManager>
{
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private Animation comboAnimation;
    private const string COMBO_PREFIX = "COMBO ";

    public int ComboCount => comboCount;
    private int comboCount = 0;

    public void ResetCombo()
    {
        comboCount = 0;
        UpdateComboText();
    }

    public void IncreaseCombo()
    {
        comboCount++;
        UpdateComboText();
    }

    private void UpdateComboText()
    {
        if (comboCount > 0)
        {
            comboText.text = COMBO_PREFIX + comboCount.ToString();
            comboText.gameObject.SetActive(true);
            comboAnimation.Play();
        }
        else
        {
            comboText.gameObject.SetActive(false);
        }
    }
}

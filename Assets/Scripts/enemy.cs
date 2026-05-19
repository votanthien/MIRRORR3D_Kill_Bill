using UnityEngine;
using UnityEngine.UI;

public class enemy : MonoBehaviour
{
    public int enemyMaxHp = 200;
    public int enemyCurrentHp = 200;
    public int enemyAttack = 15;

    [Header("UI")]
    public Slider hpSlider;

    private void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        hpSlider.maxValue = enemyMaxHp;
        hpSlider.value = enemyCurrentHp;
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStat : MonoBehaviour
{
    [Header("RPG Stats")]
    public int playerMaxHp = 100;
    public int playerCurrentHp = 100;

    public int playerMana = 0;
    public int playerMaxMana = 50;

    [Header("Shield")]
    public int playerShield = 0;
    public int maxShield = 2;

    public int playerAttack = 10;
    public int playerSp = 15;

    [Header("UI")]
    public Slider hpSlider;
    public Slider manaSlider;

    public TextMeshProUGUI shieldText;

    // Image của icon khiên
    public Image shieldImage;
    public GameObject khien1;
    public GameObject khien2;

    // Màu
    Color emptyShieldColor;
    Color activeShieldColor;

    private void Start()
    {
        ColorUtility.TryParseHtmlString("#202020", out emptyShieldColor);
        ColorUtility.TryParseHtmlString("#FFFFFF", out activeShieldColor);
        khien1.SetActive(false);
        khien2.SetActive(false);
        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        // HP
        hpSlider.maxValue = playerMaxHp;
        hpSlider.value = playerCurrentHp;

        // Mana
        manaSlider.maxValue = playerMaxMana;
        manaSlider.value = playerMana;

        // Text số khiên
        shieldText.text = playerShield + "/" + maxShield
        ;

        // Đổi màu ICON khiên
        if (playerShield <= 0)
        {
            shieldImage.color = emptyShieldColor;
            khien1.SetActive(false);
            khien2.SetActive(false);
        }
        if (playerShield == 1)
        {
            shieldImage.color = activeShieldColor; 
            khien1.SetActive(true);
            khien2.SetActive(false);
        }
        if (playerShield == 2)
        {
            shieldImage.color = activeShieldColor; 
            khien1.SetActive(true);
            khien2.SetActive(true);
        }
       
    }

    public void AddShield(int amount)
    {
        if (playerShield >= maxShield)
            return;

        playerShield += amount;

        if (playerShield > maxShield)
            playerShield = maxShield;

        UpdateUI();
    }
}
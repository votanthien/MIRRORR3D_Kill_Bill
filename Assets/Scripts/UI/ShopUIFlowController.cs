using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Điều phối luồng UI ShopScene theo đặc tả:
/// CanvasSelect, CanvasStats, CanvasUpgrade (PanelHide / PanelShow).
/// </summary>
public class ShopUIFlowController : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private GameObject _canvasSelect;
    [SerializeField] private GameObject _canvasStats;

    [Header("CanvasSelect — chọn nhân vật")]
    [SerializeField] private RectTransform _chooseSelect;
    [SerializeField] private Button[] _characterButtons;

    [Tooltip("Tên hiển thị tương ứng G1–G5. Nếu thiếu phần tử sẽ fallback G1, G2...")]
    [SerializeField] private string[] _characterNames;

    [Header("CanvasStats — thông tin nhân vật")]
    [SerializeField] private Text _nameGText;
    [SerializeField] private Button _statsBackButton;
    [SerializeField] private TabGroup _statsTabGroup;

    [Header("CanvasUpgrade — nâng cấp")]
    [SerializeField] private GameObject _panelHide;
    [SerializeField] private GameObject _panelShow;
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private Button _upgradeBackButton;
    [SerializeField] private TabGroup _upgradeTabGroup;

    private int _selectedCharacterIndex = -1;

    private void Awake()
    {
        WireCharacterButtons();
        WireBackAndUpgradeButtons();
    }

    private void Start()
    {
        ApplyDefaultState();
    }

    #region Trạng thái tổng (mục 2 & 5 đặc tả)

    /// <summary>Trạng thái mặc định khi không thao tác.</summary>
    public void ApplyDefaultState()
    {
        _selectedCharacterIndex = -1;

        if (_canvasSelect != null)
        {
            _canvasSelect.SetActive(true);
        }

        if (_canvasStats != null)
        {
            _canvasStats.SetActive(false);
        }

        if (_chooseSelect != null)
        {
            _chooseSelect.gameObject.SetActive(false);
        }

        if (_panelHide != null)
        {
            _panelHide.SetActive(true);
        }

        if (_panelShow != null)
        {
            _panelShow.SetActive(false);
        }

        SetNameGVisible(false);
    }

    #endregion

    #region Luồng chọn nhân vật (mục 3)

    private void OnCharacterButtonClicked(int index)
    {
        Debug.Log($"OnCharacterButtonClicked called, index={index}");
        if (_characterButtons == null || index < 0 || index >= _characterButtons.Length)
        {
            return;
        }

        bool wasStatsVisible = _canvasStats != null && _canvasStats.activeSelf;

        _selectedCharacterIndex = index;

        MovePanelSelectTo(_characterButtons[index].transform as RectTransform);

        if (_canvasStats != null)
        {
            _canvasStats.SetActive(true);
        }

        if (_panelHide != null)
        {
            _panelHide.SetActive(false);
        }

        UpdateCharacterName(index);

        // Tab Story mặc định chỉ khi mở Stats lần đầu; đổi G khác giữ nguyên tab hiện tại.
        if (!wasStatsVisible && _statsTabGroup != null)
        {
            _statsTabGroup.SelectTab(0);
        }
    }

    private void OnStatsBackClicked()
    {
        ApplyDefaultState();
    }

    #endregion

    #region Luồng nâng cấp (mục 4)

    private void OnUpgradeClicked()
    {
        if (_canvasSelect != null)
        {
            _canvasSelect.SetActive(false);
        }

        if (_panelHide != null)
        {
            _panelHide.SetActive(false);
        }

        if (_panelShow != null)
        {
            _panelShow.SetActive(true);
        }

        if (_upgradeTabGroup != null)
        {
            _upgradeTabGroup.SelectTab(0);
        }
    }

    private void OnUpgradeBackClicked()
    {
        ApplyDefaultState();
    }

    #endregion

    #region Helpers

    private void WireCharacterButtons()
    {
        if (_characterButtons == null)
        {
            return;
        }

        for (int i = 0; i < _characterButtons.Length; i++)
        {
            if (_characterButtons[i] == null)
            {
                continue;
            }

            int index = i;
            _characterButtons[i].onClick.AddListener(() => OnCharacterButtonClicked(index));
        }
    }

    private void WireBackAndUpgradeButtons()
    {
        if (_statsBackButton != null)
        {
            _statsBackButton.onClick.AddListener(OnStatsBackClicked);
        }

        if (_upgradeButton != null)
        {
            _upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }

        if (_upgradeBackButton != null)
        {
            _upgradeBackButton.onClick.AddListener(OnUpgradeBackClicked);
        }
    }

    /// <summary>Di chuyển khung PanelSelect bao quanh nút G vừa chọn.</summary>
    private void MovePanelSelectTo(RectTransform target)
    {
        if (_chooseSelect == null || target == null)
        {
            return;
        }

        _chooseSelect.gameObject.SetActive(true);

        _chooseSelect.anchorMin = new Vector2(0.5f, 0.5f);
        _chooseSelect.anchorMax = new Vector2(0.5f, 0.5f);
        _chooseSelect.pivot = new Vector2(0.5f, 0.5f);
        _chooseSelect.position = target.position;

        RectTransform parentRect = _chooseSelect.parent as RectTransform;
        if (parentRect != null)
        {
            Vector2 targetSize = target.rect.size;
            Vector3 scaleRatio = new Vector3(
                target.lossyScale.x / parentRect.lossyScale.x,
                target.lossyScale.y / parentRect.lossyScale.y,
                1f);
            _chooseSelect.sizeDelta = new Vector2(  
                targetSize.x * scaleRatio.x,
                targetSize.y * scaleRatio.y);
        }
    }

    private void UpdateCharacterName(int index)
    {
        if (_nameGText == null)
        {
            return;
        }

        _nameGText.text = GetCharacterName(index);
        SetNameGVisible(true);
    }

    private string GetCharacterName(int index)
    {
        if (_characterNames != null
            && index >= 0
            && index < _characterNames.Length
            && !string.IsNullOrEmpty(_characterNames[index]))
        {
            return _characterNames[index];
        }

        return $"G{index + 1}";
    }

    private void SetNameGVisible(bool visible)
    {
        if (_nameGText != null)
        {
            _nameGText.gameObject.SetActive(visible);
        }
    }

    #endregion
}

using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tab dạng radio: bật 1 content, tắt các content còn lại.
/// Indicator (nếu có) đồng bộ theo tab đang chọn.
/// </summary>
public class TabGroup : MonoBehaviour
{
    [Serializable]
    public class TabItem
    {
        [Tooltip("Nút chuyển tab (để trống nếu chỉ gọi SelectTab bằng code)")]
        public Button tabButton;

        [Tooltip("Panel nội dung hiển thị khi tab này được chọn")]
        public GameObject contentPanel;

        [Tooltip("Icon đánh dấu tab đang chọn — để trống nếu không dùng")]
        public GameObject indicator;
    }

    [SerializeField] private TabItem[] _tabs;

    private int _currentIndex = -1;

    private void Awake()
    {
        if (_tabs == null)
        {
            return;
        }

        for (int i = 0; i < _tabs.Length; i++)
        {
            int index = i;
            if (_tabs[i].tabButton != null)
            {
                _tabs[i].tabButton.onClick.AddListener(() => SelectTab(index));
            }
        }
    }

    /// <summary>Chọn tab theo index (0-based).</summary>
    public void SelectTab(int index)
    {
        if (_tabs == null || _tabs.Length == 0)
        {
            return;
        }

        index = Mathf.Clamp(index, 0, _tabs.Length - 1);
        if (_currentIndex == index)
        {
            return;
        }

        _currentIndex = index;

        for (int i = 0; i < _tabs.Length; i++)
        {
            bool isSelected = i == index;

            if (_tabs[i].contentPanel != null)
            {
                _tabs[i].contentPanel.SetActive(isSelected);
            }

            if (_tabs[i].indicator != null)
            {
                _tabs[i].indicator.SetActive(isSelected);
            }
        }
    }
}

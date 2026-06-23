using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoadGameMenu : MonoBehaviour
{
    [System.Serializable]
    public struct SaveSlotUI
    {
        public Button mainButton;         // Thanh nút to đùng để bấm chọn slot
        public TextMeshProUGUI btnText;   // Chữ hiển thị ngày giờ / "NEW GAME"
        public GameObject deleteButton;   // Icon nút thùng rác nhỏ đi kèm
    }

    public SaveSlotUI[] slots = new SaveSlotUI[4]; // Quản lý chuẩn 4 slot

    private void OnEnable()
    {
        RefreshMenuUI(); // Cứ mở bảng Load Game lên là tự động cập nhật ngày giờ mới nhất
    }

    public void RefreshMenuUI()
    {
        for (int i = 0; i < 4; i++)
        {
            int slotNumber = i + 1;
            GameSaveData data = SaveManager.LoadGame(slotNumber);

            if (data.hasData)
            {
                slots[i].btnText.text = data.saveTimeString;
                slots[i].deleteButton.SetActive(true);
            }
            else
            {
                slots[i].btnText.text = "NEW GAME";
                slots[i].deleteButton.SetActive(false);
            }

            slots[i].mainButton.onClick.RemoveAllListeners();
            slots[i].mainButton.onClick.AddListener(() => OnSelectSlot(slotNumber));
        }
    }

    public void OnSelectSlot(int slotIndex)
    {
        // Ghi nhớ slot được chọn để lúc vào gameplay hệ thống biết đường tự động lưu đè lên đúng slot này
        SaveManager.SelectedSlotIndex = slotIndex;
        GameSaveData data = SaveManager.LoadGame(slotIndex);

        if (data.hasData)
        {
            Debug.Log($"🎮 [LOAD GAME] Vào thẳng màn {data.currentStageIndex} của Slot {slotIndex}");
            SceneManager.LoadScene(2);
        }
        else
        {
            Debug.Log($"⚔️ [NEW GAME] Tạo màn chơi mới tinh tại Slot {slotIndex}");

            // Khởi tạo cục data trống để tự động lưu phát đầu tiên làm mốc
            GameSaveData newData = new GameSaveData();
            newData.playerLevel = 1;
            newData.currentStageIndex = 1;
            newData.playerMaxHp = 100;
            newData.playerCurrentHp = 100;
            newData.playerMana = 0;
            newData.playerMaxMana = 50;
            newData.playerShield = 0;
            newData.maxShield = 20;
            newData.playerAttack = 10;
            newData.playerSp = 15;
            // ... gán các chỉ số mặc định của nhân vật cấp 1 ...

            SaveManager.SaveGame(slotIndex, newData);
            RefreshMenuUI(); // Vẽ lại giao diện sang định dạng ngày giờ luôn
            SceneManager.LoadScene(2);
        }
    }

    public void OnClickDeleteSlot(int slotIndex)
    {
        SaveManager.DeleteSlot(slotIndex);
        RefreshMenuUI(); // Xóa xong reset giao diện nút về chữ "NEW GAME"
    }
}

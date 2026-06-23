using UnityEngine;

public static class SaveManager
{
    // Biến tĩnh dùng chung để lưu lại Slot mà người chơi vừa chọn ở Menu chính
    public static int SelectedSlotIndex = 1;

    // Hàm thực hiện ghi file dữ liệu
    public static void SaveGame(int slotIndex, GameSaveData data)
    {
        data.saveTimeString = System.DateTime.Now.ToString("MM/dd/yyyy HH:mm");
        data.hasData = true;

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("GameSave_Slot_" + slotIndex, json);
        PlayerPrefs.Save();

        Debug.Log($"💾 [SAVE SUCCESS] Đã lưu dữ liệu vào Slot {slotIndex} lúc {data.saveTimeString}");
    }

    // Hàm thực hiện đọc file dữ liệu
    public static GameSaveData LoadGame(int slotIndex)
    {
        string key = "GameSave_Slot_" + slotIndex;
        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key);
            return JsonUtility.FromJson<GameSaveData>(json);
        }
        return new GameSaveData(); // Trả về data rỗng nếu slot chưa có gì
    }

    // Hàm xóa file dữ liệu
    public static void DeleteSlot(int slotIndex)
    {
        PlayerPrefs.DeleteKey("GameSave_Slot_" + slotIndex);
        PlayerPrefs.Save();
        Debug.Log($"🗑️ [DELETE SUCCESS] Đã xóa dữ liệu Slot {slotIndex}");
    }
}
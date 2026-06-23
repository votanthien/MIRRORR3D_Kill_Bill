using UnityEngine;

public class GameplaySaveBridge : MonoBehaviour
{
    public static GameplaySaveBridge Instance { get; private set; }

    [Header("References")]
    public Match3.Level levelComponent;    // Tham chiếu đến file điều khiển trận đấu của bạn
    public PlayerStat playerStatComponent; // 🔥 Đã mở kết nối tới file PlayerStat của bạn

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 🔥 KHI VỪA VÀO SCENE GAMEPLAY: Đọc file save lên và gán trả chỉ số cho PlayerStat
        LoadAndApplyData();
    }

    // ==========================================
    // 💾 HÀM TỰ ĐỘNG LƯU (AUTO SAVE)
    // ==========================================
    public void TriggerAutoSave()
    {
        if (levelComponent == null || playerStatComponent == null)
        {
            Debug.LogError("❌ [SAVE BRIDGE] Thiếu cấu hình Level hoặc PlayerStat trong Inspector!");
            return;
        }

        // 1. Tạo gói data rỗng
        GameSaveData currentData = new GameSaveData();

        // 2. Gom dữ liệu trực tiếp từ file PlayerStat.cs của bạn mang đi lưu
        currentData.playerCurrentHp = playerStatComponent.playerCurrentHp;
        currentData.playerMaxHp = playerStatComponent.playerMaxHp;
        currentData.playerAttack = playerStatComponent.playerAttack;

        // Gom thêm cấp độ từ file Level của bạn (ví dụ: biến playerLevel bạn tạo ở các bước trước)
        // currentData.playerLevel = levelComponent.playerLevel;
        // currentData.currentStageIndex = ... (Màn chơi hiện tại nếu có)

        // Tạm thời gán mẫu để test nếu bạn chưa hoàn thiện biến màn chơi
        currentData.playerLevel = 1;
        currentData.currentStageIndex = 1;

        // 3. Tiến hành lưu vào Slot đang chơi thông qua SaveManager tĩnh
        SaveManager.SaveGame(SaveManager.SelectedSlotIndex, currentData);
        Debug.Log("✨ [AUTO SAVE SUCCESS] Đã lưu thông số máu, công từ PlayerStat thành công!");
    }

    // ==========================================
    // 📖 HÀM ĐỌC VÀ ĐỒNG BỘ DỮ LIỆU KHI VÀO GAME
    // ==========================================
    private void LoadAndApplyData()
    {
        // Đọc dữ liệu từ Slot mà người chơi đã click chọn ở Menu ngoài kia
        GameSaveData loadedData = SaveManager.LoadGame(SaveManager.SelectedSlotIndex);

        if (loadedData.hasData && playerStatComponent != null)
        {
            // Đổ ngược dữ liệu đã lưu vào lại các biến trong file PlayerStat.cs của bạn
            playerStatComponent.playerCurrentHp = loadedData.playerCurrentHp;
            playerStatComponent.playerMaxHp = loadedData.playerMaxHp;
            playerStatComponent.playerAttack = loadedData.playerAttack;

            // Nếu bạn có biến lưu level bên file Level.cs thì gán vào đây
            // levelComponent.playerLevel = loadedData.playerLevel;

            Debug.Log($"⚔️ [LOAD SUCCESS] Đã nạp thành công dữ liệu nhân vật: HP ({playerStatComponent.playerCurrentHp}/{playerStatComponent.playerMaxHp}), ATK ({playerStatComponent.playerAttack}) vào trận đấu!");
        }
    }
}
using Match3;
using UnityEngine;

public class skill : MonoBehaviour
{
    public Level levelRef;

    // --- BIẾN THÊM MỚI QUẢN LÝ COOLDOWN ---
    private int poisonKnivesCooldown = 0; // Số lượt còn lại phải chờ (0 là dùng được luôn)
    private const int MAX_COOLDOWN = 3;   // Cooldown gốc là 3 lượt

    public PlayerStat PlayerStat;

    private int lifeStealCooldown = 0;
    private const int MAX_LIFESTEAL_CD = 3; // Cooldown 3 lượt

    private int curseCooldown = 0;
    private const int MAX_CURSE_CD = 3;

    private int frozenCooldown = 0;
    private const int MAX_FROZEN_CD = 3;

    private int defenseCooldown = 0;
    private const int MAX_DEFENSE_CD = 3;
    void Update()
    {
        // Khi nhấn phím K
        if (Input.GetKeyDown(KeyCode.K)&& PlayerStat.playerMana>=20)
        {
            // Kiểm tra xem skill có đang bị hồi chiêu hay không
            if (poisonKnivesCooldown > 0)
            {
                Debug.LogWarning($"⏳ Kỹ năng đang hồi chiêu! Cần chờ thêm {poisonKnivesCooldown} lượt nữa.");
                return; // Chặn lại, không cho chạy tiếp xuống hàm poision_knives()
            }

            poision_knives();
            PlayerStat.playerMana-=20;
        }
        if (Input.GetKeyDown(KeyCode.L) && PlayerStat.playerMana >= 20)
        {
            if (lifeStealCooldown > 0)
            {
                Debug.LogWarning($"⏳ Hút Máu đang hồi chiêu! Chờ {lifeStealCooldown} lượt.");
                return;
            }
            life_steal();
            PlayerStat.playerMana -= 20;
        }
        if (Input.GetKeyDown(KeyCode.I) && PlayerStat.playerMana >= 20)
        {
            if (curseCooldown > 0)
            {
                Debug.LogWarning($"⏳ Lời Nguyền đang hồi chiêu! Chờ {curseCooldown} lượt.");
                return;
            }
            curse_of_weak();
            PlayerStat.playerMana -= 20;
        }
        if (Input.GetKeyDown(KeyCode.U) && PlayerStat.playerMana >= 30)
        {
            if (frozenCooldown > 0)
            {
                Debug.LogWarning($"⏳ Đóng Băng đang hồi chiêu! Chờ {frozenCooldown} lượt.");
                return;
            }
            frozen_skill();
            PlayerStat.playerMana -= 30;
        }
        if (Input.GetKeyDown(KeyCode.O) && PlayerStat.playerMana >= 20)
        {
            if (defenseCooldown > 0)
            {
                Debug.LogWarning($"⏳ Thế Thủ đang hồi chiêu! Chờ {defenseCooldown} lượt.");
                return;
            }
            defense_stance();
            PlayerStat.playerMana -= 20;
        }    
    }

    public void poision_knives()
    {
        if (levelRef != null)
        {
            levelRef.rpgEffectsEnabled = true;
            levelRef.ActivatePoisonKnives();

            // 🎯 KÍCH HOẠT COOLDOWN: Đặt số lượt chờ thành 3 ngay sau khi dùng thành công
            poisonKnivesCooldown = MAX_COOLDOWN;
            Debug.Log($"🗡️ Skill đã dùng! Bắt đầu hồi chiêu: {poisonKnivesCooldown} lượt.");
        }
        else
        {
            Debug.LogError("Chưa kéo thả Level vào ô levelRef trong script Skill!");
        }
    }
    public void life_steal()
    {
        if (levelRef != null)
        {
            levelRef.rpgEffectsEnabled = true;

            // Gọi hàm xử lý hút máu bên Level.cs
            levelRef.ActivateLifeSteal();

            // Kích hoạt Cooldown 3 lượt cho Skill 2
            lifeStealCooldown = MAX_LIFESTEAL_CD;
            Debug.Log($"🩸 Skill Hút Máu đã dùng! Cooldown: {lifeStealCooldown} lượt.");
        }
        else
        {
            Debug.LogError("Chưa kéo thả Level vào ô levelRef trong script Skill!");
        }
    }
    public void curse_of_weak()
    {
        if (levelRef != null)
        {
            levelRef.rpgEffectsEnabled = true;

            levelRef.ActivateCurseOfWeak();

            curseCooldown = MAX_CURSE_CD;
            Debug.Log($"🔮 Skill Lời Nguyền đã dùng! Cooldown: {curseCooldown} lượt.");
        }
    }
    public void frozen_skill()
    {
        if (levelRef != null)
        {
            levelRef.rpgEffectsEnabled = true;

            levelRef.ActivateFrozen();

            frozenCooldown = MAX_FROZEN_CD;
            Debug.Log($"❄️ Skill Đóng Băng đã dùng! Cooldown: {frozenCooldown} lượt.");
        }
    }
    public void defense_stance()
    {
        if (levelRef != null)
        {
            levelRef.rpgEffectsEnabled = true;

            levelRef.ActivateDefenseStance();

            defenseCooldown = MAX_DEFENSE_CD;
            Debug.Log($"🛡️ Skill Thế Thủ đã dùng! Cooldown: {defenseCooldown} lượt.");
        }
    }
    // 🎯 HÀM THÊM MỚI: Sẽ được gọi từ Level.cs khi người chơi kết thúc 1 lượt đi để giảm hồi chiêu
    public void ReduceCooldown()
    {
        if (poisonKnivesCooldown > 0)
        {
            poisonKnivesCooldown--;
            if (poisonKnivesCooldown == 0)
            {
                Debug.Log("✨ [SKILL READY] Kỹ năng Dao Độc đã hồi chiêu xong! Sẵn sàng sử dụng (Phím K).");
            }
            else
            {
                Debug.Log($"⏳ Cooldown kỹ năng Dao Độc giảm! Còn lại: {poisonKnivesCooldown} lượt.");
            }
        }
        if (lifeStealCooldown > 0)
        {
            lifeStealCooldown--;
            if (lifeStealCooldown == 0)
            {
                Debug.Log("✨ [SKILL READY] Kỹ năng Hút Máu đã hồi chiêu xong! Sẵn sàng sử dụng (Phím L).");
            }
            else
            {
                Debug.Log($"⏳ Cooldown Hút Máu giảm! Còn lại: {lifeStealCooldown} lượt.");
            }
        }
        if (curseCooldown > 0)
        {
            curseCooldown--;
            if (curseCooldown == 0) Debug.Log("✨ Lời Nguyền đã hồi xong!");
        }
        if (frozenCooldown > 0)
        {
            frozenCooldown--;
            if (frozenCooldown == 0) Debug.Log("✨ Đóng Băng đã hồi xong!");
        }
        if (defenseCooldown > 0)
        {
            defenseCooldown--;
            if (defenseCooldown == 0) Debug.Log("✨ Thế Thủ đã hồi xong!");
        }
    }
}

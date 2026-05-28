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

    private int heavenShieldCooldown = 0;
    private const int MAX_HEAVEN_CD = 3;

    private int wrathCooldown = 0;
    private const int MAX_WRATH_CD = 3;

    private int seeTheSoulCooldown = 0;
    private const int MAX_SOUL_CD = 3;

    private int meditationCooldown = 0;
    private const int MAX_MEDITATION_CD = 3;

    private int lightningCooldown = 0;
    private const int MAX_LIGHTNING_CD = 3;
    private bool isWaitingForLightningTarget = false;

    private int hammerCooldown = 0;
    private const int MAX_HAMMER_CD = 3;
    private bool isWaitingForHammerTarget = false;

    private int phoenixCooldown = 0;
    private const int MAX_PHOENIX_CD = 3;

    private int healingLeafCooldown = 0;
    private const int MAX_HEAL_LEAF_CD = 3;

    private int treeOfLifeCooldown = 0;
    private const int MAX_TREE_CD = 5;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H) && PlayerStat.playerMana >= 20)
        {
            if (hammerCooldown > 0)
            {
                Debug.LogWarning($"⏳ Giant Hammer đang hồi chiêu! Chờ {hammerCooldown} lượt.");
                return;
            }

            isWaitingForHammerTarget = true;
            PlayerStat.playerMana -= 20;
            Debug.Log("🔨 [SKILL] Giant Hammer đã sẵn sàng! Hãy CLICK CHUỘT PHẢI vào viên kẹo muốn đập vỡ.");
        }

        // 🎯 XỬ LÝ CLICK CHUỘT PHẢI KHI ĐANG NHẤC BÚA
        if (isWaitingForHammerTarget && Input.GetMouseButtonDown(1)) // 1 là Chuột Phải
        {
            ExecuteGiantHammer();
        }

        if (Input.GetKeyDown(KeyCode.V) && PlayerStat.playerMana >= 50)
        {
            if (lightningCooldown > 0)
            {
                Debug.LogWarning($"⏳ Lightning Creation đang hồi! Chờ {lightningCooldown} lượt.");
                return;
            }

            isWaitingForLightningTarget = true;
            PlayerStat.playerMana -= 50;
            Debug.Log("⚡ [SKILL] Lightning Creation đã bật! Hãy CLICK CHUỘT PHẢI vào một ô trên bàn cờ để tạo kẹo Cầu Vồng.");
        }

        // 🎯 XỬ LÝ CLICK CHUỘT PHẢI KHI ĐANG TRONG TRẠNG THÁI CHỜ
        if (isWaitingForLightningTarget && Input.GetMouseButtonDown(1)) // 1 là Chuột Phải
        {
            ExecuteLightningCreation();
            
        }
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
        if (Input.GetKeyDown(KeyCode.P) && PlayerStat.playerMana >= 20)
        {
            if (heavenShieldCooldown > 0)
            {
                Debug.LogWarning($"⏳ Khiên Thiên Đường đang hồi chiêu! Chờ {heavenShieldCooldown} lượt.");
                return;
            }
            heaven_shield();
            PlayerStat.playerMana -= 20;
        }
        if (Input.GetKeyDown(KeyCode.M) && PlayerStat.playerMana >= 30)
        {
            if (wrathCooldown > 0)
            {
                Debug.LogWarning($"⏳ Cơn Thịnh Nộ đang hồi chiêu! Chờ {wrathCooldown} lượt.");
                return;
            }
            the_wrath();
            PlayerStat.playerMana -= 30;
        }
        if (Input.GetKeyDown(KeyCode.N) && PlayerStat.playerMana >= 30)
        {
            if (seeTheSoulCooldown > 0 )
            {
                Debug.LogWarning($"⏳ See The Soul đang hồi chiêu! Chờ {seeTheSoulCooldown} lượt.");
                return;
            }
            see_the_soul();
            PlayerStat.playerMana -= 30;
        }
        if (Input.GetKeyDown(KeyCode.B) && PlayerStat.playerMana >= 30)
        {
            if (meditationCooldown > 0)
            {
                Debug.LogWarning($"⏳ Mind of Meditation đang hồi chiêu! Chờ {meditationCooldown} lượt.");
                return;
            }
            mind_of_meditation();
            PlayerStat.playerMana -= 30;
        }
        if (Input.GetKeyDown(KeyCode.G) && PlayerStat.playerMana >= 50)
        {
            if (phoenixCooldown > 0)
            {
                Debug.LogWarning($"⏳ Phoenix Wings đang hồi chiêu! Chờ {phoenixCooldown} lượt.");
                return;
            }
            phoenix_wings();
            PlayerStat.playerMana -=50;
        }
        if (Input.GetKeyDown(KeyCode.J) && PlayerStat.playerMana >= 50)
        {
            if (healingLeafCooldown > 0)
            {
                Debug.LogWarning($"⏳ Healing Leaf đang hồi chiêu! Chờ {healingLeafCooldown} lượt.");
                return;
            }
            healing_leaf();
            PlayerStat.playerMana -= 50;
        }
        if (Input.GetKeyDown(KeyCode.T) && PlayerStat.playerMana >= 50)
        {
            if (treeOfLifeCooldown > 0)
            {
                Debug.LogWarning($"⏳ Tree of Life đang hồi chiêu! Chờ {treeOfLifeCooldown} lượt.");
                return;
            }
            tree_of_life();
            PlayerStat.playerMana -= 50;
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
    public void heaven_shield()
    {
        if (levelRef != null)
        {
            levelRef.rpgEffectsEnabled = true;

            levelRef.ActivateHeavenShield();

            heavenShieldCooldown = MAX_HEAVEN_CD;
            Debug.Log($"😇 Skill Khiên Thiên Đường đã dùng! Cooldown: {heavenShieldCooldown} lượt.");
        }
    }
    public void the_wrath()
    {
        if (levelRef != null)
        {
            levelRef.rpgEffectsEnabled = true;

            levelRef.ActivateTheWrath();

            wrathCooldown = MAX_WRATH_CD;
            Debug.Log($"🔥 Skill The Wrath đã dùng! Cooldown: {wrathCooldown} lượt.");
        }
    }
    public void see_the_soul()
    {
        if (levelRef != null)
        {
            levelRef.rpgEffectsEnabled = true;

            levelRef.ActivateSeeTheSoul();

            seeTheSoulCooldown = MAX_SOUL_CD;
            Debug.Log($"👁️ Skill See The Soul đã dùng! Cooldown: {seeTheSoulCooldown} lượt.");
        }
    }
    public void mind_of_meditation()
    {
        if (levelRef != null)
        {
            levelRef.rpgEffectsEnabled = true;

            levelRef.ActivateMindOfMeditation();

            meditationCooldown = MAX_MEDITATION_CD;
            Debug.Log($"🧘‍♂️ Skill Mind of Meditation đã dùng! Cooldown: {meditationCooldown} lượt.");
        }
    }

    private void ExecuteLightningCreation()
    {
        if (levelRef == null || levelRef.gameGrid == null) return;

        // Bắn một tia Raycast từ Camera đến vị trí con trỏ chuột trong không gian 2D
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

        // Nếu chuột click trúng vào một Collider
        if (hit.collider != null)
        {
            // Kiểm tra xem Collider đó có chứa linh hồn của viên kẹo (GamePiece) không
            Match3.GamePiece hitPiece = hit.collider.GetComponent<Match3.GamePiece>();

            if (hitPiece != null)
            {
                levelRef.rpgEffectsEnabled = true;

                // Gọi hàm công năng vừa thêm ở Bước 1 để biến đổi kẹo
                levelRef.gameGrid.ChangePieceToRainbow(hitPiece.X, hitPiece.Y);

                // Đặt Cooldown và tắt trạng thái chờ
                lightningCooldown = MAX_LIGHTNING_CD;
                isWaitingForLightningTarget = false;

                Debug.Log($"⚡ Biến đổi thành công ô ({hitPiece.X}, {hitPiece.Y}) thành kẹo Cầu Vồng! Cooldown: {lightningCooldown} lượt.");
            }
        }
    }
    private void ExecuteGiantHammer()
    {
        if (levelRef == null || levelRef.gameGrid == null) return;

        // Bắn tia Raycast tọa độ chuột 2D
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

        if (hit.collider != null)
        {
            Match3.GamePiece hitPiece = hit.collider.GetComponent<Match3.GamePiece>();

            if (hitPiece != null)
            {
                levelRef.rpgEffectsEnabled = true;

                // Gọi hàm đập kẹo vừa viết ở Bước 1
                levelRef.gameGrid.HammerDestroyPiece(hitPiece.X, hitPiece.Y);

                // Áp cooldown và hạ búa xuống
                hammerCooldown = MAX_HAMMER_CD;
                isWaitingForHammerTarget = false;

                Debug.Log($"🔨 Bùm! Giant Hammer đã đập vỡ ô ({hitPiece.X}, {hitPiece.Y})! Cooldown: {hammerCooldown} lượt.");
            }
        }
    }
    public void phoenix_wings()
    {
        if (levelRef != null)
        {
            levelRef.rpgEffectsEnabled = true;

            levelRef.ActivatePhoenixWings();

            phoenixCooldown = MAX_PHOENIX_CD;
            Debug.Log($"🔥 Skill Phoenix Wings đã dùng! Cooldown: {phoenixCooldown} lượt.");
        }
    }
    public void healing_leaf()
    {
        if (levelRef != null)
        {
            levelRef.rpgEffectsEnabled = true;

            // Gọi hàm buff từ Level sang
            levelRef.ActivateHealingLeaf();

            // Áp thời gian hồi chiêu
            healingLeafCooldown = MAX_HEAL_LEAF_CD;
            Debug.Log($"🍃 Kỹ năng Healing Leaf đã sử dụng! Cooldown: {healingLeafCooldown} lượt.");
        }
    }
    public void tree_of_life()
    {
        if (levelRef != null)
        {
            levelRef.rpgEffectsEnabled = true;

            // Gọi hàm xử lý hồi máu tức thì từ Level sang
            levelRef.ActivateTreeOfLife();

            // Áp thời gian hồi chiêu
            treeOfLifeCooldown = MAX_TREE_CD;
            Debug.Log($"🌳 Kỹ năng Tree of Life đã sử dụng! Cooldown: {treeOfLifeCooldown} lượt.");
        }
    }
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
        if (heavenShieldCooldown > 0)
        {
            heavenShieldCooldown--;
            if (heavenShieldCooldown == 0) Debug.Log("✨ Khiên Thiên Đường đã hồi xong!");
        }
        if (wrathCooldown > 0)
        {
            wrathCooldown--;
            if (wrathCooldown == 0) Debug.Log("✨ Cơn Thịnh Nộ đã hồi xong!");
        }
        if (seeTheSoulCooldown > 0)
        {
            seeTheSoulCooldown--;
            if (seeTheSoulCooldown == 0) Debug.Log("✨ See The Soul đã hồi xong!");
        }
        if (meditationCooldown > 0)
        {
            meditationCooldown--;
            if (meditationCooldown == 0) Debug.Log("✨ Mind of Meditation đã hồi xong!");
        }
        if (lightningCooldown > 0)
        {
            lightningCooldown--;
            if (lightningCooldown == 0) Debug.Log("✨ Lightning Creation đã hồi xong!");
        }
        if (hammerCooldown > 0)
        {
            hammerCooldown--;
            if (hammerCooldown == 0) Debug.Log("✨ Giant Hammer đã hồi xong!");
        }
        if (phoenixCooldown > 0)
        {
            phoenixCooldown--;
            if (phoenixCooldown == 0) Debug.Log("✨ Phoenix Wings đã hồi xong!");
        }
        if (healingLeafCooldown > 0)
        {
            healingLeafCooldown--;
            if (healingLeafCooldown == 0) Debug.Log("✨ Healing Leaf đã hồi xong!");
        }
        if (treeOfLifeCooldown > 0)
        {
            treeOfLifeCooldown--;
            if (treeOfLifeCooldown == 0) Debug.Log("✨ Tree of Life đã hồi xong!");
        }
    }
}

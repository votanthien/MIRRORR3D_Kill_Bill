using System.Collections;
using UnityEngine;

namespace Match3
{
    public class Level : MonoBehaviour
    {
        public GameGrid gameGrid;
        public Hud hud;
        private int poisonTurnsRemaining = 0; // Số lượt dính độc còn lại
        private float poisonPercent = 0.05f;  // 5% máu mỗi lượt
        public skill playerSkillRef;
        private int curseTurnsRemaining = 0;
        private int frozenTurnsRemaining = 0; // Số lượt Đóng Băng còn lại trên quái
        [Header("Score Targets")]
        public int score1Star;
        public int score2Star;
        public int score3Star;
      
        public PlayerStat statplayer;

       public enemy enemy;

        protected LevelType type;
        protected int currentScore;
        private bool _didWin;
        private bool _isGameOver;
        private int _currentFrameShieldMultiplier = 0; // Dùng để gom cụm kẹo Khiên nổ cùng lúc
        private int heavenShieldTurnsRemaining = 0; // Số lượt buff Khiên Thiên Đường còn lại
        private int wrathTurnsRemaining = 0; // Số lượt Cơn Thịnh Nộ còn lại
        private int seeTheSoulTurnsRemaining = 0; // Số lượt buff x3 Mana còn lại
        private int meditationTurnsRemaining = 0; // Số lượt buff x3 Hồi Máu còn lại
        private int phoenixWingsTurnsRemaining = 0; // Số lượt buff bảo hộ hồi sinh còn lại

        private int healingLeafTurnsRemaining = 0; // Số lượt hồi máu còn lại
        private float healingLeafPercent = 0.07f;  // 7% máu mỗi lượt

        public Animator enemyAnimator;
        [Header("Lightning Effect")]
        public GameObject lightningEffectPrefab; // Prefab tia sét
        public Transform lightningSpawnPoint;    // Vị trí xuất hiện effect

        private int _currentFrameSwordCount = 0; private int _currentFrameSpellCount = 0; // Đếm số lượng kẹo Spell nổ cùng lúc trong khung hình

        private int playerLevel = 1;        // Cấp độ hiện tại của người chơi
        private int piecesEatenCount = 0;   // Bộ đếm số lượng viên kẹo đã ăn trong cấp này
        private const int PIECES_TO_LEVEL_UP = 20; // Mốc yêu cầu để lên cấp (20 cục)
        private void Start()
        {
            hud.SetScore(currentScore);

            // Khởi tạo máu ban đầu
            statplayer.playerCurrentHp = statplayer.playerMaxHp;
            enemy.enemyCurrentHp = enemy.enemyMaxHp;
            _isGameOver = false;

          
                // Đọc dữ liệu từ Slot mà người chơi vừa ấn chọn từ Menu ngoài kia
                GameSaveData loadedData = SaveManager.LoadGame(SaveManager.SelectedSlotIndex);

                if (loadedData.hasData)
                {
                    // Áp dụng các thông số đã lưu vào nhân vật hiện tại
                    this.playerLevel = loadedData.playerLevel;
                    this.statplayer.playerCurrentHp = loadedData.playerCurrentHp;
                    this.statplayer.playerAttack = loadedData.playerAttack;
                    Debug.Log($"⚔️ Đã đồng bộ dữ liệu nhân vật Level {this.playerLevel} vào trận đấu!");
                }
            
        }
        private void Update()
        {
            if (heavenShieldTurnsRemaining > 0)
            {
                statplayer.maxShield = 60;
            }
            else
            {
                statplayer.maxShield = 20;
            }
        }
        ///hieu ung tia sét
       private void SpawnLightningEffect()
        {
            if (lightningEffectPrefab != null && lightningSpawnPoint != null)
            {
                GameObject fx = Instantiate(
                    lightningEffectPrefab,
                    lightningSpawnPoint.position,
                    lightningSpawnPoint.rotation
                );

                ParticleSystem ps = fx.GetComponent<ParticleSystem>();

                if (ps != null)
                {
                    ps.Play();
                    Destroy(fx, ps.main.duration + ps.main.startLifetime.constantMax);
                }
                else
                {
                    //Destroy(fx, 3f);
                }
            }
        }

        public LevelType Type => type;
        // 1. Thêm biến này vào cùng cụm nhóm trạng thái (ở gần các biến _isGameOver)
        public bool rpgEffectsEnabled = false;

        // 2. Sửa lại hàm OnPieceCleared một chút ở đoạn giữa
        public virtual void OnPieceCleared(GamePiece piece)
        {
            if (_isGameOver) return;

            currentScore += piece.score;
            hud.SetScore(currentScore);
            piecesEatenCount++; // Cứ ăn 1 cục (bất kỳ loại nào) thì cộng 1 điểm

            if (piecesEatenCount >= PIECES_TO_LEVEL_UP)
            {
                playerLevel++;          // Tăng cấp độ lên 1
                piecesEatenCount = 0;   // Reset bộ đếm về 0 để tính cho level tiếp theo

                // In ra Debug.Log thông báo người chơi đã lên cấp theo yêu cầu
                Debug.Log($"✨ [LEVEL UP] Chúc mừng! Bạn đã ăn đủ 20 cục kẹo và lên LEVEL {playerLevel}!");
                GameplaySaveBridge.Instance.TriggerAutoSave();
            }
            if (!rpgEffectsEnabled) return;

            // ==========================================
            // 1. TIA SÉT (Chỉ dành cho Rainbow)
            // ==========================================
            if (piece.Type == PieceType.Rainbow)
            {
                
                PlayEnemyHitAnimation(); // animation hit
                int lightningDamage = statplayer.playerSp * 2;
                SpawnLightningEffect(); // gọi hiệu ứng tia sét
                enemy.enemyCurrentHp -= lightningDamage;

                Debug.Log($"⚡ TIA SÉT đánh trúng! Gây {lightningDamage} Sát thương lên quái!");
                CheckBattleStatus();
                return;
            }

            // ==========================================
            // 2. CỤC RAW & HỆ SỐ NHÂN (Lv2, Lv3)
            // ==========================================
            int multiplier = 1;

            if (piece.Type == PieceType.RowClear || piece.Type == PieceType.ColumnClear)
            {
                multiplier = 2;
            }
            else if (piece.Type == PieceType.Lv3)
            {
                multiplier = 4;
            }

            if (piece.IsColored())
            {
                ColorType colorType = piece.ColorComponent.Color;

                // ---------------------------------------------------------------------
                // XỬ LÝ RIÊNG CHO KHIÊN: Gom cả hàng 3 lại để chỉ tính 1 lần
                // ---------------------------------------------------------------------
                if (colorType == ColorType.Shield)
                {
                    if (_currentFrameShieldMultiplier == 0)
                    {
                        // Kích hoạt bộ đếm thời gian chờ cả hàng nổ xong ở cuối khung hình
                        StartCoroutine(ApplyShieldAtEndOfFrame());
                    }
                    // Lấy hệ số nhân lớn nhất trong các viên vừa nổ (nếu có cục Lv2 hoặc Lv3 ở trong hàng)
                   
                        _currentFrameShieldMultiplier += multiplier;
                    
                }
                else if (colorType == ColorType.Sword)
                {
                    if (_currentFrameSwordCount == 0)
                    {
                        // Kích hoạt bộ xử lý tính sát thương Kiếm vào cuối khung hình
                        StartCoroutine(ApplySwordDamageAtEndOfFrame());
                    }

                    // Cộng dồn số lượng viên Kiếm bị phá hủy trong cụm combo này
                    // Nếu viên nổ là hàng Clear (Lv2) hoặc Lv3, chúng ta cộng tương ứng theo multiplier để tính thưởng viên
                    _currentFrameSwordCount += multiplier;
                }
                else if (colorType == ColorType.Spell)
                {
                    if (_currentFrameSpellCount == 0)
                    {
                        // Kích hoạt bộ xử lý tính điểm Spell vào cuối khung hình
                        StartCoroutine(ApplySpellPointsAtEndOfFrame());
                    }
                    _currentFrameSpellCount += multiplier;
                }
                else
                {
                    // Kiếm, HP, Mana, Spell vẫn giữ nguyên (nổ viên nào tính sát thương viên đó)
                    ApplyRawDamage(colorType, multiplier);
                }
            }

            CheckBattleStatus();
        }
        private IEnumerator ApplySpellPointsAtEndOfFrame()
        {
            // Đợi toàn bộ các viên kẹo Spell trong lượt combo này nổ sạch hoàn toàn
            yield return new WaitForEndOfFrame();

            if (_currentFrameSpellCount > 0)
            {
                // 1. Xác định hệ số nhân sát thương phép dựa trên số lượng kẹo Spell (Giống hệt Kiếm)
                int spellMultiplier = 3; // Ăn 3 viên = nhân 3

                if (_currentFrameSpellCount == 4)
                {
                    spellMultiplier = 6; // Ăn 4 viên = nhân 6
                }
                else if (_currentFrameSpellCount >= 5)
                {
                    spellMultiplier = 12; // Ăn 5 viên trở lên = nhân 12
                }
                else if (_currentFrameSpellCount < 3)
                {
                    spellMultiplier = _currentFrameSpellCount;
                }

                // 2. Tính toán lượng sát thương Phép (Spell Damage) dựa trên chỉ số của người chơi
                // Giả sử bạn dùng thuộc tính statplayer.playerAttack hoặc một biến phép riêng như statplayer.playerMagic
                int baseMagicAttack = statplayer.playerAttack;
                int totalSpellDamage = baseMagicAttack * spellMultiplier;

                // 3. Trừ thẳng vào máu quái vật (Enemy)
                enemy.enemyCurrentHp -= totalSpellDamage;

                // 4. Hiển thị Log thông báo sát thương Phép ra Console
                Debug.Log($"🔮 SPELL ATTACK: Phá hủy tổng cộng {_currentFrameSpellCount} viên Spell! " +
                          $"Hệ số phép: x{spellMultiplier}. Gây {totalSpellDamage} Sát thương Phép thẳng vào Quái! " +
                          $"(Máu quái còn lại: {enemy.enemyCurrentHp})");

                // 5. Reset bộ đếm và cập nhật trạng thái trận đấu (kiểm tra xem quái chết chưa)
                _currentFrameSpellCount = 0;
                CheckBattleStatus();
            }
        }
        private IEnumerator ApplySwordDamageAtEndOfFrame()
        {
            // Đợi toàn bộ các viên kẹo Kiếm trong lượt nổ này được dọn sạch hoàn toàn
            yield return new WaitForEndOfFrame();

            if (_currentFrameSwordCount > 0)
            {
                // 1. Xác định hệ số nhân sát thương dựa trên số lượng kiếm ăn được
                int swordMultiplier = 3; // Mặc định ăn 3 viên = 3 điểm (x3 playerAttack)

                if (_currentFrameSwordCount == 4)
                {
                    swordMultiplier = 6; // Ăn 4 viên = 6 điểm (x6 playerAttack)
                }
                else if (_currentFrameSwordCount >= 5)
                {
                    swordMultiplier = 12; // Ăn 5 viên trở lên = 12 điểm (x12 playerAttack)
                }
                // Trường hợp hiếm gặp nếu nổ dây chuyền rơi lẻ chỉ được 1-2 viên
                else if (_currentFrameSwordCount < 3)
                {
                    swordMultiplier = _currentFrameSwordCount;
                }

                // 2. Kiểm tra trạng thái buff cuồng nộ (The Wrath) nhân 3 sát thương kiếm
                int wrathMultiplier = (wrathTurnsRemaining > 0) ? 3 : 1;

                // 3. Tính toán tổng sát thương cuối cùng
                int finalDamage = statplayer.playerAttack * swordMultiplier * wrathMultiplier;
                PlayEnemyHitAnimation(); // animation hit
                enemy.enemyCurrentHp -= finalDamage;

                // In nhật ký log ra màn hình Console để dễ theo dõi
                string wrathMsg = wrathMultiplier > 1 ? " [🔥 X3 WRATH]" : "";
                Debug.Log($"🗡️ SWORD COMBO: Phá hủy tổng cộng {_currentFrameSwordCount} viên Kiếm! " +
                          $"Hệ số điểm: x{swordMultiplier}. Gây {finalDamage} Sát thương lên quái!{wrathMsg}");

                // 4. Reset bộ đếm và cập nhật trạng thái trận đấu
                _currentFrameSwordCount = 0;
                CheckBattleStatus();
            }
        }



        protected virtual void GameWin()
        {
            _isGameOver = true;
            gameGrid.GameOver();
            _didWin = true;
            StartCoroutine(WaitForGridFill());
        }

        protected virtual void GameLose()
        {
            _isGameOver = true;
            gameGrid.GameOver();
            _didWin = false;
            StartCoroutine(WaitForGridFill());
        }

        // HÀM NÀY ĐƯỢC GỌI MỖI KHI NGƯỜI CHƠI ĐI 1 NƯỚC HỢP LỆ
        public virtual void OnMove()
        {
            if (_isGameOver) return;

            // Kích hoạt Coroutine Chờ người chơi nổ kẹo xong -> Quái phản công
            StartCoroutine(EnemyTurnCoroutine());
            if (poisonTurnsRemaining > 0 && enemy.enemyCurrentHp > 0)
            {
                // Tính 5% dựa trên MÁU HIỆN TẠI hoặc MÁU TỐI ĐA (ở đây mình tính theo máu tối đa của quái nhé)
                int poisonDamage = Mathf.RoundToInt(enemy.enemyMaxHp * poisonPercent);

                enemy.enemyCurrentHp -= poisonDamage;
                poisonTurnsRemaining--;

                Debug.Log($"🤢 Độc phát tác! Quái mất {poisonDamage} máu. (Còn lại {poisonTurnsRemaining} lượt độc)");

                CheckBattleStatus();
            }
            if (playerSkillRef != null)
            {
                playerSkillRef.ReduceCooldown(); // Báo sang script skill để trừ đi 1 lượt hồi chiêu
            }
            if (curseTurnsRemaining > 0)
            {
                curseTurnsRemaining--;
                if (curseTurnsRemaining == 0)
                {
                    Debug.Log("✨ Quái vật đã thoát khỏi Lời Nguyền Yếu Ớt! Sức mạnh của nó trở lại bình thường.");
                }
                else
                {
                    Debug.Log($"💀 Lời Nguyền còn tác dụng trong {curseTurnsRemaining} lượt nữa.");
                }
            }
            if (frozenTurnsRemaining > 0)
            {
                frozenTurnsRemaining--;
                if (frozenTurnsRemaining == 0)
                {
                    Debug.Log("🔥 Băng tan! Quái vật đã thoát khỏi trạng thái Đóng Băng và có thể di chuyển lại.");
                }
                else
                {
                    Debug.Log($"❄️ Trạng thái Đóng Băng còn tác dụng trong {frozenTurnsRemaining} lượt nữa.");
                }
            }
            if (heavenShieldTurnsRemaining > 0)
            {
                heavenShieldTurnsRemaining--;
                if (heavenShieldTurnsRemaining == 0)
                {
                    Debug.Log("😇 Khiên Thiên Đường hết hiệu lực! Giới hạn lá chắn trở lại thành 2.");

                    // Nếu hết buff mà lượng khiên tích lũy vẫn đang > 2, ta ép nó về max mặc định là 2
                    if (statplayer.playerShield > 2)
                    {
                        statplayer.playerShield = 2;
                        Debug.Log($"🛡️ Lá chắn dư thừa bị vỡ, trả về mức tối đa mặc định: {statplayer.playerShield}/2");
                    }
                }
                else
                {
                    Debug.Log($"😇 Buff Khiên Thiên Đường còn tác dụng trong {heavenShieldTurnsRemaining} lượt. (Giới hạn hiện tại: {statplayer.maxShield})");
                }
            }
            if (wrathTurnsRemaining > 0)
            {
                wrathTurnsRemaining--;
                if (wrathTurnsRemaining == 0)
                {
                    Debug.Log("💨 Cơn Thịnh Nộ đã kết thúc! Sát thương trở về bình thường.");
                }
                else
                {
                    Debug.Log($"🔥 Buff Cơn Thịnh Nộ (Sát thương x3) còn tác dụng trong {wrathTurnsRemaining} lượt.");
                }
            }
            // --- ĐOẠN THÊM MỚI: GIẢM LƯỢT BUFF SEE THE SOUL ---
            if (seeTheSoulTurnsRemaining > 0)
            {
                seeTheSoulTurnsRemaining--;
                if (seeTheSoulTurnsRemaining == 0)
                {
                    Debug.Log("👁️ See The Soul đã kết thúc! Lượng Mana hồi phục trở về bình thường.");
                }
                else
                {
                    Debug.Log($"👁️ Buff See The Soul (Mana x3) còn tác dụng trong {seeTheSoulTurnsRemaining} lượt.");
                }
            }
            // --- ĐOẠN THÊM MỚI: GIẢM LƯỢT BUFF MIND OF MEDITATION ---
            if (meditationTurnsRemaining > 0)
            {
                meditationTurnsRemaining--;
                if (meditationTurnsRemaining == 0)
                {
                    Debug.Log("🧘‍♂️ Mind of Meditation đã kết thúc! Lượng HP hồi phục trở về bình thường.");
                }
                else
                {
                    Debug.Log($"🧘‍♂️ Buff Mind of Meditation (HP x3) còn tác dụng trong {meditationTurnsRemaining} lượt.");
                }
            }
            // --- ĐOẠN THÊM MỚI: GIẢM LƯỢT BUFF PHOENIX WINGS ---
            if (phoenixWingsTurnsRemaining > 0)
            {
                phoenixWingsTurnsRemaining--;
                if (phoenixWingsTurnsRemaining == 0)
                {
                    Debug.Log("🔥 Phoenix Wings đã hết hiệu lực! Bạn không còn được bảo hộ hồi sinh nữa.");
                }
                else
                {
                    Debug.Log($"🔥 Buff Phoenix Wings (Hồi sinh) còn tác dụng trong {phoenixWingsTurnsRemaining} lượt.");
                }
            }
            if (healingLeafTurnsRemaining > 0 && statplayer.playerCurrentHp > 0)
            {
                // Tính 7% lượng máu dựa trên MÁU TỐI ĐA của người chơi
                int healAmount = Mathf.RoundToInt(statplayer.playerMaxHp * healingLeafPercent);

                // Tiến hành hồi máu và khống chế không vượt quá máu tối đa
                statplayer.playerCurrentHp = Mathf.Min(statplayer.playerCurrentHp + healAmount, statplayer.playerMaxHp);
                healingLeafTurnsRemaining--;

                Debug.Log($"🍃 [HEALING LEAF] Kích hoạt! Bạn được hồi {healAmount} HP. Máu hiện tại: {statplayer.playerCurrentHp}/{statplayer.playerMaxHp} (Còn lại {healingLeafTurnsRemaining} lượt buff)");
            }
        }





        // COROUTINE XỬ LÝ LƯỢT CỦA QUÁI VẬT
        private IEnumerator EnemyTurnCoroutine()
        {
            // Bước 1: Đợi cho lưới kẹo nổ combo và rơi xuống đầy đủ (hết trạng thái Filling)
            while (gameGrid.IsFilling)
            {
                yield return null;
            }

            // Bước 2: Kiểm tra nếu quái đã bị người chơi tiêu diệt ở lượt vừa rồi thì dừng lại, không đánh nữa
            if (_isGameOver || enemy.enemyCurrentHp <= 0) yield break;

            if (frozenTurnsRemaining > 0)
            {
                Debug.Log($"❄️ [QUÁI BỊ SKIP LƯỢT] Quái vật đang bị đóng băng cứng ngắc! Không thể ra đòn ở lượt này.");
                yield return new WaitForSeconds(0.5f); // Chờ ngắn để người chơi kịp thấy dòng thông báo

                // Thoát khỏi Coroutine ngay tại đây, quái hoàn toàn không làm gì được bạn!
                yield break;
            }
 
            // Bước 3: Quái vật bắt đầu tấn công
            Debug.Log("👹 [LƯỢT QUÁI] Quái vật chuẩn bị ra đòn!");
            // BẬT BOOL ATTACK LÊN TRUE
            //enemyAnimator.SetBool("Attack", true);
            yield return new WaitForSeconds(0.5f);// Tạo một khoảng hoãn nhỏ tạo cảm giác chuyển lượt rõ ràng hơn
            enemyAnimator.SetBool("Attack", true);
            yield return new WaitForSeconds(1f);
            enemyAnimator.SetBool("Attack", false);
            // =========================================================================
            // 🔮 TÍCH HỢP LỜI NGUYỀN: Tính toán sát thương thực tế của quái ở lượt này
            // =========================================================================
            int finalEnemyAttack = enemy.enemyAttack;

            if (curseTurnsRemaining > 0)
            {
                // Giảm 40% -> Sát thương thực tế chỉ còn bằng 60% (0.6f) sát thương gốc
                finalEnemyAttack = Mathf.RoundToInt(enemy.enemyAttack * 0.6f);
                Debug.Log($"💀 Quái đang bị nguyền rủa! Sát thương giảm từ {enemy.enemyAttack} xuống {finalEnemyAttack}!");
            }
            // =========================================================================

            int shieldDamage = 10; // Quái mặc định đánh trừ 10 điểm khiên theo yêu cầu

            if (statplayer.playerShield >= shieldDamage)
            {
                // Trường hợp 1: Khiên có từ 10 điểm trở lên -> Trừ sạch vào khiên, máu an toàn tuyệt đối
                statplayer.playerShield -= shieldDamage;
                Debug.Log($"🛡️ Giáp của bạn chống đỡ hoàn toàn! Khiên bị trừ {shieldDamage} điểm. " +
                          $"Khiên còn lại: {statplayer.playerShield}. Bạn không bị mất máu.");
            }
            else
            {
                // Trường hợp 2: Khiên dưới 10 điểm -> Trừ hết số khiên đang có, lượng sát thương còn thừa đập vào HP
                 // Số điểm sát thương tràn qua khiên



              
                statplayer.playerCurrentHp -= enemy.enemyAttack; // Trừ máu lượng còn thiếu

                Debug.Log($"🩸 Bạn bị mất {enemy.enemyAttack} HP! Máu còn lại: {statplayer.playerCurrentHp}/{statplayer.playerMaxHp}");
            }

            // Bước 4: Kiểm tra xem người chơi có bị hết máu sau đòn đánh của quái không
            CheckBattleStatus();
        }

        private void CheckBattleStatus()
        {
            if (_isGameOver) return;

            if (enemy.enemyCurrentHp <= 0)
            {
                Debug.Log("🎉 Kẻ địch bị tiêu diệt! Bạn thắng trận!");
                GameWin();
            }
            else if (statplayer.playerCurrentHp <= 0)
            {
                // 🔥 KIỂM TRA BUFF HỒI SINH TRƯỚC KHI TÍNH LÀ THUA
                if (phoenixWingsTurnsRemaining > 0)
                {
                    // Tính toán lượng máu hồi lại bằng 40% Máu tối đa
                    int reviveHp = Mathf.RoundToInt(statplayer.playerMaxHp * 0.40f);
                    statplayer.playerCurrentHp = reviveHp;

                    // Xóa buff ngay lập tức sau khi đã dùng mạng hồi sinh này
                    phoenixWingsTurnsRemaining = 0;

                    Debug.Log($"🔥 [PHOENIX WINGS] Bạn đã hồi sinh từ tro tàn! Trả lại {reviveHp} HP (40% Máu tối đa). Trận đấu tiếp tục!");
                }
                else
                {
                    // Nếu không có buff thì chết thật
                    Debug.Log("💀 Bạn đã hết HP! Thất bại!");
                    GameLose();
                }
            }
        }

        protected virtual IEnumerator WaitForGridFill()
        {
            while (gameGrid.IsFilling)
            {
                yield return null;
            }

            if (_didWin)
            {
                hud.OnGameWin(currentScore);
            }
            else
            {
                hud.OnGameLose();
            }
        }
        // Hàm hỗ trợ tính toán sát thương/hiệu ứng dựa trên Màu và Hệ số nhân
        public void ApplyRawDamage(ColorType colorType, int multiplier)
        {
            // Kiểm tra xem chiêu The Wrath có đang kích hoạt không, nếu có thì nhân 3, không thì x1
            int wrathMultiplier = (wrathTurnsRemaining > 0) ? 3 : 1;

            switch (colorType)
            {
                
                case ColorType.Mana:
                    // NHÂN THÊM HỆ SỐ SEE THE SOUL VÀO MANA
                    int soulMultiplier = (seeTheSoulTurnsRemaining > 0) ? 3 : 1;

                    int manaRestore = 5 * multiplier * soulMultiplier;
                    statplayer.playerMana = Mathf.Min(statplayer.playerMana + manaRestore, statplayer.playerMaxMana);

                    string soulMsg = soulMultiplier > 1 ? "[👁️ X3 MANA]" : "";
                    Debug.Log($"🔵 Mana! Hồi {manaRestore} Năng lượng! (x{multiplier}) {soulMsg}");
                    break;

                case ColorType.Hp:
                    // NHÂN THÊM HỆ SỐ MEDITATION VÀO LƯỢNG MÁU HỒI
                    int meditationMultiplier = (meditationTurnsRemaining > 0) ? 3 : 1;

                    int hpRestore = 10 * multiplier * meditationMultiplier;
                    statplayer.playerCurrentHp = Mathf.Min(statplayer.playerCurrentHp + hpRestore, statplayer.playerMaxHp);

                    string meditationMsg = meditationMultiplier > 1 ? "[🧘‍♂️ X3 HP]" : "";
                    Debug.Log($"❤️ HP! Hồi {hpRestore} Máu! (x{multiplier}) {meditationMsg}");
                    break;

               ;
            }
        }
        private IEnumerator ApplyShieldAtEndOfFrame()
        {
            // Đợi đến cuối khung hình khi toàn bộ các viên kẹo trong cụm đã nổ xong xuôi
            yield return new WaitForEndOfFrame();

            if (_currentFrameShieldMultiplier > 0)
            {
                // 1. Xác định số điểm khiên nhận được dựa trên số lượng kẹo Khiên nổ (multiplier)
                int shieldGain = 0;

                if (_currentFrameShieldMultiplier == 3)
                {
                    shieldGain = 3;  // Ăn 3 viên = 3 điểm khiên
                }
                else if (_currentFrameShieldMultiplier == 4)
                {
                    shieldGain = 4;  // Ăn 4 viên = 4 điểm khiên
                }
                else if (_currentFrameShieldMultiplier >= 5)
                {
                    shieldGain = 20; // Ăn 5 viên trở lên = 20 điểm khiên cực khủng!
                }
                else
                {
                    // Phòng trường hợp combo rơi lẻ tẻ nhỏ hơn 3 viên
                    shieldGain = _currentFrameShieldMultiplier;
                }

                // 2. Cộng tích lũy vào hệ thống khiên của người chơi (khống chế theo maxShield)
                statplayer.playerShield = Mathf.Min(statplayer.playerShield + shieldGain, statplayer.maxShield);

                Debug.Log($"🛡️ SHIELD COMBO: Nổ cụm {_currentFrameShieldMultiplier} viên Khiên! " +
                          $"Tích lũy được +{shieldGain} điểm khiên. (Khiên hiện tại: {statplayer.playerShield}/{statplayer.maxShield})");

                // Reset lại biến đếm để chuẩn bị cho combo tiếp theo
                _currentFrameShieldMultiplier = 0;
                CheckBattleStatus();
            }
        }

        // HÀM MỚI: Phát ra animation đánh khi quái vật bị sát thương
        public void PlayEnemyHitAnimation()
        {
            // Reset trigger cũ tránh bị lỗi spam animation
            enemyAnimator.ResetTrigger("Hit1");
            enemyAnimator.ResetTrigger("Hit2");

            // Random 0 hoặc 1
            int randomHit = Random.Range(0, 2);

            if (randomHit == 0)
            {
                enemyAnimator.SetTrigger("Hit1");
            }
            else
            {
                enemyAnimator.SetTrigger("Hit2");
            }
        }
        // HÀM MỚI: Xử lý khi người chơi vuốt cục Lv2 và Lv3 cùng lúc (Nhân 3)
        public void ExecuteComboLv2Lv3(ColorType color, GamePiece p1, GamePiece p2)
        {
            if (_isGameOver) return;

            // Kích hoạt sát thương hệ số 3
            ApplyRawDamage(color, 3);

            // Xóa 2 viên kẹo này khỏi bàn cờ
            p1.GameGridRef.DestroyPieceAt(p1.X, p1.Y);
            p2.GameGridRef.DestroyPieceAt(p2.X, p2.Y);

            CheckBattleStatus();
            StartCoroutine(p1.GameGridRef.Fill());
        }
        public void ActivatePoisonKnives()
        {
            if (_isGameOver) return;

            // 1. Sát thương tức thì = 1.5x Sát thương người chơi (playerAttack)
            int instantDamage = Mathf.RoundToInt(statplayer.playerAttack * 1.5f);
            enemy.enemyCurrentHp -= instantDamage;
            Debug.Log($"🗡️ [SKILL] Ném Dao Độc! Gây {instantDamage} sát thương tức thì!");

            // 2. Kích hoạt trạng thái nhiễm độc trong 5 lượt
            poisonTurnsRemaining = 5;
            Debug.Log("🤢 Quái vật đã bị nhiễm độc trong 5 lượt tiếp theo!");

            CheckBattleStatus();
        }
        public void ActivateLifeSteal()
        {
            if (_isGameOver) return;

            // 1. Gây x2 Sát thương tức thì lên Quái
            int stealDamage = statplayer.playerAttack * 2;
            enemy.enemyCurrentHp -= stealDamage;
            Debug.Log($"🩸 [SKILL] Hút Máu! Gây {stealDamage} Sát thương lên quái! (x2 Damge)");

            // 2. Hồi lại 10% Máu dựa trên MÁU TỐI ĐA (playerMaxHp) của Người chơi
            int hpHeal = Mathf.RoundToInt(statplayer.playerMaxHp * 0.10f);

            // Đảm bảo lượng máu sau khi hồi không vượt quá Máu Tối Đa
            statplayer.playerCurrentHp = Mathf.Min(statplayer.playerCurrentHp + hpHeal, statplayer.playerMaxHp);

            Debug.Log($"❤️ Hút thành công! Hồi lại {hpHeal} Máu cho người chơi. (Máu hiện tại: {statplayer.playerCurrentHp}/{statplayer.playerMaxHp})");

            CheckBattleStatus();
        }
        public void ActivateCurseOfWeak()
        {
            if (_isGameOver) return;

            // 1. Gây sát thương x1 cho Quái vật
            int instantDamage = statplayer.playerAttack * 1;
            enemy.enemyCurrentHp -= instantDamage;
            Debug.Log($"🔮 [SKILL] Lời Nguyền Yếu Ớt! Gây {instantDamage} sát thương!");

            // 2. Áp đặt trạng thái giảm 40% sát thương trong 5 lượt
            curseTurnsRemaining = 5;
            Debug.Log("💀 Quái vật đã bị dính lời nguyền! Sát thương đầu ra giảm 40% trong 5 lượt tiếp theo.");

            CheckBattleStatus();
        }
        public void ActivateFrozen()
        {
            if (_isGameOver) return;

            // Gây sát thương bằng 1.5x Sát thương phép (playerSp) của Người chơi
            int instantDamage = Mathf.RoundToInt(statplayer.playerSp * 1.5f);
            enemy.enemyCurrentHp -= instantDamage;
            Debug.Log($"❄️ [SKILL] Băng Phong! Gây {instantDamage} Sát thương phép lên quái!");

            // Kích hoạt trạng thái Đóng Băng trong 5 lượt
            frozenTurnsRemaining = 5;
            Debug.Log("🥶 Quái vật đã bị ĐÓNG BĂNG hoàn toàn trong 5 lượt tiếp theo!");

            CheckBattleStatus();
        }

        // Thêm hàm này vào trong class Level.cs (ví dụ ngay dưới hàm ActivateFrozen)
        public void ActivateDefenseStance()
        {
            if (_isGameOver) return;

            // SỬA Ở ĐÂY: Thay số 2 bằng hàm GetMaxShieldAllowed() để có thể hồi lên tới 6 khiên
            statplayer.playerShield = Mathf.Min(statplayer.playerShield + 2, statplayer.maxShield);

            Debug.Log($"🛡️ [SKILL] Thế Thủ! Bạn nhận được 2 Khiên. (Khiên hiện tại: {statplayer.maxShield}/{statplayer.maxShield})");
        }
       
        public void ActivateHeavenShield()
        {
            if (_isGameOver) return;

            // Kích hoạt trạng thái nhân 3 giới hạn khiên trong 10 lượt
            heavenShieldTurnsRemaining = 10;
            Debug.Log($"😇 [BUFF] Khiên Thiên Đường! Giới hạn lá chắn tăng gấp 3 (Tối đa: {statplayer.maxShield} khiên) trong 10 lượt tiếp theo!");

          
        }
        public void ActivateTheWrath()
        {
            if (_isGameOver) return;

            // Kích hoạt trạng thái x3 sát thương trong 10 lượt
            wrathTurnsRemaining = 10;
            Debug.Log("🔥 [BUFF] Cơn Thịnh Nộ! Sát thương hệ Kiếm (Sword) và Phép (Spell) sẽ được nhân 3 trong 10 lượt tới!");
        }
        public void ActivateSeeTheSoul()
        {
            if (_isGameOver) return;

            // Kích hoạt trạng thái x3 Mana hồi phục trong 10 lượt
            seeTheSoulTurnsRemaining = 10;
            Debug.Log("👁️ [BUFF] See The Soul! Lượng Mana hồi phục khi ăn kẹo sẽ được nhân 3 trong 10 lượt tới!");
        }
        public void ActivateMindOfMeditation()
        {
            if (_isGameOver) return;

            // Kích hoạt trạng thái x3 hồi máu từ kẹo HP trong 10 lượt
            meditationTurnsRemaining = 10;
            Debug.Log("🧘‍♂️ [BUFF] Mind of Meditation! Lượng HP hồi phục từ kẹo Trái Tim sẽ được nhân 3 trong 10 lượt tới!");
        }
        public void ActivatePhoenixWings()
        {
            if (_isGameOver) return;

            // Kích hoạt trạng thái bảo hộ hồi sinh trong 10 lượt
            phoenixWingsTurnsRemaining = 10;
            Debug.Log("🔥 [BUFF] Phoenix Wings! Đôi cánh phượng hoàng dang rộng, bạn sẽ được tự động hồi sinh nếu cạn máu trong 10 lượt tới!");
        }
        public void ActivateHealingLeaf()
        {
            if (_isGameOver) return;

            // Kích hoạt bùa lợi hồi máu trong 4 lượt
            healingLeafTurnsRemaining = 4;
            Debug.Log("🍃 [BUFF] Healing Leaf! Lá cây chữa lành bao bọc lấy bạn, hồi 7% máu tối đa mỗi lượt trong 4 lượt tới.");
        }
        public void ActivateTreeOfLife()
        {
            if (_isGameOver) return;

            // 1. Tính toán lượng máu hồi phục = 70% Máu tối đa của người chơi
            int healAmount = Mathf.RoundToInt(statplayer.playerMaxHp * 0.70f);

            // 2. Tiến hành cộng máu và khống chế không cho vượt quá lượng máu tối đa (playerMaxHp)
            statplayer.playerCurrentHp = Mathf.Min(statplayer.playerCurrentHp + healAmount, statplayer.playerMaxHp);

            Debug.Log($"🌳 [SKILL] Tree of Life kích hoạt! Gọi ra đại thụ sinh mệnh, hồi ngay lập tức {healAmount} HP. Máu hiện tại: {statplayer.playerCurrentHp}/{statplayer.playerMaxHp}");

            // Kiểm tra lại trạng thái trận đấu (nếu cần)
            CheckBattleStatus();
        }
    }
}
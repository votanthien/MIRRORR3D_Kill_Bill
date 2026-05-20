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
        private void Start()
        {
            hud.SetScore(currentScore);

            // Khởi tạo máu ban đầu
            statplayer.playerCurrentHp = statplayer.playerMaxHp;
            enemy.enemyCurrentHp = enemy.enemyMaxHp;
            _isGameOver = false;
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

            if (!rpgEffectsEnabled) return;

            // ==========================================
            // 1. TIA SÉT (Chỉ dành cho Rainbow)
            // ==========================================
            if (piece.Type == PieceType.Rainbow)
            {
                int lightningDamage = statplayer.playerSp * 2;
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
                    if (multiplier > _currentFrameShieldMultiplier)
                    {
                        _currentFrameShieldMultiplier = multiplier;
                    }
                }
                else
                {
                    // Kiếm, HP, Mana, Spell vẫn giữ nguyên (nổ viên nào tính sát thương viên đó)
                    ApplyRawDamage(colorType, multiplier);
                }
            }

            CheckBattleStatus();
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
            yield return new WaitForSeconds(0.5f); // Tạo một khoảng hoãn nhỏ tạo cảm giác chuyển lượt rõ ràng hơn

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

            if (statplayer.playerShield > 0)
            {
                // Nếu có khiên, khiên sẽ hấp thụ toàn bộ đòn đánh và giảm đi 1 điểm
                statplayer.playerShield--;
                Debug.Log($"🛡️ Khiên của bạn đã đỡ đòn! Khiên còn lại: {statplayer.playerShield}/2. Bạn không mất máu.");
            }
            else
            {
                // Nếu không có khiên, trừ lượng sát thương thực tế (đã tính giảm công nếu có nguyền) trực tiếp vào HP
                statplayer.playerCurrentHp -= finalEnemyAttack;
                Debug.Log($"💥 Quái cắn bạn! Bạn mất {finalEnemyAttack} HP. Máu còn lại: {statplayer.playerCurrentHp}/{statplayer.playerMaxHp}");
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
                Debug.Log("💀 Bạn đã hết HP! Thất bại!");
                GameLose();
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
            switch (colorType)
            {
                case ColorType.Sword:
                    int damage = statplayer.playerAttack * multiplier;
                    enemy.enemyCurrentHp -= damage;
                    Debug.Log($"🗡️ Sword! Gây {damage} Sát thương! (x{multiplier})");
                    break;

                case ColorType.Mana: // <-- Đã sửa: Trả lại đúng case Mana và bỏ chữ private lỗi
                    int manaRestore = 5 * multiplier;
                    statplayer.playerMana = Mathf.Min(statplayer.playerMana + manaRestore, statplayer.playerMaxMana);
                    Debug.Log($"🔵 Mana! Hồi {manaRestore} Năng lượng! (x{multiplier})");
                    break;

                case ColorType.Hp:
                    int hpRestore = 10 * multiplier;
                    statplayer.playerCurrentHp = Mathf.Min(statplayer.playerCurrentHp + hpRestore, statplayer.playerMaxHp);
                    Debug.Log($"❤️ HP! Hồi {hpRestore} Máu! (x{multiplier})");
                    break;

                case ColorType.Spell:
                    int spellDamage = statplayer.playerSp * multiplier;
                    enemy.enemyCurrentHp -= spellDamage;
                    Debug.Log($"✨ Spell! Gây {spellDamage} Sát thương phép! (x{multiplier})");
                    break;
            }
        }
        private IEnumerator ApplyShieldAtEndOfFrame()
        {
            // Đợi đến cuối khung hình khi toàn bộ các viên kẹo trong hàng 3 đã nổ xong xuôi
            yield return new WaitForEndOfFrame();

            if (_currentFrameShieldMultiplier > 0)
            {
                // Chỉ cộng khiên DUY NHẤT 1 LẦN cho cả cụm kẹo vừa biến mất
                int shieldGain = 1 * _currentFrameShieldMultiplier;
                statplayer.playerShield = Mathf.Min(statplayer.playerShield + shieldGain, 2);

                Debug.Log($"🛡️ SHIELD: Cả cụm kẹo nổ giúp hồi {shieldGain} Khiên! (Khiên hiện tại: {statplayer.playerShield}/2)");

                // Reset lại biến để chuẩn bị cho lượt đi/combo tiếp theo
                _currentFrameShieldMultiplier = 0;
                CheckBattleStatus();
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

            // Cộng ngay lập tức 2 điểm Khiên cho người chơi
            // Đảm bảo lượng khiên sau khi cộng không vượt quá mốc tối đa là 2
            statplayer.playerShield = Mathf.Min(statplayer.playerShield + 2, 2);

            Debug.Log($"🛡️ [SKILL] Thế Thủ! Bạn nhận được 2 Khiên bảo vệ. (Khiên hiện tại: {statplayer.playerShield}/2)");

            // Kỹ năng phòng thủ thuần túy nên không cần chạy CheckBattleStatus để kiểm tra máu quái, 
            // nhưng nếu bạn có UI hiển thị khiên thì nên cập nhật UI ở đây nhé!
        }
    }
}
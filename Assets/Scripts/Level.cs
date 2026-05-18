using System.Collections;
using UnityEngine;

namespace Match3
{
    public class Level : MonoBehaviour
    {
        public GameGrid gameGrid;
        public Hud hud;

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

        private void Start()
        {
            hud.SetScore(currentScore);

            // Khởi tạo máu ban đầu
            statplayer.playerCurrentHp = statplayer.playerMaxHp;
            enemy.enemyCurrentHp = enemy.enemyMaxHp;
            _isGameOver = false;
        }

        public LevelType Type => type;

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
        }

        public virtual void OnPieceCleared(GamePiece piece)
        {
            if (_isGameOver) return;

            currentScore += piece.score;
            hud.SetScore(currentScore);

            // =========================================================
            // 1. LOGIC TIA SÉT (ĐỘC LẬP - KHÔNG LIÊN QUAN ĐẾN CỤC RAW)
            // =========================================================
            // ColumnClear và Rainbow giờ được tính chung là Tia Sét
            if (piece.Type == PieceType.Rainbow)
            {
                int lightningDamage = statplayer.playerSp * 2; // Ví dụ sát thương của tia sét
                enemy.enemyCurrentHp -= lightningDamage;

                Debug.Log($"⚡ TIA SÉT đánh trúng! Gây {lightningDamage} Sát thương lên quái!");
                CheckBattleStatus();

                // Tia sét nổ xong thì thoát hàm, không xử lý phần Cục Raw ở dưới nữa
                return;
            }

            // =========================================================
            // 2. LOGIC CỦA CỤC RAW THÔNG THƯỜNG (KIỂM TRA LV2, LV3)
            // =========================================================
            int multiplier = 1; // Cục Raw bình thường hệ số là 1

            if (piece.Type == PieceType.RowClear || piece.Type == PieceType.ColumnClear )
            {
                // Dùng RowClear làm đại diện cho cục Raw Lv2
                multiplier = 2;
            }
            // Giả sử bạn thêm PieceType.Lv3 vào code
            /* else if (piece.Type == PieceType.Lv3) 
            {
                multiplier = 4; // Cục Raw Lv3 nhân 4 theo đúng thiết kế của bạn
            } */

            // Xử lý chỉ số dựa theo Màu của Cục Raw và nhân với Hệ số (Lv)
            if (piece.IsColored())
            {
                ColorType colorType = piece.ColorComponent.Color;

                switch (colorType)
                {
                    case ColorType.Sword:
                        int damage = statplayer.playerAttack * multiplier;
                        enemy.enemyCurrentHp -= damage;
                        Debug.Log($"🗡️ Sword [Lv{multiplier}]! Gây {damage} Sát thương!");
                        break;

                    case ColorType.Shield:
                        statplayer.playerShield = Mathf.Min(statplayer.playerShield + (1 * multiplier), 2);
                        Debug.Log($"🛡️ Shield [Lv{multiplier}]! Hồi {1 * multiplier} Khiên!");
                        break;

                    case ColorType.Mana:
                        int manaRestore = 5 * multiplier;
                        statplayer.playerMana = Mathf.Min(statplayer.playerMana + manaRestore, statplayer.playerMaxMana);
                        Debug.Log($"🔵 Mana [Lv{multiplier}]! Hồi {manaRestore} Năng lượng!");
                        break;

                    case ColorType.Hp:
                        int hpRestore = 10 * multiplier;
                        statplayer.playerCurrentHp = Mathf.Min(statplayer.playerCurrentHp + hpRestore, statplayer.playerMaxHp);
                        Debug.Log($"❤️ HP [Lv{multiplier}]! Hồi {hpRestore} Máu!");
                        break;

                    case ColorType.Spell:
                        int spellDamage = statplayer.playerSp * multiplier;
                        enemy.enemyCurrentHp -= spellDamage;
                        Debug.Log($"✨ Spell [Lv{multiplier}]! Gây {spellDamage} Sát thương phép!");
                        break;
                }
            }

            CheckBattleStatus();
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

            // Bước 3: Quái vật bắt đầu tấn công
            Debug.Log("👹 [LƯỢT QUÁI] Quái vật chuẩn bị ra đòn!");
            yield return new WaitForSeconds(0.5f); // Tạo một khoảng hoãn nhỏ tạo cảm giác chuyển lượt rõ ràng hơn

            if (statplayer.playerShield > 0)
            {
                // Nếu có khiên, khiên sẽ hấp thụ toàn bộ đòn đánh và giảm đi 1 điểm
                statplayer.playerShield--;
                Debug.Log($"🛡️ Khiên của bạn đã đỡ đòn! Khiên còn lại: {statplayer.playerShield}/2. Bạn không mất máu.");
            }
            else
            {
                // Nếu không có khiên, trừ trực tiếp vào HP
                statplayer.playerCurrentHp -= enemy.enemyAttack;
                Debug.Log($"💥 Quái cắn bạn! Bạn mất {enemy.enemyAttack} HP. Máu còn lại: {statplayer.playerCurrentHp}/{statplayer.playerMaxHp}");
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
                    Debug.Log($"🗡️ Sword Combo! Gây {damage} Sát thương vật lý! (Hệ số x{multiplier})");
                    break;

                case ColorType.Shield:
                    statplayer.playerShield = Mathf.Min(statplayer.playerShield + (1 * multiplier), 2);
                    Debug.Log($"🛡️ Shield Combo! Hồi {1 * multiplier} Khiên! (Hệ số x{multiplier})");
                    break;

                case ColorType.Mana:
                    int manaRestore = 5 * multiplier;
                    statplayer.playerMana = Mathf.Min(statplayer.playerMana + manaRestore, statplayer.playerMaxMana);
                    Debug.Log($"🔵 Mana Combo! Hồi {manaRestore} Năng lượng! (Hệ số x{multiplier})");
                    break;

                case ColorType.Hp:
                    int hpRestore = 10 * multiplier;
                    statplayer.playerCurrentHp = Mathf.Min(statplayer.playerCurrentHp + hpRestore, statplayer.playerMaxHp);
                    Debug.Log($"❤️ HP Combo! Hồi {hpRestore} Máu! (Hệ số x{multiplier})");
                    break;

                case ColorType.Spell:
                    int spellDamage = statplayer.playerSp * multiplier;
                    enemy.enemyCurrentHp -= spellDamage;
                    Debug.Log($"✨ Spell Combo! Gây {spellDamage} Sát thương phép! (Hệ số x{multiplier})");
                    break;
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
    }
}
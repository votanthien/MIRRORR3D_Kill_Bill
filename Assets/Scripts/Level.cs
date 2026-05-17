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

            if (piece.IsColored())
            {
                ColorType colorType = piece.ColorComponent.Color;

                switch (colorType)
                {
                    case ColorType.Sword:
                        enemy.enemyCurrentHp -= statplayer.playerAttack;
                        Debug.Log($"🗡️ Bạn chém Sword! Máu địch còn: {enemy.enemyCurrentHp}/{enemy.enemyMaxHp}");
                        break;

                    case ColorType.Shield:
                        statplayer.playerShield = Mathf.Min(statplayer.playerShield + 1, 2);
                        Debug.Log($"🛡️ Bạn buff Shield! Khiên hiện tại: {statplayer.playerShield}/2");
                        break;

                    case ColorType.Mana:
                        statplayer.playerMana += 5;
                        Debug.Log($"🔵 Bạn hồi Mana! Năng lượng: {statplayer.playerMana}");
                        break;

                    case ColorType.Hp:
                        statplayer.playerCurrentHp = Mathf.Min(statplayer.playerCurrentHp + 10, statplayer.playerMaxHp);
                        Debug.Log($"❤️ Bạn hồi HP! Máu của bạn: {statplayer.playerCurrentHp}/{statplayer.playerMaxHp}");
                        break;

                    case ColorType.Spell:
                        enemy.enemyCurrentHp -= statplayer.playerSp;
                        Debug.Log($"✨ Bạn tung Spell! Máu địch còn: {enemy.enemyCurrentHp}/{enemy.enemyMaxHp}");
                        break;
                }

                // Kiểm tra ngay xem quái có chết sau chuỗi ăn kẹo này không
                CheckBattleStatus();
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
    }
}
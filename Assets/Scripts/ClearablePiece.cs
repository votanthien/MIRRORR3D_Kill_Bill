using System.Collections;
using UnityEngine;

namespace Match3
{
    public class ClearablePiece : MonoBehaviour
    {
        public AnimationClip clearAnimation;

        public bool IsBeingCleared { get; private set; }

        protected GamePiece piece;

        private void Awake()
        {
            piece = GetComponent<GamePiece>();
        }

        public virtual void Clear()
        {
            // --- THÊM DÒNG NÀY ---
            // Nếu kẹo đang trong quá trình nổ rồi thì chặn lại, không cho chạy tiếp để tránh gọi điểm 2 lần!
            if (IsBeingCleared)
            {
                return;
            }

            // Đánh dấu là kẹo bắt đầu nổ
            IsBeingCleared = true;

            // Báo điểm sang Level (Đảm bảo giờ chỉ bị gọi 1 lần duy nhất cho mỗi viên kẹo)
            piece.GameGridRef.level.OnPieceCleared(piece);

            StartCoroutine(ClearCoroutine());
        }

        private IEnumerator ClearCoroutine()
        {
            var animator = GetComponent<Animator>();

            if (animator)
            {
                animator.Play(clearAnimation.name);

                yield return new WaitForSeconds(clearAnimation.length);

                Destroy(gameObject);
            }
        }
    }
}

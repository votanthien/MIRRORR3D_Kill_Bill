namespace Match3
{
    public class LevelMoves : Level
    {
        public int numMoves;
        public int movesUsed = 0;

        // ... các hàm khác ...

        public override void OnMove()
        {
           
            base.OnMove();

           
            movesUsed++;
            hud.SetRemaining(numMoves - movesUsed);

            if (numMoves - movesUsed == 0)
            {
                if (currentScore >= score1Star)
                {
                    GameWin();
                }
                else
                {
                    GameLose();
                }
            }
        }
    }
}

namespace MinesweeperWebApp.Services
{
    // This service handles score calculation for a winning game.
    public class ScoreService
    {
        // This method calculates a score based on time,
        // board size, and difficulty.
        public int CalculateScore(int elapsedSeconds, int boardSize, int difficulty)
        {
            // Keep time from causing negative scores.
            if (elapsedSeconds < 1)
            {
                elapsedSeconds = 1;
            }

            // Basic score formula.
            int score = (boardSize * difficulty * 100) - elapsedSeconds;

            // Make sure the score never drops below zero.
            if (score < 0)
            {
                score = 0;
            }

            return score;
        }
    }
}
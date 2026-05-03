namespace MinesweeperWebApp.Models
{
    public class Board
    {
        public int Size { get; set; }
        public Cell[][] Cells { get; set; }

        public bool IsGameOver { get; set; }
        public bool IsWin { get; set; }
        public int Score { get; set; }

        // Tracks if the gold bag was found on the current move.
        public bool GoldBagFoundThisMove { get; set; }

        // Builds the board with mines, neighbor counts, and the gold bag for Hard mode.
        public Board(int size, string difficulty = "Easy")
        {
            Size = size;
            Cells = new Cell[size][];

            for (int row = 0; row < size; row++)
            {
                Cells[row] = new Cell[size];

                for (int col = 0; col < size; col++)
                {
                    Cells[row][col] = new Cell();
                }
            }

            PlaceMines();

            if (difficulty == "Hard")
            {
                PlaceGoldBag();
            }

            CalculateLiveNeighbors();
        }

        // Needed so saved game data can be loaded back from JSON.
        public Board()
        {
        }

        // Reveals a cell and updates the game state.
        public bool RevealCell(int row, int col)
        {
            GoldBagFoundThisMove = false;

            if (IsGameOver || IsWin)
            {
                return false;
            }

            if (row < 0 || col < 0 || row >= Size || col >= Size)
            {
                return false;
            }

            Cell clickedCell = Cells[row][col];

            if (clickedCell.IsVisited || clickedCell.IsFlagged)
            {
                return false;
            }

            clickedCell.IsVisited = true;

            // Hitting a mine ends the game.
            if (clickedCell.HasMine)
            {
                IsGameOver = true;
                return true;
            }

            // Finding the gold bag triples the current score.
            if (clickedCell.HasGoldBag)
            {
                Score = Score * 3;
                GoldBagFoundThisMove = true;
            }
            else if (clickedCell.LiveNeighbors > 0)
            {
                Score += clickedCell.LiveNeighbors * 10;
            }

            // Reveal blank areas around safe empty tiles.
            if (clickedCell.LiveNeighbors == 0 && !clickedCell.HasGoldBag)
            {
                FloodFillReveal(row, col);
            }

            CheckForWin();
            return true;
        }

        // Reveals connected blank cells and nearby numbered cells.
        private void FloodFillReveal(int row, int col)
        {
            for (int r = row - 1; r <= row + 1; r++)
            {
                for (int c = col - 1; c <= col + 1; c++)
                {
                    if (r < 0 || c < 0 || r >= Size || c >= Size)
                    {
                        continue;
                    }

                    if (r == row && c == col)
                    {
                        continue;
                    }

                    Cell neighbor = Cells[r][c];

                    if (neighbor.IsVisited || neighbor.IsFlagged || neighbor.HasMine || neighbor.HasGoldBag)
                    {
                        continue;
                    }

                    neighbor.IsVisited = true;

                    if (neighbor.LiveNeighbors > 0)
                    {
                        Score += neighbor.LiveNeighbors * 10;
                    }

                    if (neighbor.LiveNeighbors == 0)
                    {
                        FloodFillReveal(r, c);
                    }
                }
            }
        }

        // Lets the service check for a win after flag changes.
        public void UpdateWinStatus()
        {
            CheckForWin();
        }

        // Checks if every needed cell has been handled.
        private void CheckForWin()
        {
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    Cell cell = Cells[row][col];

                    if (!cell.IsVisited && !cell.IsFlagged)
                    {
                        return;
                    }
                }
            }

            IsWin = true;
        }

        // Places mines randomly on the board.
        private void PlaceMines()
        {
            Random rand = new Random();
            int minesToPlace = Size;
            int placed = 0;

            while (placed < minesToPlace)
            {
                int row = rand.Next(Size);
                int col = rand.Next(Size);

                if (!Cells[row][col].HasMine)
                {
                    Cells[row][col].HasMine = true;
                    placed++;
                }
            }
        }

        // Places one gold bag on a safe tile for Hard mode.
        private void PlaceGoldBag()
        {
            Random rand = new Random();
            bool placed = false;

            while (!placed)
            {
                int row = rand.Next(Size);
                int col = rand.Next(Size);

                if (!Cells[row][col].HasMine)
                {
                    Cells[row][col].HasGoldBag = true;
                    placed = true;
                }
            }
        }

        // Counts how many mines are around each cell.
        private void CalculateLiveNeighbors()
        {
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if (Cells[row][col].HasMine)
                    {
                        Cells[row][col].LiveNeighbors = -1;
                        continue;
                    }

                    int mineCount = 0;

                    for (int r = row - 1; r <= row + 1; r++)
                    {
                        for (int c = col - 1; c <= col + 1; c++)
                        {
                            if (r >= 0 && r < Size && c >= 0 && c < Size)
                            {
                                if (!(r == row && c == col) && Cells[r][c].HasMine)
                                {
                                    mineCount++;
                                }
                            }
                        }
                    }

                    Cells[row][col].LiveNeighbors = mineCount;
                }
            }
        }
    }
}
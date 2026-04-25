namespace MinesweeperWebApp.Models
{
    public class Board
    {
        public int Size { get; set; }
        public Cell[][] Cells { get; set; }

        public bool IsGameOver { get; set; }
        public bool IsWin { get; set; }
        public int Score { get; set; }

        // Method to initialize the board with mines and calculate live neighbors
        public Board(int size)
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
            CalculateLiveNeighbors();
        }

        // Parameterless constructor for deserialization
        public Board()
        {
        }

        // Method to reveal a cell and update game state accordingly
        public bool RevealCell(int row, int col)
        {
            if (IsGameOver || IsWin)
                return false;

            if (row < 0 || col < 0 || row >= Size || col >= Size)
                return false;

            Cell clickedCell = Cells[row][col];

            if (clickedCell.IsVisited || clickedCell.IsFlagged)
                return false;

            clickedCell.IsVisited = true;

            // Bomb clicked = game over
            if (clickedCell.HasMine)
            {
                IsGameOver = true;
                return true;
            }

            // Award points for numbered tiles only
            if (clickedCell.LiveNeighbors > 0)
            {
                Score += clickedCell.LiveNeighbors * 10;
            }

            // Optional: reveal blank areas automatically
            if (clickedCell.LiveNeighbors == 0)
            {
                FloodFillReveal(row, col);
            }

            CheckForWin();
            return true;
        }

        // Method to reveal surrounding blank cells
        private void FloodFillReveal(int row, int col)
        {
            for (int r = row - 1; r <= row + 1; r++)
            {
                for (int c = col - 1; c <= col + 1; c++)
                {
                    if (r < 0 || c < 0 || r >= Size || c >= Size)
                        continue;

                    if (r == row && c == col)
                        continue;

                    Cell neighbor = Cells[r][c];

                    if (neighbor.IsVisited || neighbor.IsFlagged || neighbor.HasMine)
                        continue;

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

        // Milestone 3:
        // This lets the service check for a win after flag changes too
        public void UpdateWinStatus()
        {
            CheckForWin();
        }

        // Method to check if the player has won
        private void CheckForWin()
        {
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    Cell cell = Cells[row][col];

                    // Milestone 3:
                    // The player wins when every cell has either been revealed or flagged
                    if (!cell.IsVisited && !cell.IsFlagged)
                    {
                        return;
                    }
                }
            }

            IsWin = true;
        }

        // Method to place mines randomly on the board
        private void PlaceMines()
        {
            Random rand = new Random();
            int minesToPlace = Size;
            int placed = 0;

            while (placed < minesToPlace)
            {
                int row = rand.Next(Size);
                int col = rand.Next(Size);

                if (Cells[row] != null && Cells[row][col] != null && !Cells[row][col].HasMine)
                {
                    Cells[row][col].HasMine = true;
                    placed++;
                }
            }
        }

        // Method to calculate the number of live neighbors for each cell
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
namespace MinesweeperWebApp.Models
{
    public class Board
    {
        // Size of the board (e.g., 10 for a 10x10 board)
        public int Size { get; set; }
        public Cell[][] Cells { get; set; }

        // Constructor to initialize the board with the given size
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
        }

        // Method to reveal a cell at the specified row and column
        public void RevealCell(int row, int col)
        {
            if (row < 0 || col < 0 || row >= Size || col >= Size)
                return;

            if (Cells[row][col].IsVisited || Cells[row][col].IsFlagged)
                return;

            Cells[row][col].IsVisited = true;
        }
    }
}

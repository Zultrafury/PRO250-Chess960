namespace Test960;

public class UnitTest1
{
    void CheckPiecesPlaced(string row)
    {
        Assert.True(row.Contains('k') || row.Contains('K'));
        Assert.True(row.Contains('q') || row.Contains('Q'));
        Assert.True(row.Count(c => c == 'n') == 2 || row.Count(c => c == 'N') == 2);
        Assert.True(row.Count(c => c == 'r') == 2 || row.Count(c => c == 'R') == 2);
        Assert.True(row.Count(c => c == 'b') == 2 || row.Count(c => c == 'B') == 2);
    }

    void CheckKingBetweenRooks(string row)
    {
        if (row.Contains('k')) Assert.True(row.IndexOf('k') > row.IndexOf('r') && row.IndexOf('k') < row.LastIndexOf('r'));
        else Assert.True(row.IndexOf('K') > row.IndexOf('R') && row.IndexOf('K') < row.LastIndexOf('R'));
    }

    void CheckBishopsOppositeSquares(string row)
    {
        if (row.Contains('b')) Assert.True(row.IndexOf('b')%2 != row.LastIndexOf('b')%2);
        else Assert.True(row.IndexOf('B')%2 != row.LastIndexOf('B')%2);
    }
    
    [Fact]
    public void PiecesPlaced()
    {
        List<string> testrows =  new List<string>();
        for (int i = 0; i < 100; i++) testrows.Add(Game.Chess960Row(i%2==1));

        foreach (var row in testrows) CheckPiecesPlaced(row);
    }

    [Fact]
    public void KingBetweenRooks()
    {
        List<string> testrows =  new List<string>();
        for (int i = 0; i < 100; i++) testrows.Add(Game.Chess960Row(i%2==1));

        foreach (var row in testrows) CheckKingBetweenRooks(row);
    }

    [Fact]
    public void BishopsOppositeSquares()
    {
        List<string> testrows =  new List<string>();
        for (int i = 0; i < 100; i++) testrows.Add(Game.Chess960Row(i%2==1));

        foreach (var row in testrows) CheckBishopsOppositeSquares(row);
    }

    [Fact]
    public void NormalBoardCheck()
    {
        var game = new Game(true);
        char[,] board = new char[8,8]
        {
            {'R','N','B','Q','K','B','N','R'},
            {'P','P','P','P','P','P','P','P'},
            {'.','.','.','.','.','.','.','.'},
            {'.','.','.','.','.','.','.','.'},
            {'.','.','.','.','.','.','.','.'},
            {'.','.','.','.','.','.','.','.'},
            {'p','p','p','p','p','p','p','p'},
            {'r','n','b','q','k','b','n','r'}
        };
        
        Assert.True(game.board.Cast<char>().SequenceEqual(board.Cast<char>()));
    }
    
    [Fact]
    public void Chess960BoardCheck()
    {
        var game = new Game(false);

        var chars = new []{ 'X','P','.','.','.','.','p','x' };
        for (int i = 0; i < chars.Length; i++)
        {
            switch (chars[i])
            {
                case '.' or 'P' or 'p':
                    for (int j = 0; j < 8; j++) Assert.True(game.board[i,j] == chars[i]);
                    break;
                case 'X' or 'x':
                    string row = "";
                    for (int j = 0; j < 8; j++) row += game.board[i,j];
                    CheckPiecesPlaced(row);
                    CheckKingBetweenRooks(row);
                    CheckBishopsOppositeSquares(row);
                    break;
            }
        }
    }
}
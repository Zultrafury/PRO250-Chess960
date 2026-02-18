public class Game {
    [Flags]
    enum Castling {
        None = 0,
        RookA = 1,
        RookH = 2,
        Either = 3
    };

    private readonly char[,] board = new char[8, 8];
    public bool isWhiteTurn = true;
    private (int row, int col) enPassant = (-1, -1); // location of pawn that advanced two squares last move
    private Castling whiteCastle = Castling.Either, blackCastle = Castling.Either;
    private char whitePawnPromote = 'q', blackPawnPromote = 'Q';
    
    static string placeKingRooks(bool capitalized)
    {
        char[] outstring = "........".ToCharArray();

        int king = 1 + (Random.Shared.Next() % 6);
        outstring[king] = capitalized ? 'K' : 'k';

        for (int i = 0; i < 2; i++)
        {
            int rook = 0;
            if (i == 0)
            {
                rook = Random.Shared.Next() % king;
            }
            else
            {
                rook = king + (Random.Shared.Next() % (king - 7));
            }
                
            outstring[rook] = capitalized ? 'R' : 'r';
        }
            
        return outstring.ToString()!;
    }
        
    static string placeBishops(bool capitalized, string kingrookstring)
    {
        int first = 2*(Random.Shared.Next()%4);
        int second = 2*(Random.Shared.Next()%4)+1;
        char bishopchar = capitalized ? 'C' : 'c';
        char[] outstring = "........".ToCharArray();
        outstring[first] = bishopchar; 
        outstring[second] = bishopchar;
        return outstring.ToString()!;
    }

    public Game() {
        //       abcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefgh
        var b = "RNBQKBNRPPPPPPPP................................pppppppprnbqkbnr";
        for (int r = 0, i = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
            board[r, c] = b[i++];
    }
    
    public Game(bool notnormal)
    {
        
        
        if (notnormal)
        {
            
            var b = "RNBQKBNRPPPPPPPP................................pppppppprnbqkbnr";
            for (int r = 0, i = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
                board[r, c] = b[i++];
        }
        else
        {
            var b = "RNBQKBNRPPPPPPPP................................pppppppprnbqkbnr";
            for (int r = 0, i = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
                board[r, c] = b[i++];
        }
    }
    

    public Game(Game save) => Restore(save);

    public string MoveString(int fromRow, int fromCol, int toRow, int toCol) =>
        "" + board[fromRow, fromCol] + RC(fromRow, fromCol) + "-" + RC(toRow, toCol) +
        (board[toRow, toCol] == '.' ? "" : "*" + board[toRow, toCol]);

    public char SetPromotion(char promote) {
        if ((isWhiteTurn ? "rbnq" : "RBNQ").Contains(promote))
            return isWhiteTurn ? whitePawnPromote = promote : blackPawnPromote = promote;
        return isWhiteTurn ? whitePawnPromote : blackPawnPromote;
    }

    public void Restore(Game save) {
        Array.Copy(save.board, board, 64);
        isWhiteTurn = save.isWhiteTurn;
        enPassant = save.enPassant;
        whiteCastle = save.whiteCastle;
        blackCastle = save.blackCastle;
        whitePawnPromote = save.whitePawnPromote;
        blackPawnPromote = save.blackPawnPromote;
    }

    public Game Save() => new(this);

    public override string ToString() {
        char[] chars = new char[64 + 8];
        for (int r = 0, i = 0; r < 8; r++) {
            for (int c = 0; c < 8; c++)
                chars[i++] = board[r, c];
            chars[i++] = '\n';
        }
        return new string(chars);
    }

    public static bool IsWhite(char piece) => char.IsLower(piece);

    public bool MovePossible(int fromRow, int fromCol, int toRow, int toCol) =>
        MovePossible(fromRow, fromCol, toRow, toCol, isWhiteTurn);

    private bool MovePossible(int fromRow, int fromCol, int toRow, int toCol, bool isWhite) {
        if (fromRow == toRow && fromCol == toCol) return false;
        if (fromRow is < 0 or >= 8 || fromCol is < 0 or >= 8) return false;
        var piece = board[fromRow, fromCol];
        if (piece == '.' || IsWhite(piece) != isWhite) return false;
        if (toRow is < 0 or >= 8 || toCol is < 0 or >= 8) return false;
        if (toRow == fromRow && toCol == fromCol) return false;
        var capture = board[toRow, toCol];
        if (capture != '.' && IsWhite(capture) == IsWhite(piece)) return false;
        int rowDiff = toRow - fromRow, colDiff = toCol - fromCol;
        switch (char.ToLower(piece)) { // break will verify the path is clear
            case 'p':
                if (Math.Abs(colDiff) > 1) return false;
                int pawnDir = IsWhite(piece) ? -1 : 1;
                if (colDiff != 0 && Math.Abs(rowDiff) == 1 && capture == '.') // en passant
                    return enPassant.row == (piece == 'p' ? toRow + 1 : toRow - 1) && enPassant.col == toCol;
                if (rowDiff == pawnDir) return (capture == '.') == (colDiff == 0);
                if (rowDiff != pawnDir + pawnDir || fromRow != (IsWhite(piece) ? 6 : 1) || colDiff != 0) return false;
                return board[fromRow + pawnDir, fromCol] == '.' && capture == '.';
            case 'r':
                if (rowDiff == 0 || colDiff == 0) break;
                return false;
            case 'n':
                return rowDiff is -2 or 2 && colDiff is -1 or 1 || rowDiff is -1 or 1 && colDiff is -2 or 2;
            case 'b':
                if (rowDiff == colDiff || rowDiff == -colDiff) break;
                return false;
            case 'q':
                if (rowDiff == 0 || colDiff == 0 || rowDiff == colDiff || rowDiff == -colDiff) break;
                return false;
            case 'k':
                if (rowDiff == 0 && colDiff is -2 or 2) { // castling
                    if (((isWhite ? whiteCastle : blackCastle) & (colDiff < 0 ? Castling.RookA : Castling.RookH)) == 0) return false;
                    int rookStartCol = colDiff < 0 ? 0 : 7, rookDestCol = colDiff < 0 ? 3 : 5, rookDirection = colDiff < 0 ? 1 : -1;
                    for (int c = rookStartCol + rookDirection; c != rookDestCol; c += rookDirection)
                        if (board[fromRow, c] != '.')
                            return false;
                    return true;
                }
                return rowDiff is 1 or 0 or -1 && colDiff is 1 or 0 or -1;
            default:
                throw new Exception("unknown piece");
        }
        int rowDir = Math.Sign(rowDiff), colDir = Math.Sign(colDiff);
        for (int r = fromRow + rowDir, c = fromCol + colDir; r != toRow || c != toCol; r += rowDir, c += colDir)
            if (board[r, c] != '.')
                return false;
        return true;
    }

    public bool Move(int fromRow, int fromCol, int toRow, int toCol) {
        if (!MovePossible(fromRow, fromCol, toRow, toCol, isWhiteTurn)) return false;
        MoveInternal(fromRow, fromCol, toRow, toCol);
        isWhiteTurn = !isWhiteTurn;
        return true;
    }

    private void MoveInternal(int fromRow, int fromCol, int toRow, int toCol) {
        char piece = board[toRow, toCol] = board[fromRow, fromCol];
        if (piece is 'p' or 'P' && toRow is 0 or 7) board[toRow, toCol] = piece == 'p' ? whitePawnPromote : blackPawnPromote;
        else if (piece is 'p' or 'P' && toCol != fromCol && board[toRow, toCol] == '.') {
            int captureRow = toRow + (piece == 'p' ? 1 : -1);
            if (enPassant.row == captureRow && enPassant.col == toCol)
                board[captureRow, toCol] = '.'; // capture en passant
        } else if (piece is 'k' or 'K' && fromCol - toCol is -2 or 2) { // castling
            int rookFromCol = toCol == 2 ? 0 : 7, rookToCol = toCol == 2 ? 3 : 5;
            board[toRow, rookToCol] = board[toRow, rookFromCol];
            board[toRow, rookFromCol] = '.';
            _ = piece is 'k' ? whiteCastle = Castling.None : blackCastle = Castling.None;
        } else if (fromRow == 0 && fromCol == 0) whiteCastle &= ~Castling.RookA;
        else if (fromRow == 0 && fromCol == 7) whiteCastle &= ~Castling.RookH;
        else if (fromRow == 7 && fromCol == 0) blackCastle &= ~Castling.RookA;
        else if (fromRow == 7 && fromCol == 7) blackCastle &= ~Castling.RookH;
        else if (fromRow == 0 && fromCol == 4) whiteCastle = Castling.None;
        else if (fromRow == 7 && fromCol == 4) blackCastle = Castling.None;
        enPassant = (piece is 'p' or 'P' && toRow - fromRow is -2 or 2) ? (toRow, toCol) : (-1, -1);
        board[fromRow, fromCol] = '.';
    }

    private static readonly (int dy, int dx)[] RookDirections = { (1, 0), (-1, 0), (0, 1), (0, -1) };
    private static readonly (int dy, int dx)[] BishopDirections = { (1, 1), (-1, 1), (1, -1), (-1, -1) };
    private static readonly (int dy, int dx)[] QueenKingDirections = { (1, 1), (-1, 1), (1, -1), (-1, -1), (1, 0), (-1, 0), (0, 1), (0, -1) };
    private static readonly (int dy, int dx)[] KnightDirections = { (1, 2), (2, 1), (-1, 2), (2, -1), (1, -2), (-2, 1), (-1, -2), (-2, -1) };
    private static readonly (int dy, int dx)[] WhitePawnDirections = { (-1, 0), (-1, -1), (-1, 1), (-2, 0) };
    private static readonly (int dy, int dx)[] BlackPawnDirections = { (1, 0), (1, -1), (1, 1), (2, 0) };

    private static readonly Dictionary<char, (int dy, int dx)[]> PieceDirections = new() {
        { 'r', RookDirections },
        { 'R', RookDirections },
        { 'b', BishopDirections },
        { 'B', BishopDirections },
        { 'q', QueenKingDirections },
        { 'Q', QueenKingDirections },
        { 'k', QueenKingDirections },
        { 'K', QueenKingDirections },
        { 'n', KnightDirections },
        { 'N', KnightDirections },
        { 'p', WhitePawnDirections },
        { 'P', BlackPawnDirections }
    };

    private IEnumerable<(int row, int col, char capture)> PossibleMoves(int fromRow, int fromCol, bool isWhite) {
        char piece = board[fromRow, fromCol];
        if (piece != '.' && IsWhite(piece) == isWhite) {
            var directions = PieceDirections[piece];
            if (char.ToLower(piece) is 'p' or 'n' or 'k') { // pawns, knights, and kings can only move one step
                if (char.ToLower(piece) == 'k') { // castling
                    if (MovePossible(fromRow, fromCol, fromRow, fromCol - 2, isWhite))
                        yield return (fromRow, fromCol - 2, '.');
                    if (MovePossible(fromRow, fromCol, fromRow, fromCol + 2, isWhite))
                        yield return (fromRow, fromCol + 2, '.');
                }
                foreach (var dir in directions)
                    if (MovePossible(fromRow, fromCol, fromRow + dir.dy, fromCol + dir.dx, isWhite))
                        yield return (fromRow + dir.dy, fromCol + dir.dx, board[fromRow + dir.dy, fromCol + dir.dx]);
            } else
                foreach (var dir in directions) // rooks, bishops, and queens can move multiple steps
                    for (int r = fromRow + dir.dy, c = fromCol + dir.dx; r is >= 0 and < 8 && c is >= 0 and < 8; r += dir.dy, c += dir.dx) {
                        if (MovePossible(fromRow, fromCol, r, c, isWhite))
                            yield return (r, c, board[r, c]);
                        if (board[r, c] != '.') break;
                    }
        }
    }

    public IEnumerable<(char piece, int fromRow, int fromCol, int toRow, int toCol, char capture, int score)> PossibleMoves() =>
        PossibleMoves(isWhiteTurn);

    private IEnumerable<(char piece, int fromRow, int fromCol, int toRow, int toCol, char capture, int score)> PossibleMoves(bool isWhite) {
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
                foreach (var m in PossibleMoves(r, c, isWhite))
                    yield return (board[r, c], r, c, m.row, m.col, m.capture,
                        Value(m.capture, m.row) + (char.ToLower(board[r, c]) == 'p' && m.row is 0 or 7 ? 8 : 0));
    }

    public (char piece, int fromRow, int fromCol, int toRow, int toCol, char capture, int score) BestMove(Random random, int depth) {
        var saves = new Game[depth];
        for (int i = 0; i < depth; i++) saves[i] = new Game(this);
        return BestMoveInternal(isWhiteTurn, random, depth, saves);
    }

    private (char piece, int fromRow, int fromCol, int toRow, int toCol, char capture, int score) BestMoveInternal(bool isWhite, Random random, int depth, Game[] saves) {
        (char piece, int fromRow, int fromCol, int toRow, int toCol, char capture, int score) bestMove = default;
        var (bestScore, bestMovesCount, moves) = (int.MinValue, 0, IsCheck(isWhite) ?? PossibleMoves(isWhite));
        if (depth == 0)
            foreach (var move in moves)
                BestScore(move.score, move);
        else {
            saves[depth - 1].Restore(this);
            foreach (var move in moves) {
                MoveInternal(move.fromRow, move.fromCol, move.toRow, move.toCol);
                BestScore(move.score - BestMoveInternal(!isWhite, random, depth - 1, saves).score, move);
                Restore(saves[depth - 1]);
            }
        }
        return bestMove;

        void BestScore(int score, (char piece, int fromRow, int fromCol, int toRow, int toCol, char capture, int score) move) {
            if (score > bestScore)
                (bestScore, bestMove, bestMovesCount) = (score, move, 1);
            else if (score == bestScore && random.Next(++bestMovesCount) == 0)
                bestMove = move;
        }
    }

    public (char piece, int fromRow, int fromCol, int toRow, int toCol, char capture) ParseMove(string cmd) {
        var parts = cmd.Split("-");
        (int row, int col) from = RC(parts[0].Substring(parts[0].Length - 2)), to = RC(parts[1]);
        return (board[from.row, from.col], from.row, from.col, to.row, to.col, board[to.row, to.col]);
    }

    public (bool check, bool checkmate) IsCheck() {
        var check = IsCheck(isWhiteTurn);
        return (check != null, check is { Count: 0 });
    }

    private List<(char piece, int fromRow, int fromCol, int toRow, int toCol, char capture, int score)> IsCheck(bool isWhite) {
        char king = isWhite ? 'k' : 'K';
        bool kingFound = false, inCheck = PossibleMoves(!isWhite).Any(m => m.capture == king);
        if (!inCheck) return null;

        List<(char piece, int fromRow, int fromCol, int toRow, int toCol, char capture, int score)> possibleMoves = new();
        var save = Save();
        foreach (var move in PossibleMoves(isWhite)) {
            MoveInternal(move.fromRow, move.fromCol, move.toRow, move.toCol);
            if (PossibleMoves(!isWhite).All(m => m.capture != king)) possibleMoves.Add(move);
            Restore(save);
        }
        return possibleMoves; // .Count == 0 means checkmate
    }

    public bool IsStalemate() => IsCheck() == (true, false) && !PossibleMoves().Any();

    private static int Value(char piece, int row) => char.ToLower(piece) switch {
        '.' => 0,
        'p' when piece == 'p' && row == 1 || piece == 'P' && row == 6 => 3, // a pawn about to be promoted
        'p' => 1,
        'r' => 5,
        'n' => 3,
        'b' => 3,
        'q' => 9,
        'k' => 1000,
        _ => throw new Exception("unknown piece")
    };

    public int Score() => Score(isWhiteTurn) - Score(!isWhiteTurn);

    private int Score(bool isWhite) {
        int sum = 0;
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
                if (board[r, c] != '.' && isWhite == IsWhite(board[r, c]))
                    sum += Value(board[r, c], r);
        return sum;
    }

    private static string RC(int row, int col) => "" + (char)('a' + col) + (char)('8' - row);
    private static (int row, int col) RC(string rc) => ('8' - rc[1], rc[0] - 'a');
}
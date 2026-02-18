public class Program {
    private static readonly Random random = new(0);

    public static void Main(string[] args) {
        Console.WriteLine("Press '9' for chess 960, press anything else for normal chess.");
        var game = new Game(Console.ReadKey(true).KeyChar == '9');
        string lastWhiteCmd = "best4", lastBlackCmd = "best4";
        bool autoMode = false;
        for (;;) {
            Console.Write(game);
            (bool inCheck, bool checkmate) = game.IsCheck();
            Console.Write("Turn: " + (game.isWhiteTurn ? "white" : "BLACK") + (inCheck ? " (Check)" : "" + " Score: " + game.Score()));
            if (checkmate) {
                Console.WriteLine(" Checkmate!");
                break;
            }
            var moves = game.PossibleMoves().ToList();
            if (moves.Count == 0) {
                Console.WriteLine(" No legal moves!");
                break;
            }
            Console.WriteLine(" Moves: " + string.Join(", ", moves.Select(m => game.MoveString(m.fromRow, m.fromCol, m.toRow, m.toCol))));
            var cmd = autoMode ? "" : Console.ReadLine();
            if (cmd == "") cmd = game.isWhiteTurn ? lastWhiteCmd : lastBlackCmd;
            if (cmd == "auto")
                autoMode = true;
            else if (cmd.StartsWith("best")) {
                _ = game.isWhiteTurn ? lastWhiteCmd = cmd : lastBlackCmd = cmd;
                var move = game.BestMove(random, int.Parse(cmd.Substring(4)));
                Console.WriteLine("Move: " + game.MoveString(move.fromRow, move.fromCol, move.toRow, move.toCol));
                if (!game.Move(move.fromRow, move.fromCol, move.toRow, move.toCol))
                    Console.WriteLine("invalid move?");
            } else if (cmd.Contains("-")) {
                var move = game.ParseMove(cmd);
                if (move.piece == '.' ||
                    Game.IsWhite(move.piece) != game.isWhiteTurn ||
                    (move.capture != '.' && Game.IsWhite(move.capture) == game.isWhiteTurn) ||
                    game.MovePossible(move.fromRow, move.fromCol, move.toRow, move.toCol)) {
                    Console.WriteLine("Invalid move");
                    continue;
                }
                Console.WriteLine("Move: " + cmd + (move.capture == '.' ? "" : "*" + move.capture));
                if (!game.Move(move.fromRow, move.fromCol, move.toRow, move.toCol))
                    Console.WriteLine("invalid move");
            } else if (cmd.Length == 2 && cmd.StartsWith("^")) {
                Console.WriteLine("Pawn promotion set to " + game.SetPromotion(cmd[1]));
            }
        }
    }
}
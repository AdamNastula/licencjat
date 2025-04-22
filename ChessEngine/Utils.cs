namespace ChessEngine;

public class Utils
{
    public static void PrintBitboard(UInt64 bitboard)
    {
        string bitboardString = bitboard.ToString("b8").PadLeft(64, '0');
        string line = String.Empty;
        
        foreach (var bit in bitboardString)
        {
            line += bit;

            if (line.Length == 8)
            {
                Console.WriteLine(line);
                line = String.Empty;
            }
        }
        
        Console.WriteLine();
    }
}
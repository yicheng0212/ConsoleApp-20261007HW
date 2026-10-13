using System.Globalization;
using System.Linq;

namespace ConsoleApp20261013
{
    internal class Program
    {
        private const decimal DecimalSmallestPositiveValue = 0.0000000000000000000000000001m;

        static void Main(string[] args)
        {
            ConsoleColor originalForeground = Console.ForegroundColor;
            ConsoleColor originalBackground = Console.BackgroundColor;
            var rows = new[]
            {
                CreateRow("bool", typeof(bool), sizeof(bool), $"{bool.TrueString} 或 {bool.FalseString}"),
                CreateRow("byte", typeof(byte), sizeof(byte), Format(byte.MinValue, byte.MaxValue)),
                CreateRow("  sbyte\n  (signed byte)", typeof(sbyte), sizeof(sbyte), Format(sbyte.MinValue, sbyte.MaxValue)),
                CreateRow("char", typeof(char), sizeof(char), $"U+{(int)char.MinValue:X4} 至 U+{(int)char.MaxValue:X4}"),
                CreateRow("decimal", typeof(decimal), sizeof(decimal), $"±{ToScientific(DecimalSmallestPositiveValue, 1)} 至 ±{ToScientific(decimal.MaxValue, 4)}"),
                CreateRow("double", typeof(double), sizeof(double), $"±{ToScientific(double.Epsilon, 1)} 至 ±{ToScientific(double.MaxValue, 1)}"),
                CreateRow("float", typeof(float), sizeof(float), $"±{ToScientific(float.Epsilon, 1)} 至 ±{ToScientific(float.MaxValue, 1)}", true),
                CreateRow("int", typeof(int), sizeof(int), Format(int.MinValue, int.MaxValue), true),
                CreateRow("  uint\n  (unsigned int)", typeof(uint), sizeof(uint), Format(uint.MinValue, uint.MaxValue), true),
                CreateRow("nint", typeof(nint), IntPtr.Size, $"取決於平台 (在執行階段計算)：{Format(nint.MinValue, nint.MaxValue)}", true),
                CreateRow("nuint", typeof(nuint), UIntPtr.Size, $"取決於平台 (在執行階段計算)：{Format(nuint.MinValue, nuint.MaxValue)}", true),
                CreateRow("long", typeof(long), sizeof(long), Format(long.MinValue, long.MaxValue)),
                CreateRow("  ulong\n  (unsigned long)", typeof(ulong), sizeof(ulong), Format(ulong.MinValue, ulong.MaxValue)),
                CreateRow("short", typeof(short), sizeof(short), Format(short.MinValue, short.MaxValue)),
                CreateRow("  ushort\n  (unsigned short)", typeof(ushort), sizeof(ushort), Format(ushort.MinValue, ushort.MaxValue), true)
            };

            try
            {
                PrintTable(rows);
            }
            finally
            {
                Console.ForegroundColor = originalForeground;
                Console.BackgroundColor = originalBackground;
            }
        }

        private static TypeRow CreateRow(string csharpType, Type clrType, int size, string range, bool highlight = false)
        {
            string sizeText = $"{size} {(size == 1 ? "Byte" : "Bytes")}";
            return new TypeRow(csharpType, clrType.FullName ?? clrType.Name, sizeText, range, highlight);
        }

        private static string Format<T>(T minValue, T maxValue) where T : IFormattable
        {
            return $"{minValue.ToString("N0", CultureInfo.InvariantCulture)} 至 {maxValue.ToString("N0", CultureInfo.InvariantCulture)}";
        }

        private static string ToScientific<T>(T value, int decimals) where T : IFormattable
        {
            string formatted = value.ToString($"0.{new string('0', decimals)}E+0", CultureInfo.InvariantCulture);
            string[] parts = formatted.Split('E');
            string exponent = string.Concat(parts[1].Select(character => character switch
            {
                '0' => '⁰', '1' => '¹', '2' => '²', '3' => '³', '4' => '⁴',
                '5' => '⁵', '6' => '⁶', '7' => '⁷', '8' => '⁸', '9' => '⁹',
                '-' => '⁻', '+' => '⁺', _ => character
            }));
            return $"{parts[0]} × 10{exponent}";
        }

        private static void PrintTable(TypeRow[] rows)
        {
            ConsoleColor originalForeground = Console.ForegroundColor;
            ConsoleColor originalBackground = Console.BackgroundColor;
            string[] headers = { "C# 型別關鍵字", "大小", "範圍", ".NET 類型" };
            string[][] cells = rows.Select(row => new[] { row.CSharpType, row.Size, row.Range, row.ClrType }).ToArray();
            int[] widths = headers.Select((header, column) => Math.Max(
                DisplayWidth(header),
                cells.Max(row => row[column].Split('\n').Max(DisplayWidth)))).ToArray();

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            WriteBorder(widths, '┌', '┬', '┐', originalForeground, originalBackground);
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            WriteRow(headers, widths, originalForeground, originalBackground);
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            WriteBorder(widths, '├', '┼', '┤', originalForeground, originalBackground);

            for (int i = 0; i < rows.Length; i++)
            {
                Console.BackgroundColor = i % 2 == 0 ? ConsoleColor.Gray : ConsoleColor.White;
                Console.ForegroundColor = rows[i].Highlight ? ConsoleColor.Red : ConsoleColor.Black;
                WriteRow(cells[i], widths, originalForeground, originalBackground);
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            WriteBorder(widths, '└', '┴', '┘', originalForeground, originalBackground);
        }

        private static void WriteBorder(int[] widths, char left, char middle, char right, ConsoleColor originalForeground, ConsoleColor originalBackground)
        {
            ConsoleColor borderForeground = Console.ForegroundColor;
            ConsoleColor borderBackground = Console.BackgroundColor;
            Console.Write(left);
            for (int column = 0; column < widths.Length; column++)
            {
                Console.Write(new string('─', widths[column] + 2));
                Console.Write(column == widths.Length - 1 ? right : middle);
            }

            Console.ForegroundColor = originalForeground;
            Console.BackgroundColor = originalBackground;
            Console.WriteLine();
            Console.ForegroundColor = borderForeground;
            Console.BackgroundColor = borderBackground;
        }

        private static void WriteRow(string[] values, int[] widths, ConsoleColor originalForeground, ConsoleColor originalBackground)
        {
            ConsoleColor rowForeground = Console.ForegroundColor;
            ConsoleColor rowBackground = Console.BackgroundColor;
            string[][] lines = values.Select(value => value.Split('\n')).ToArray();
            int lineCount = lines.Max(column => column.Length);

            for (int line = 0; line < lineCount; line++)
            {
                Console.ForegroundColor = rowForeground;
                Console.BackgroundColor = rowBackground;
                Console.Write('│');
                for (int column = 0; column < values.Length; column++)
                {
                    string value = line < lines[column].Length ? lines[column][line] : string.Empty;
                    Console.Write($" {PadRight(value, widths[column])} │");
                }

                Console.ForegroundColor = originalForeground;
                Console.BackgroundColor = originalBackground;
                Console.WriteLine();
            }

            Console.ForegroundColor = rowForeground;
            Console.BackgroundColor = rowBackground;
        }

        private static string PadRight(string value, int width)
        {
            return value + new string(' ', width - DisplayWidth(value));
        }

        private static int DisplayWidth(string value)
        {
            return value.Sum(character => IsWide(character) ? 2 : 1);
        }

        private static bool IsWide(char character)
        {
            return character is >= '\u2E80' and <= '\uA4CF'
                or >= '\uAC00' and <= '\uD7AF'
                or >= '\uF900' and <= '\uFAFF'
                or >= '\uFE30' and <= '\uFE6F'
                or >= '\uFF01' and <= '\uFF60';
        }

        private sealed record TypeRow(string CSharpType, string ClrType, string Size, string Range, bool Highlight);
    }
}

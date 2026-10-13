
using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ConsoleApp20261013
{
    internal class Program
    {
        // decimal 可以表示的最小正數
        private const decimal DecimalSmallestPositiveValue =
            0.0000000000000000000000000001m;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            // 儲存原本的主控台顏色
            ConsoleColor originalForeground = Console.ForegroundColor;
            ConsoleColor originalBackground = Console.BackgroundColor;

            // 建立各種 C# 資料型別的資訊
            var rows = new[]
            {
                CreateRow("bool", typeof(bool), sizeof(bool), "True 或 False"),
                CreateRow("byte", typeof(byte), sizeof(byte), Format(byte.MinValue, byte.MaxValue)),
                CreateRow("  sbyte\n  (signed byte)", typeof(sbyte), sizeof(sbyte), Format(sbyte.MinValue, sbyte.MaxValue)),
                CreateRow("char", typeof(char), sizeof(char), $"U+{(int)char.MinValue:X4} 至 U+{(int)char.MaxValue:X4}"),
                CreateRow("decimal", typeof(decimal), sizeof(decimal), $"0、±{ToScientific(DecimalSmallestPositiveValue, 1)} 至 ±{ToScientific(decimal.MaxValue, 4)}"),
                CreateRow("double", typeof(double), sizeof(double), $"0、±{ToScientific(double.Epsilon, 1)} 至 ±{ToScientific(double.MaxValue, 1)}"),
                CreateRow("float", typeof(float), sizeof(float), $"0、±{ToScientific(float.Epsilon, 1)} 至 ±{ToScientific(float.MaxValue, 1)}", true),
                CreateRow("int", typeof(int), sizeof(int), Format(int.MinValue, int.MaxValue), true),
                CreateRow("  uint\n  (unsigned int)", typeof(uint), sizeof(uint), Format(uint.MinValue, uint.MaxValue), true),
                CreateRow("nint", typeof(nint), IntPtr.Size, $"取決於平台：{Format(nint.MinValue, nint.MaxValue)}", true),
                CreateRow("nuint", typeof(nuint), UIntPtr.Size, $"取決於平台：{Format(nuint.MinValue, nuint.MaxValue)}", true),
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
                // 程式結束時恢復原本顏色
                Console.ForegroundColor = originalForeground;
                Console.BackgroundColor = originalBackground;
            }
        }

        // 建立一筆資料型別的資訊
        private static TypeRow CreateRow(
            string csharpType, Type clrType, int size,
            string range, bool highlight = false)
        {
            string sizeText = $"{size} {(size == 1 ? "Byte" : "Bytes")}";

            return new TypeRow(
                csharpType,
                clrType.FullName ?? clrType.Name,
                sizeText,
                range,
                highlight);
        }

        // 將數值加上千分位
        private static string Format<T>(T minValue, T maxValue)
            where T : IFormattable
        {
            return $"{minValue.ToString("N0", CultureInfo.InvariantCulture)} 至 " +
                   $"{maxValue.ToString("N0", CultureInfo.InvariantCulture)}";
        }

        // 將數字轉換成科學記號
        private static string ToScientific<T>(T value, int decimals)
            where T : IFormattable
        {
            string formatted = value.ToString(
                $"0.{new string('0', decimals)}E+0",
                CultureInfo.InvariantCulture);

            string[] parts = formatted.Split('E');

            // 將指數轉換成上標數字
            string exponent = string.Concat(
                parts[1].Select(c => c switch
                {
                    '0' => '⁰',
                    '1' => '¹',
                    '2' => '²',
                    '3' => '³',
                    '4' => '⁴',
                    '5' => '⁵',
                    '6' => '⁶',
                    '7' => '⁷',
                    '8' => '⁸',
                    '9' => '⁹',
                    '-' => '⁻',
                    '+' => '⁺',
                    _ => c
                }));

            return $"{parts[0]} × 10{exponent}";
        }

        // 印出完整表格
        private static void PrintTable(TypeRow[] rows)
        {
            string[] headers =
            {
                "C# 型別關鍵字",
                "大小",
                "範圍",
                ".NET 類型"
            };

            // 將資料整理成二維表格
            string[][] cells = rows.Select(row => new[]
            {
                row.CSharpType,
                row.Size,
                row.Range,
                row.ClrType
            }).ToArray();

            // 自動計算每個欄位需要的寬度
            int[] widths = headers.Select((header, column) =>
                Math.Max(
                    DisplayWidth(header),
                    cells.Max(row =>
                        row[column].Split('\n').Max(DisplayWidth))
                )).ToArray();

            // 計算表格總寬度
            int tableWidth = 1;

            foreach (int width in widths)
            {
                tableWidth += width + 3;
            }

            // 印出表格上方邊框
            WriteBorder(widths, '┌', '┬', '┐');

            // 印出標題
            WriteRow(headers, widths,
                ConsoleColor.White, ConsoleColor.DarkYellow);

            // 印出標題分隔線
            WriteBorder(widths, '├', '┼', '┤');

            // 印出所有資料列
            for (int i = 0; i < rows.Length; i++)
            {
                // 灰色和白色交錯
                ConsoleColor background =
                    i % 2 == 0
                        ? ConsoleColor.Gray
                        : ConsoleColor.White;

                // 特別標記的型別使用紅色
                ConsoleColor foreground =
                    rows[i].Highlight
                        ? ConsoleColor.Red
                        : ConsoleColor.Black;

                WriteRow(cells[i], widths, foreground, background);
            }

            // 印出表格下方邊框
            WriteBorder(widths, '└', '┴', '┘');

            Console.ResetColor();
        }

        // 印出表格邊框
        private static void WriteBorder(
            int[] widths, char left, char middle, char right)
        {
            StringBuilder line = new StringBuilder();

            line.Append(left);

            for (int i = 0; i < widths.Length; i++)
            {
                line.Append('─', widths[i] + 2);

                if (i == widths.Length - 1)
                    line.Append(right);
                else
                    line.Append(middle);
            }

            WriteColoredLine(
                line.ToString(),
                ConsoleColor.White,
                ConsoleColor.DarkYellow);
        }

        // 印出一整列資料，支援同一格內換行
        private static void WriteRow(
            string[] values, int[] widths,
            ConsoleColor foreground, ConsoleColor background)
        {
            string[][] lines = values
                .Select(value => value.Split('\n'))
                .ToArray();

            int lineCount = lines.Max(column => column.Length);

            for (int line = 0; line < lineCount; line++)
            {
                StringBuilder output = new StringBuilder();

                output.Append('│');

                for (int column = 0; column < values.Length; column++)
                {
                    string value =
                        line < lines[column].Length
                            ? lines[column][line]
                            : "";

                    output.Append(' ');
                    output.Append(PadRight(value, widths[column]));
                    output.Append(" │");
                }

                WriteColoredLine(
                    output.ToString(),
                    foreground,
                    background);
            }
        }

        // 只對實際表格文字設定背景顏色
        private static void WriteColoredLine(
            string text, ConsoleColor foreground,
            ConsoleColor background)
        {
            // 使用 ANSI 色彩控制碼
            // 每一列開始設定顏色，結束立即重設
            string foregroundCode = foreground switch
            {
                ConsoleColor.White => "97",
                ConsoleColor.Red => "91",
                ConsoleColor.Black => "30",
                _ => "39"
            };

            string backgroundCode = background switch
            {
                ConsoleColor.DarkYellow => "43",
                ConsoleColor.Gray => "47",
                ConsoleColor.White => "107",
                _ => "49"
            };

            // ESC[ 是 ANSI 控制序列的開頭
            string startColor =
                $"\u001b[{foregroundCode};{backgroundCode}m";

            // 重設文字和背景顏色
            string resetColor = "\u001b[0m";

            // 一次輸出完整文字，重設顏色後才換行
            Console.Write(startColor + text + resetColor + "\n");
        }

        // 補空白讓欄位對齊
        private static string PadRight(string value, int width)
        {
            return value + new string(
                ' ',
                Math.Max(0, width - DisplayWidth(value)));
        }

        // 計算文字在主控台中的顯示寬度
        private static int DisplayWidth(string value)
        {
            int width = 0;

            foreach (char c in value)
            {
                if (IsWide(c))
                    width += 2;
                else
                    width++;
            }

            return width;
        }

        // 判斷是否為常見的全形字元
        private static bool IsWide(char c)
        {
            return c is >= '\u2E80' and <= '\uA4CF'
                or >= '\uAC00' and <= '\uD7AF'
                or >= '\uF900' and <= '\uFAFF'
                or >= '\uFE30' and <= '\uFE6F'
                or >= '\uFF01' and <= '\uFF60';
        }

        // 儲存每一筆型別資料
        private sealed record TypeRow(
            string CSharpType,
            string ClrType,
            string Size,
            string Range,
            bool Highlight);
    }
}

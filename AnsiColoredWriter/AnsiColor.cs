namespace AnsiColoredWriters;

public readonly struct AnsiColor : IEquatable<AnsiColor>
{
    private readonly string internalColor = "";
    private AnsiColor(string internalColor)
    {
        this.internalColor = internalColor;
    }

    public override string ToString()
    {
        return internalColor;
    }

    public static AnsiColor operator +(AnsiColor left, AnsiColor right)
    {
        return Add(left, right);
    }

    public static implicit operator string(AnsiColor color)
    {
        return color.internalColor;
    }

    // Reset
    public static readonly AnsiColor Empty = new("");

    // Reset
    public static readonly AnsiColor Reset = new("\u001b[0m");

    // Text colors
    public static readonly AnsiColor Black = new("\u001b[30m");
    public static readonly AnsiColor Red = new("\u001b[31m");
    public static readonly AnsiColor Green = new("\u001b[32m");
    public static readonly AnsiColor Yellow = new("\u001b[33m");
    public static readonly AnsiColor Blue = new("\u001b[34m");
    public static readonly AnsiColor Magenta = new("\u001b[35m");
    public static readonly AnsiColor Cyan = new("\u001b[36m");
    public static readonly AnsiColor White = new("\u001b[37m");
    public static readonly AnsiColor BrightBlack = new("\u001b[90m");
    public static readonly AnsiColor BrightRed = new("\u001b[91m");
    public static readonly AnsiColor BrightGreen = new("\u001b[92m");
    public static readonly AnsiColor BrightYellow = new("\u001b[93m");
    public static readonly AnsiColor BrightBlue = new("\u001b[94m");
    public static readonly AnsiColor BrightMagenta = new("\u001b[95m");
    public static readonly AnsiColor BrightCyan = new("\u001b[96m");
    public static readonly AnsiColor BrightWhite = new("\u001b[97m");

    // Background colors
    public static readonly AnsiColor BgBlack = new("\u001b[40m");
    public static readonly AnsiColor BgRed = new("\u001b[41m");
    public static readonly AnsiColor BgGreen = new("\u001b[42m");
    public static readonly AnsiColor BgYellow = new("\u001b[43m");
    public static readonly AnsiColor BgBlue = new("\u001b[44m");
    public static readonly AnsiColor BgMagenta = new("\u001b[45m");
    public static readonly AnsiColor BgCyan = new("\u001b[46m");
    public static readonly AnsiColor BgWhite = new("\u001b[47m");
    public static readonly AnsiColor BgBrightBlack = new("\u001b[100m");
    public static readonly AnsiColor BgBrightRed = new("\u001b[101m");
    public static readonly AnsiColor BgBrightGreen = new("\u001b[102m");
    public static readonly AnsiColor BgBrightYellow = new("\u001b[103m");
    public static readonly AnsiColor BgBrightBlue = new("\u001b[104m");
    public static readonly AnsiColor BgBrightMagenta = new("\u001b[105m");
    public static readonly AnsiColor BgBrightCyan = new("\u001b[106m");
    public static readonly AnsiColor BgBrightWhite = new("\u001b[107m");

    // Text styles
    public static readonly AnsiColor Bold = new("\u001b[1m");
    public static readonly AnsiColor Dim = new("\u001b[2m");
    public static readonly AnsiColor Italic = new("\u001b[3m");
    public static readonly AnsiColor Underline = new("\u001b[4m");
    public static readonly AnsiColor Blink = new("\u001b[5m");
    public static readonly AnsiColor Reverse = new("\u001b[7m");
    public static readonly AnsiColor Hidden = new("\u001b[8m");
    public static readonly AnsiColor Strikethrough = new("\u001b[9m");

    // Helper methods
    public static AnsiColor RGBForeground(int r, int g, int b)
    {
        return new AnsiColor($"\u001b[38;2;{r};{g};{b}m");
    }

    public static AnsiColor RGBBackground(int r, int g, int b)
    {
        return new AnsiColor($"\u001b[48;2;{r};{g};{b}m");
    }

    public static AnsiColor Color256Foreground(byte colorCode)
    {
        return new AnsiColor($"\u001b[38;5;{colorCode}m");
    }

    public static AnsiColor Color256Background(byte colorCode)
    {
        return new AnsiColor($"\u001b[48;5;{colorCode}m");
    }

    public static AnsiColor Add(AnsiColor left, AnsiColor right)
    {
        return new AnsiColor(left.internalColor + right.internalColor);
    }

    public override bool Equals(object? obj)
    {
        return obj is AnsiColor color && Equals(color);
    }

    public override int GetHashCode()
    {
        return internalColor.GetHashCode();
    }

    public static bool operator ==(AnsiColor left, AnsiColor right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AnsiColor left, AnsiColor right)
    {
        return !left.Equals(right);
    }

    public bool Equals(AnsiColor other)
    {
        return internalColor.Equals(other.internalColor);
    }
}
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
    public static readonly AnsiColor Empty = new AnsiColor("");

    // Reset
    public static readonly AnsiColor Reset = new AnsiColor("\u001b[0m");

    // Text colors
    public static readonly AnsiColor Black = new AnsiColor("\u001b[30m");
    public static readonly AnsiColor Red = new AnsiColor("\u001b[31m");
    public static readonly AnsiColor Green = new AnsiColor("\u001b[32m");
    public static readonly AnsiColor Yellow = new AnsiColor("\u001b[33m");
    public static readonly AnsiColor Blue = new AnsiColor("\u001b[34m");
    public static readonly AnsiColor Magenta = new AnsiColor("\u001b[35m");
    public static readonly AnsiColor Cyan = new AnsiColor("\u001b[36m");
    public static readonly AnsiColor White = new AnsiColor("\u001b[37m");
    public static readonly AnsiColor BrightBlack = new AnsiColor("\u001b[90m");
    public static readonly AnsiColor BrightRed = new AnsiColor("\u001b[91m");
    public static readonly AnsiColor BrightGreen = new AnsiColor("\u001b[92m");
    public static readonly AnsiColor BrightYellow = new AnsiColor("\u001b[93m");
    public static readonly AnsiColor BrightBlue = new AnsiColor("\u001b[94m");
    public static readonly AnsiColor BrightMagenta = new AnsiColor("\u001b[95m");
    public static readonly AnsiColor BrightCyan = new AnsiColor("\u001b[96m");
    public static readonly AnsiColor BrightWhite = new AnsiColor("\u001b[97m");

    // Background colors
    public static readonly AnsiColor BgBlack = new AnsiColor("\u001b[40m");
    public static readonly AnsiColor BgRed = new AnsiColor("\u001b[41m");
    public static readonly AnsiColor BgGreen = new AnsiColor("\u001b[42m");
    public static readonly AnsiColor BgYellow = new AnsiColor("\u001b[43m");
    public static readonly AnsiColor BgBlue = new AnsiColor("\u001b[44m");
    public static readonly AnsiColor BgMagenta = new AnsiColor("\u001b[45m");
    public static readonly AnsiColor BgCyan = new AnsiColor("\u001b[46m");
    public static readonly AnsiColor BgWhite = new AnsiColor("\u001b[47m");
    public static readonly AnsiColor BgBrightBlack = new AnsiColor("\u001b[100m");
    public static readonly AnsiColor BgBrightRed = new AnsiColor("\u001b[101m");
    public static readonly AnsiColor BgBrightGreen = new AnsiColor("\u001b[102m");
    public static readonly AnsiColor BgBrightYellow = new AnsiColor("\u001b[103m");
    public static readonly AnsiColor BgBrightBlue = new AnsiColor("\u001b[104m");
    public static readonly AnsiColor BgBrightMagenta = new AnsiColor("\u001b[105m");
    public static readonly AnsiColor BgBrightCyan = new AnsiColor("\u001b[106m");
    public static readonly AnsiColor BgBrightWhite = new AnsiColor("\u001b[107m");

    // Text styles
    public static readonly AnsiColor Bold = new AnsiColor("\u001b[1m");
    public static readonly AnsiColor Dim = new AnsiColor("\u001b[2m");
    public static readonly AnsiColor Italic = new AnsiColor("\u001b[3m");
    public static readonly AnsiColor Underline = new AnsiColor("\u001b[4m");
    public static readonly AnsiColor Blink = new AnsiColor("\u001b[5m");
    public static readonly AnsiColor Reverse = new AnsiColor("\u001b[7m");
    public static readonly AnsiColor Hidden = new AnsiColor("\u001b[8m");
    public static readonly AnsiColor Strikethrough = new AnsiColor("\u001b[9m");

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
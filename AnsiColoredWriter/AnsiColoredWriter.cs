using System.Diagnostics.CodeAnalysis;

namespace AnsiColoredWriters;

public sealed class AnsiColoredWriter
{
    public required AnsiColor Style { get; set; }

    private readonly TextWriter textWriter;

    [SetsRequiredMembers]
    public AnsiColoredWriter(TextWriter textWriter, AnsiColor style)
    {
        this.textWriter = textWriter;
        Style = style;
    }

    public void WriteLine(string? message)
    {
        textWriter.WriteLine($"{Style}{message}{AnsiColor.Reset}");
    }

    public void Write(string? message)
    {
        textWriter.Write($"{Style}{message}{AnsiColor.Reset}");
    }

    public async Task WriteLineAsync(string? message)
    {
        await textWriter.WriteLineAsync($"{Style}{message}{AnsiColor.Reset}");
    }

    public async Task WriteAsync(string? message)
    {
        await textWriter.WriteAsync($"{Style}{message}{AnsiColor.Reset}");
    }
}

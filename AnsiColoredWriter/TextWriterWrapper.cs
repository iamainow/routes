namespace AnsiColoredWriters;

public sealed class TextWriterWrapper : ITextWriterWrapper
{
    private readonly TextWriter textWriter;
    public TextWriterWrapper(TextWriter textWriter)
    {
        this.textWriter = textWriter;
    }
    public void Write(string? message)
    {
        textWriter.Write(message);
    }
    public void WriteLine(string? message)
    {
        textWriter.WriteLine(message);
    }
    public async Task WriteAsync(string? message)
    {
        await textWriter.WriteAsync(message);
    }
    public async Task WriteLineAsync(string? message)
    {
        await textWriter.WriteLineAsync(message);
    }
}
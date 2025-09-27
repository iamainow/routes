namespace AnsiColoredWriters;

public interface ITextWriterWrapper
{
    void Write(string? message);
    void WriteLine(string? message);
    Task WriteAsync(string? message);
    Task WriteLineAsync(string? message);
}

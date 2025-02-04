using System.Reactive.Subjects;

namespace Webion.IIS.Cli.Helpers.Progress;

public sealed class ProgressStream : Stream
{
    private readonly Subject<ProgressStreamEvent> _onRead = new();
    private readonly Stream _stream;

    public ProgressStream(Stream stream)
    {
        _stream = stream;
    }

    public IObservable<ProgressStreamEvent> OnRead => _onRead;
    
    
    public override void Flush()
    {
        _stream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        _onRead.OnNext(new ProgressStreamEvent(count));
        return _stream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _stream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _stream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _stream.Write(buffer, offset, count);
    }

    public override bool CanRead => _stream.CanRead;

    public override bool CanSeek => _stream.CanSeek;

    public override bool CanWrite => _stream.CanWrite;

    public override long Length => _stream.Length;

    public override long Position
    {
        get => _stream.Position;
        set => _stream.Position = value;
    }
}
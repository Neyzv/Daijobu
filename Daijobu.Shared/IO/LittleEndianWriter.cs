using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace Daijobu.Shared.IO;

public class LittleEndianWriter
    : IBinaryWriter, IDisposable
{
    private bool _isBufferRented;
    private byte[] _buffer = Array.Empty<byte>();
    private int _maxPosition;

    public int Length =>
        _buffer.Length;
    
    private int _position;
    public int Position
    {
        get => _position;
        private set
        {
            _position = value;

            if (_maxPosition < value)
                _maxPosition = value;
        }
    }

    public int BytesAvailable =>
        Length - Position;
    
    private Span<byte> GetSpan(int count)
    {
        CheckAndResizeBuffer(count);

        var span = _buffer.AsSpan(Position, count);

        Position += count;

        return span;
    }

    private void CheckAndResizeBuffer(int count, int? position = null)
    {
        position ??= Position;
        
        var bytesAvailable = Length - position.Value;
        if (count <= bytesAvailable)
            return;

        var currentCount = Length;
        var growBy = Math.Max(count, currentCount);

        if (count is 0)
            growBy = Math.Max(growBy, 256);

        var newCount = currentCount + growBy;

        if ((uint)newCount > int.MaxValue)
        {
            var needed = (uint)(currentCount - bytesAvailable + count);

            if (needed > Array.MaxLength)
                throw new Exception("The requested operation would exceed the maximum array length.");

            newCount = Array.MaxLength;
        }

        var newArray = ArrayPool<byte>.Shared.Rent(newCount);
        Array.Copy(_buffer, newArray, _buffer.Length);

        if (_isBufferRented)
            ArrayPool<byte>.Shared.Return(_buffer);

        _buffer = newArray;
        _isBufferRented = true;
    }

    public Memory<byte> BufferAsMemory() =>
        _buffer.AsMemory(0, _maxPosition);
    
    public Span<byte> BufferAsSpan() =>
        _buffer.AsSpan(0, _maxPosition);

    public byte[] BufferAsArray() =>
        _buffer.AsSpan(0, _maxPosition).ToArray();
    
    public void WriteByte(byte value) =>
        GetSpan(sizeof(byte))[0] = value;

    public void WriteSByte(sbyte value) =>
        GetSpan(sizeof(sbyte))[0] = (byte)value;

    public void WriteBool(bool value) =>
        GetSpan(sizeof(bool))[0] = (byte)(value ? 1 : 0);

    public void WriteUShort(ushort value) =>
        BinaryPrimitives.WriteUInt16BigEndian(GetSpan(sizeof(ushort)), value);

    public void WriteShort(short value) =>
        BinaryPrimitives.WriteInt16BigEndian(GetSpan(sizeof(short)), value);

    public void WriteUInt(uint value) =>
        BinaryPrimitives.WriteUInt32BigEndian(GetSpan(sizeof(uint)), value);

    public void WriteInt(int value) =>
        BinaryPrimitives.WriteInt32BigEndian(GetSpan(sizeof(int)), value);

    public void WriteULong(ulong value) =>
        BinaryPrimitives.WriteUInt64BigEndian(GetSpan(sizeof(ulong)), value);

    public void WriteLong(long value) =>
        BinaryPrimitives.WriteInt64BigEndian(GetSpan(sizeof(long)), value);

    public void WriteFloat(float value) =>
        BinaryPrimitives.WriteSingleBigEndian(GetSpan(sizeof(float)), value);

    public void WriteDouble(double value) =>
        BinaryPrimitives.WriteDoubleBigEndian(GetSpan(sizeof(double)), value);

    public void WriteMemory(Memory<byte> value) =>
        WriteSpan(value.Span);

    public void WriteSpan(Span<byte> value) =>
        value.CopyTo(GetSpan(value.Length));

    public void WriteStringBytes(string value) =>
        WriteSpan(Encoding.UTF8.GetBytes(value));

    public void WriteUtfBytes(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);

        WriteUShort((ushort)bytes.Length);
        WriteSpan(bytes);
    }

    public void Seek(int offset, SeekOrigin origin = SeekOrigin.Begin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                CheckAndResizeBuffer(offset, offset);
                Position = offset;
                break;
            case SeekOrigin.Current:
                CheckAndResizeBuffer(offset);
                Position += offset;
                break;
            case SeekOrigin.End:
                Position = Length - Math.Abs(offset);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(origin));
        }
    }

    public void Dispose()
    {
        if (_isBufferRented)
        {
            ArrayPool<byte>.Shared.Return(_buffer);
            _isBufferRented = false;
        }
    }
}
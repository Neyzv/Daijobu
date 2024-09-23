using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace Daijobu.Shared.IO;

public sealed class LittleEndianReader
    : IBinaryReader
{
    private readonly ReadOnlyMemory<byte> _buffer;

    public int Length =>
        _buffer.Length;
    
    public int Position { get; private set; }

    public int BytesAvailable =>
        Length - Position;

    public LittleEndianReader(ReadOnlyMemory<byte> buffer) =>
        _buffer = buffer;

    public LittleEndianReader(ReadOnlySequence<byte> buffer) =>
        _buffer = SequenceMarshal.TryGetReadOnlyMemory(buffer, out var memory) ? memory : buffer.ToArray();
    
    public LittleEndianReader(Stream stream)
    {
        if (stream is not MemoryStream ms)
        {
            ms = new MemoryStream();
            stream.CopyTo(ms);
        }

        _buffer = ms.ToArray();
    }

    public byte ReadByte() =>
        ReadSpan(sizeof(byte))[0];

    public sbyte ReadSByte() =>
        (sbyte)ReadSpan(sizeof(sbyte))[0];

    public bool ReadBool() =>
        ReadSpan(sizeof(byte))[0] is not 0;

    public ushort ReadUShort() =>
        BinaryPrimitives.ReadUInt16LittleEndian(ReadSpan(sizeof(ushort)));

    public short ReadShort() =>
        BinaryPrimitives.ReadInt16LittleEndian(ReadSpan(sizeof(short)));

    public uint ReadUInt() =>
        BinaryPrimitives.ReadUInt32LittleEndian(ReadSpan(sizeof(uint)));

    public int ReadInt() =>
        BinaryPrimitives.ReadInt32LittleEndian(ReadSpan(sizeof(int)));

    public ulong ReadULong() =>
        BinaryPrimitives.ReadUInt64LittleEndian(ReadSpan(sizeof(ulong)));

    public long ReadLong() =>
        BinaryPrimitives.ReadInt64LittleEndian(ReadSpan(sizeof(long)));
    
    public float ReadFloat() =>
        BinaryPrimitives.ReadSingleBigEndian(ReadSpan(sizeof(float)));

    public double ReadDouble() =>
        BinaryPrimitives.ReadDoubleBigEndian(ReadSpan(sizeof(double)));

    public ReadOnlyMemory<byte> ReadMemory(int count)
    {
        var memory = _buffer.Slice(Position, count);
        Position += count;
        
        return memory;
    }

    public ReadOnlySpan<byte> ReadSpan(int count) =>
        ReadMemory(count).Span;

    public string ReadStringBytes(int count) =>
        Encoding.UTF8.GetString(ReadSpan(count));

    public string ReadUtfBytes()=>
        ReadStringBytes(ReadUShort());

    public void Seek(int offset, SeekOrigin origin = SeekOrigin.Begin) =>
        Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => _buffer.Length - offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin)),
        };
}
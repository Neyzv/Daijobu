namespace Daijobu.Shared.IO;

public interface IBinaryWriter
{
    int Length { get; }

    int Position { get; }

    int BytesAvailable { get; }

    Memory<byte> BufferAsMemory();

    Span<byte> BufferAsSpan();

    byte[] BufferAsArray();

    void WriteByte(byte value);

    void WriteSByte(sbyte value);

    void WriteBool(bool value);

    void WriteUShort(ushort value);

    void WriteShort(short value);

    void WriteUInt(uint value);

    void WriteInt(int value);

    void WriteULong(ulong value);

    void WriteLong(long value);

    void WriteFloat(float value);

    void WriteDouble(double value);

    void WriteMemory(Memory<byte> value);

    void WriteSpan(Span<byte> value);

    void WriteStringBytes(string value);

    void WriteUtfBytes(string value);

    void Seek(int offset, SeekOrigin origin = SeekOrigin.Begin);
}
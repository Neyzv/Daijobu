namespace Daijobu.Shared.IO;

public interface IBinaryReader
{
    int Length { get; }

    int Position { get; }

    int BytesAvailable { get; }

    byte ReadByte();

    sbyte ReadSByte();

    bool ReadBool();

    ushort ReadUShort();

    short ReadShort();

    uint ReadUInt();

    int ReadInt();

    ulong ReadULong();

    long ReadLong();

    float ReadFloat();

    double ReadDouble();

    ReadOnlyMemory<byte> ReadMemory(int count);

    ReadOnlySpan<byte> ReadSpan(int count);

    string ReadStringBytes(int count);

    string ReadUtfBytes();

    void Seek(int offset, SeekOrigin origin = SeekOrigin.Begin);
}
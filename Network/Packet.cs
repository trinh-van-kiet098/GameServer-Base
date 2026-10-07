using System.Buffers.Binary;
using System.Text;

namespace GameServer.Network;

public sealed class Packet
{
    public ushort Opcode { get; }
    public byte[] Payload { get; }

    public Packet(ushort opcode, byte[] payload)
    {
        Opcode = opcode;
        Payload = payload;
    }

    public Packet(Opcode opcode, byte[] payload) : this((ushort)opcode, payload) { }

    public byte[] Serialize()
    {
        var buf = new byte[6 + Payload.Length];
        BinaryPrimitives.WriteInt32LittleEndian(buf.AsSpan(0, 4), 6 + Payload.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(buf.AsSpan(4, 2), Opcode);
        Payload.CopyTo(buf, 6);
        return buf;
    }

    public static Packet String(Opcode op, string s)
        => new(op, Encoding.UTF8.GetBytes(s));

    public static Packet Int(Opcode op, int v)
    {
        var b = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(b, v);
        return new Packet(op, b);
    }

    public static Packet Empty(Opcode op) => new(op, Array.Empty<byte>());

    public string ReadString() => Encoding.UTF8.GetString(Payload);
    public int ReadInt() => Payload.Length >= 4
        ? BinaryPrimitives.ReadInt32LittleEndian(Payload)
        : 0;
}
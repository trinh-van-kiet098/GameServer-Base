using System;
using System.Collections.Generic;


public static class RoomEnterStatus
{
    public const byte Success = 0;
    public const byte Full = 1;
    public const byte NotFound = 2;
}

public static class PacketOpcode
{
    public const ushort Unknown = 0x0000;
    public const ushort Connect = 0x0001;
    public const ushort CreateAccount = 0x0102;
    public const ushort CreateAccountResponse = 0x0202;


    public const ushort Handshake = 0x0010;
    public const ushort Disconnect = 0x0002;
    public const ushort ServerBusy = 0x0003;
    public const ushort Login = 0x0101;
    public const ushort Logout = 0x0103;
    public const ushort LoginResponse = 0x0104;
    public const ushort UserState = 0x0105;
    public const ushort UserJoined = 0x0106;
    public const ushort UserLeft = 0x0107;
    public const ushort Chat = 0x0108;
    public const ushort Move = 0x0201;
    public const ushort FoundRoom = 0x0301;
    public const ushort IntoRoom = 0x0302;
    public const ushort CreateRoom = 0x0303;
    public const ushort ChangeRoom = 0x0304;
    public const ushort OutRoom = 0x0305;
    public const ushort RoomListEntry = 0x0306;
    public const ushort RoomChanged = 0x0307;
    public const ushort RoomEnterResult = 0x0308;
}
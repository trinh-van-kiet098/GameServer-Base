namespace GameServer.Network;

public enum Opcode : ushort
{
    C2S_Login      = 1001,
    C2S_Register   = 1002,
    C2S_Move       = 1003,
    C2S_AttackBoss = 1004,
    C2S_BuyItem    = 1005,
    C2S_JoinRoom   = 1006,
    C2S_RoomAction = 1007,

    S2C_LoginOk      = 2001,
    S2C_LoginFail    = 2002,
    S2C_BossSpawn    = 2003,
    S2C_MatchStart   = 2004,
    S2C_MatchResult  = 2005,
    S2C_BuyOk        = 2006,
    S2C_BuyFail      = 2007,
    S2C_PlayerJoined = 2008,
    S2C_Notify       = 2009,
}
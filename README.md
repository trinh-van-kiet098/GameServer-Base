# GameServer (.NET 10)

## Run
    dotnet run

Server listen o :9000.

## AT
- Thread-per-Map (4 map) : Core/GameLoopThread + World/Map
- Thread-per-Room (dong) : World/Room
- Service Pool           : Services/ (Auth, Shop) tren ThreadPool
- Network IO             : async socket, outbound queue rieng

## Opcode
- 1001 C2S_Login (string user)
- 1002 C2S_Register
- 1003 C2S_Move
- 1004 C2S_AttackBoss (int bossId)
- 1005 C2S_BuyItem (int itemId)
- 1006 C2S_JoinRoom
- 1007 C2S_RoomAction

## Packet format
    [4B length][2B opcode][payload]
    length = 6 + payload.Length

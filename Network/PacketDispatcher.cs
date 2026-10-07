using GameServer.Core;
using GameServer.Services;
using GameServer.World.Map;
using GameServer.World.Room;

namespace GameServer.Network;

public sealed class PacketDispatcher
{
    private readonly MapManager _maps;
    private readonly RoomManager _rooms;
    private readonly AuthService _auth;
    private readonly ShopService _shop;

    public PacketDispatcher(
        MapManager maps,
        RoomManager rooms,
        AuthService auth,
        ShopService shop,
        SessionManager sessions)
    {
        _maps = maps;
        _rooms = rooms;
        _auth = auth;
        _shop = shop;
    }

    public void Dispatch(ClientSession session, Packet p)
    {
        var op = (Opcode)p.Opcode;
        switch (op)
        {
            case Opcode.C2S_Login:
                _ = _auth.LoginAsync(p.ReadString(), "x", session);
                break;

            case Opcode.C2S_Register:
                _ = _auth.RegisterAsync(p.ReadString(), "x", session);
                break;

            case Opcode.C2S_BuyItem:
                if (session.Player != null)
                    _shop.BuyItem(session.Player, p.ReadInt(), 100);
                break;

            case Opcode.C2S_Move:
            {
                var map = _maps.Get(session.CurrentMapId == 0 ? 1 : session.CurrentMapId);
                session.CurrentMapId = map.Map.MapId;
                map.Post(() =>
                {
                    if (session.Player != null)
                    {
                        if (!map.Map.HasPlayer(session.Player))
                            map.Map.AddPlayer(session.Player);
                        session.Player.Move(0, 0);
                    }
                });
                break;
            }

            case Opcode.C2S_AttackBoss:
            {
                var map = _maps.Get(session.CurrentMapId == 0 ? 1 : session.CurrentMapId);
                var bossId = p.ReadInt();
                map.Post(() => session.Player?.AttackBoss(bossId));
                break;
            }

            case Opcode.C2S_JoinRoom:
            {
                if (session.Player == null) break;
                var room = _rooms.CreateRoom(maxSlots: 2);
                room.AddPlayer(session.Player);
                session.CurrentRoomId = room.RoomId;
                break;
            }

            case Opcode.C2S_RoomAction:
            {
                if (session.CurrentRoomId is not { } rid) break;
                var rt = _rooms.GetThread(rid);
                if (rt == null) break;
                rt.Post(() => rt.Room.HandleAction(session.Player!, p));
                break;
            }

            default:
                Logger.Warn($"unknown opcode {op}");
                break;
        }
    }
}
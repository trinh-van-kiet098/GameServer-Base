using GameServer.Core;
using GameServer.Network;
using GameServer.World.Entity;

namespace GameServer.Services;

public sealed class ShopService
{
    private readonly DbWorker _db;
    public ShopService(DbWorker db) { _db = db; }

    public void BuyItem(Player player, int itemId, int price)
    {
        Task.Run(() =>
        {
            if (player.Gold < price)
            {
                player.Session.Send(Packet.Empty(Opcode.S2C_BuyFail));
                return;
            }

            player.Gold -= price;
            player.Inventory.Add(itemId);

            _db.Enqueue(() => Task.CompletedTask);

            player.Session.Send(Packet.Int(Opcode.S2C_BuyOk, itemId));
            Logger.Info($"buy item={itemId} price={price} player={player.Name}");
        });
    }
}
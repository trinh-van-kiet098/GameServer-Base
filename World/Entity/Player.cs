using GameServer.Network;

namespace GameServer.World.Entity;

public sealed class Player : Entity
{
    public string Name { get; }
    public ClientSession Session { get; }
    public int Gold { get; set; } = 1000;
    public int Score { get; set; }
    public List<int> Inventory { get; } = new();

    public Player(ClientSession session, string name)
    {
        Session = session;
        Name = name;
        Id = name.GetHashCode();
    }

    public void Move(float x, float y) { X = x; Y = y; }

    public void AttackBoss(long bossId) { Score += 1; }
}
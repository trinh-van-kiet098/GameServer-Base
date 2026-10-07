namespace GameServer.World.Entity;

public class Monster : Entity
{
    public int Hp { get; set; } = 100;
    public virtual void UpdateAI(long nowMs) { }
}
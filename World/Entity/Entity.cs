namespace GameServer.World.Entity;

public abstract class Entity
{
    public long Id { get; init; }
    public float X { get; set; }
    public float Y { get; set; }
}
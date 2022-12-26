using UnityEngine;

public abstract class Command
{
    public abstract void Execute(GameObject player);
    public abstract void Undo();
}

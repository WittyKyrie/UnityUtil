using UnityEngine;

public class MoveForWard : Command
{
    private GameObject _player;
    public override void Execute(GameObject player)
    {
        _player = player;
        player.transform.Translate(Vector3.forward);
    }

    public override void Undo()
    {
        _player.transform.Translate(Vector3.back);
    }
}

public class MoveLeft : Command
{
    private GameObject _player;
    public override void Execute(GameObject player)
    {
        _player = player;
        player.transform.Translate(Vector3.left);
    }
    
    public override void Undo()
    {
        _player.transform.Translate(Vector3.right);
    }
}

public class MoveRight : Command
{
    private GameObject _player;
    public override void Execute(GameObject player)
    {
        _player = player;
        player.transform.Translate(Vector3.right);
    }
    
    public override void Undo()
    {
        _player.transform.Translate(Vector3.left);
    }
}

public class MoveBack : Command
{
    private GameObject _player;
    public override void Execute(GameObject player)
    {
        _player = player;
        player.transform.Translate(Vector3.back);
    }
    
    public override void Undo()
    {
        _player.transform.Translate(Vector3.forward);
    }
}
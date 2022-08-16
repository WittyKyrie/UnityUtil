using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private readonly MoveForWard _moveForward = new MoveForWard();
    private readonly MoveBack _moveBack = new MoveBack();
    private readonly MoveLeft _moveLeft = new MoveLeft();
    private readonly MoveRight _moveRight = new MoveRight();

    private GameObject _playerCube;
    private KeyCode[] _keyCodes;

    private void Start()
    {
        _playerCube = GameObject.Find("Player");
        _keyCodes = new[] {KeyCode.A, KeyCode.W, KeyCode.S, KeyCode.D,KeyCode.B};
    }

    private void Update()
    {
        PlayerInputHandler();
    }

    private void PlayerInputHandler()
    {
        if (Input.GetKeyDown(_keyCodes[1]))
        {
            _moveForward.Execute(_playerCube);
            CommandManager.Instance.AddCommands(_moveForward);//顺序不能弄混，因为要等赋值完后再加入
        }

        if (Input.GetKeyDown(_keyCodes[0]))
        {
            _moveLeft.Execute(_playerCube);
            CommandManager.Instance.AddCommands(_moveLeft);
        }

        if (Input.GetKeyDown(_keyCodes[3]))
        {
            _moveRight.Execute(_playerCube);
            CommandManager.Instance.AddCommands(_moveRight);
        }

        if (Input.GetKeyDown(_keyCodes[2]))
        {
            _moveBack.Execute(_playerCube);
            CommandManager.Instance.AddCommands(_moveBack);
        }
        
        if (Input.GetKeyDown(_keyCodes[4]))
        {
            StartCoroutine(CommandManager.Instance.UndoStart());
        }
        
    }
}

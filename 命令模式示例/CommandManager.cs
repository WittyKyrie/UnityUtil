using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandManager : MonoBehaviour
{
    public static CommandManager Instance;
    private readonly List<Command> _commandList = new List<Command>();

    private void Awake()
    {
        if (Instance) Destroy(Instance);
        else Instance = this;
    }

    public void AddCommands(Command command)
    {
        _commandList.Add(command);
    }

    public IEnumerator UndoStart()
    {
        _commandList.Reverse();
        foreach (Command command in _commandList)
        {
            yield return new WaitForSeconds(.2f);
            command.Undo();
        }

        _commandList.Clear();
    }

}

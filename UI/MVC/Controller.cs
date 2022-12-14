using System;
using UnityEngine;

public class Controller : MonoBehaviour {
    private MonoBehaviour _model;
    private MonoBehaviour _view;

    public bool IsShowing { get; private set; }

    private void Start() {
        foreach (var component in transform.GetComponents<MonoBehaviour>()) {
            var componentName = component.GetType().ToString();
            if (componentName.Contains("Model")) {
                _model = component;
            } else if (componentName.Contains("View")) {
                _view = component;
            }
        }
    }

    public void DispatchMessage(Message message) {
        IsShowing = message.Command switch {
            Message.CommonCommand.Show => true,
            Message.CommonCommand.Hide => false,
            _ => IsShowing
        };

        var modelType = _model.GetType();
        var handleMessage = modelType.GetMethod("HandleMessage");
        if (handleMessage == null) {
            return;
        }
        var data = handleMessage.Invoke(_model, new object[] {message});
            
        var viewType = _view.GetType();
        var handleCommand = viewType.GetMethod("HandleCommand");
        if (handleCommand == null) {
            return;
        }
        handleCommand.Invoke(_view, new[] {message.Command, data});
    }

    public void DispatchMessageDelayed(Message message, float time) {
        Timer.Register(time, false, false, () => {
            DispatchMessage(message);
        });
    }
        
    public void Show(ValueType extraParams) {
        DispatchMessage(new Message {
            Command = Message.CommonCommand.Show,
            ExtraParams = extraParams
        });
    }
        
    public void Hide(ValueType extraParams) {
        DispatchMessage(new Message {
            Command = Message.CommonCommand.Hide,
            ExtraParams = extraParams
        });
    }
}
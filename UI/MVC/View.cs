using System;
using UnityEngine;

namespace RGScript.UI.MVC {
    public abstract class View<T> : MonoBehaviour {
        private Controller _controller;
        // ReSharper disable once UnusedMember.Global
        protected bool IsShowing => _controller.IsShowing;

        protected abstract void OnShow(T data);
        protected abstract void OnHide(T data);
        protected abstract void OnRefresh(T data);
        protected abstract void ProcessMessage(ValueType command, T data);
        protected abstract bool OnBackButtonClicked();

        protected virtual void Start() {
            _controller = transform.GetComponent<Controller>();
        }

        // ReSharper disable once UnusedMember.Global
        public void HandleCommand(ValueType command, T data) {
            switch (command) {
                case Message.CommonCommand.Show:
                    OnShow(data);
                    break;
                case Message.CommonCommand.Hide:
                    OnHide(data);
                    break;
                case Message.CommonCommand.Refresh:
                    OnRefresh(data);
                    break;
                default:
                    ProcessMessage(command, data);
                    break;
            }
        }

        protected void DispatchMessage(Message message) {
            _controller.DispatchMessage(message);
        }
        
        protected void DispatchMessageDelayed(Message message, float time) {
            _controller.DispatchMessageDelayed(message, time);
        }
    }
}
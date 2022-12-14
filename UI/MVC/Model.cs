using System;
using UnityEngine;

namespace RGScript.UI.MVC {
    public abstract class Model<T> : MonoBehaviour {
        private Controller _controller;
        
        [SerializeField]
        protected T data;
        
        protected abstract void OnShow(ValueType extraParams);
        protected abstract void OnHide(ValueType extraParams);
        protected virtual void OnRefresh() {}
        protected abstract void ProcessMessage(Message message);
        
        private void Start() {
            _controller = transform.GetComponent<Controller>();
        }
        
        public T HandleMessage(Message message) {
            switch (message.Command) {
                case Message.CommonCommand.Show:
                    OnShow(message.ExtraParams);
                    break;
                case Message.CommonCommand.Hide:
                    OnHide(message.ExtraParams);
                    break;
                case Message.CommonCommand.Refresh:
                    OnRefresh();
                    break;
                default:
                    ProcessMessage(message);
                    break;
            }

            return data;
        }
    }
}

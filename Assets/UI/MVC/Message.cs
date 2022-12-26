using System;

    public struct Message {
        public enum CommonCommand {
            Show, Hide, Refresh
        }

        public ValueType Command;
        public ValueType ExtraParams;
    }

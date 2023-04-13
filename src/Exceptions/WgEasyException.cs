using System;

namespace WgEasyManager.Exceptions {
    [Serializable]
    public class WgEasyException : Exception {
        public WgEasyException() { }
        public WgEasyException(string message) : base(message) { }
        public WgEasyException(string message, Exception innerException) : base(message, innerException) { }
    }
}

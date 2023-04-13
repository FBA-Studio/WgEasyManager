using System;

namespace WgEasyManager.Exceptions {
    [Serializable]
    public class WgEasyManagerException : Exception {
        public WgEasyManagerException() { }
        public WgEasyManagerException(string message) : base(message) { }
        public WgEasyManagerException(string message, Exception innerException) : base(message, innerException) { }
    }
}

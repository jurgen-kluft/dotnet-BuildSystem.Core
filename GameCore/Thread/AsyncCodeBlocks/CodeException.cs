using System;

namespace GameCore
{
    /// <summary>
    /// Exception thrown by WaitableAsync or asyc code blocks.
    /// This exception objects will alsways have an InnerException
    /// that contains the actual exception thrown by code inside
    /// WaitableAsync or Async code blocks
    /// </summary>
    [Serializable]
    public class CodeException : Exception
    {
        public CodeException()
        {
        }
        public CodeException(string message)
            : base(message)
        {
        }
        public CodeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

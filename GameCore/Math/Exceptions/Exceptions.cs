using System;
using System.Runtime.Serialization;

namespace GameCore
{
	/// <summary>
	/// Base class for all exceptions thrown by Dim.Math.
	/// </summary>
	public class MathException : ApplicationException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MathException"/> class.
		/// </summary>
		public MathException() : base() {}
		/// <summary>
		/// Initializes a new instance of the <see cref="MathException"/> class with a specified exception message.
		/// </summary>
		/// <param name="message">A message that describes the exception</param>
		public MathException(string inMessage) : base(inMessage) { }
		/// <summary>
		/// Initializes a new instance of the <see cref="MathException"/> class 
		/// with a supplied error message and a reference to the the cause of this exception.
		/// </summary>
		/// <param name="message">A message that describes the exception</param>
		/// <param name="inner">
		/// The exception that is the cause of the current exception. 
		/// If the inInnerException parameter is not a null reference, the current exception is raised 
		/// in a catch block that handles the inner exception.
		/// </param>
		public MathException(string inMessage, Exception inInnerException) : base(inMessage, inInnerException) { }
		/// <summary>
		/// Initializes a new instance of the <see cref="MathException"/> class with serialized data.
		/// </summary>
		/// <param name="info">The object that holds the serialized object data.</param>
		/// <param name="context">The contextual information about the source or destination.</param>
		protected MathException(SerializationInfo inInfo, StreamingContext inContext) : base(inInfo, inContext) { }
	}
}

using System;

namespace Core
{
	/// <summary>
	/// Base class for all ManagedIOCP exceptions
	/// </summary>
	[Serializable()]
	public class ManagedIOCPException : ApplicationException
	{
		public ManagedIOCPException()
		{
		}
		public ManagedIOCPException(string message) 
			: base(message)
		{
		}
		public ManagedIOCPException(string message,Exception innerException)
			: base(message,innerException)
		{
		}
	}
}

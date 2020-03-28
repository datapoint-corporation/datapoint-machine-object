using System;

namespace Datapoint.MachineObject
{
	/// <summary>
	///		Thrown when an unexpected error is encountered while attempting
	///		to read a machine object.
	/// </summary>
	[Serializable]
	public class MachineObjectReadException : Exception
	{
		/// <inheritdoc />
		public MachineObjectReadException()
		{
		}

		/// <inheritdoc />
		public MachineObjectReadException(string? message) : base(message)
		{
		}

		/// <inheritdoc />
		public MachineObjectReadException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}

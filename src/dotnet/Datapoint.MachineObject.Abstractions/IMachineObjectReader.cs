using System.Text;

namespace Datapoint.MachineObject
{
	/// <summary>
	///		A machine object reader.
	/// </summary>
	public interface IMachineObjectReader
	{
		/// <summary>
		///		Gets the machine object character encoding.
		/// </summary>
		Encoding Encoding { get; }

		/// <summary>
		///		Reads a machine object.
		/// </summary>
		/// <exception cref="Datapoint.MachineObject.MachineObjectParseException">
		///		Thrown when an unexpected error prevents the machine object from being read.
		/// </exception>
		/// <returns>
		///		The machine object.
		///	</returns>
		IMachineObject ReadMachineObject();
	}
}

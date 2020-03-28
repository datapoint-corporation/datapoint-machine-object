using System;
using System.Collections.Generic;
using System.Text;

namespace Datapoint.MachineObject
{
	public interface IMachineObject
	{
		Encoding Encoding { get; }

		IReadOnlyDictionary<string, string> Messages { get; }

		Version Version { get; }
	}
}
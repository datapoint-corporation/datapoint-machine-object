using System;
using System.Collections.Generic;
using System.Text;

namespace Datapoint.MachineObject
{
	public sealed class MachineObject : IMachineObject
	{
		internal MachineObject(Version version, Encoding encoding, IReadOnlyDictionary<string, string> messages)
		{
			Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
			Messages = messages ?? throw new ArgumentNullException(nameof(messages));
			Version = version ?? throw new ArgumentNullException(nameof(version));
		}

		public Encoding Encoding { get; }

		public IReadOnlyDictionary<string, string> Messages { get; }

		public Version Version { get; }
	}
}

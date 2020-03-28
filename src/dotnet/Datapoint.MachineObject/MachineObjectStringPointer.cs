namespace Datapoint.MachineObject
{
	internal struct MachineObjectStringPointer
	{
		public MachineObjectStringPointer(long offset, int length)
		{
			Offset = offset;
			Length = length;
		}

		public long Offset { get; }

		public int Length { get; }
	}
}

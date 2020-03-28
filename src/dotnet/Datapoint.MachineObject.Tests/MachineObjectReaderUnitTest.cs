using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Datapoint.MachineObject.Tests
{
	public sealed class MachineObjectReaderUnitTest : IDisposable
	{
		private readonly MachineObjectReader _machineObjectReader;

		public MachineObjectReaderUnitTest()
		{
			_machineObjectReader = new MachineObjectReader
			(
				new FileStream
				(
					Path.Combine
					(
						Directory.GetCurrentDirectory(),
						"messages",
						"pt-PT",
						"default.mo"
					),

					FileMode.Open
				)
			);
		}

		public void Dispose()
		{
			_machineObjectReader.Dispose();
		}

		[Fact]
		public void Read() =>

			_machineObjectReader
				.ReadMachineObject();

		[Fact]
		public void EncodingMatch() =>

			Assert.Equal
			(
				_machineObjectReader.Encoding,
				_machineObjectReader.ReadMachineObject().Encoding
			);


		[Fact]
		public void VersionMatch() =>

			Assert.Equal
			(
				new Version(0, 0),
				_machineObjectReader.ReadMachineObject().Version
			);

		[Fact]
		public void MessagesCount() =>

			Assert.True
			(
				_machineObjectReader.ReadMachineObject().Messages.Count > 0
			);

		[Fact]
		public void HelloWorldMessage()
		{
			var mo = _machineObjectReader.ReadMachineObject();

			Assert.Equal
			(
				mo.Encoding.GetString
				(
					System.Text.Encoding.Convert
					(
						Encoding.Unicode,
						mo.Encoding,
						Encoding.Unicode.GetBytes("Olá Mundo")
					)
				),
				mo.Messages.GetValueOrDefault("Hello World")
			);
		}



		[Fact]
		public void GoodbyeWorldMessage()
		{
			var mo = _machineObjectReader.ReadMachineObject();

			Assert.Equal
			(
				mo.Encoding.GetString
				(
					System.Text.Encoding.Convert
					(
						Encoding.Unicode,
						mo.Encoding,
						Encoding.Unicode.GetBytes("Adeus Mundo")
					)
				),
				mo.Messages.GetValueOrDefault("Goodbye World")
			);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace Datapoint.MachineObject
{
	/// <summary>
	///		<inheritdoc />
	/// </summary>
	public sealed class MachineObjectReader : IMachineObjectReader, IDisposable
	{
		/// <summary>
		///		The input stream endianness.
		/// </summary>
		private bool _littleEndian = BitConverter.IsLittleEndian;

		/// <summary>
		///		The machine object major version number.
		/// </summary>
		private int _major;

		/// <summary>
		///		The machine object minor version number.
		/// </summary>
		private int _minor;

		/// <summary>
		///		The number of strings within the machine object.
		/// </summary>
		private int _numberOfStrings;

		/// <summary>
		///		The original strings input stream offset.
		/// </summary>
		private long _originalStringsOffset;

		/// <summary>
		///		The expected input stream position.
		/// </summary>
		private long _position;

		/// <summary>
		///		The translated strings input stream offset.
		/// </summary>
		private long _translatedStringsOffset;

		/// <summary>
		///		The machine object messages indexed by identifier.
		/// </summary>
		private Dictionary<string, string>? _messages;

		/// <summary>
		///		Creates a new machine object reader with the default UTF8 as
		///		its text character set.
		/// </summary>
		/// <param name="stream">
		///		The machine object reader input stream.
		///	</param>
		public MachineObjectReader(Stream stream) : this(stream, Encoding.UTF8) { }

		/// <summary>
		///		Creates a new machine object reader.
		/// </summary>
		/// <param name="stream">
		///		The machine object reader input stream.
		///	</param>
		/// <param name="encoding">
		///		The machine object text character set.
		///	</param>
		public MachineObjectReader(Stream stream, Encoding encoding)
		{
			Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
			Stream = stream ?? throw new ArgumentNullException(nameof(stream));
		}

		/// <inheritdoc />
		public Encoding Encoding { get; }

		/// <summary>
		///		Gets the input stream.
		/// </summary>
		public Stream Stream { get; }

		/// <inheritdoc />
		public void Dispose() =>

			Stream.Dispose();

		/// <inheritdoc />
		public IMachineObject ReadMachineObject()
		{
			try
			{
				_position = Stream.Position;
				Seek(0);

				// The first two words serve the identification of the file.
				// The magic number will always signal GNU MO files. The number
				// is stored in the byte order used when the MO file was
				// generated, so the magic number really is two numbers: 
				// 0x950412de and 0xde120495.
				switch (ReadWord())
				{
					case 0x950412de:
						_littleEndian = true;
						break;

					case 0xde120495:
						_littleEndian = false;
						break;

					default:
						throw new Exception("Can not determine machine object stream endian configuration.");
				}

				// The second word describes the current revision of the file
				// format, composed of a major and a minor revision number. 
				// The revision numbers ensure that the readers of MO files can
				// distinguish new formats from old ones and handle their
				// contents, as far as possible.
				_major = Convert.ToInt32(ReadShort());
				_minor = Convert.ToInt32(ReadShort());

				if (_major > 1)
					throw new Exception("Machine object version is not supported.");

				// Then following words define the number of strings and the
				// offset for the original and translated hash tables.
				_numberOfStrings = Convert.ToInt32(ReadWord());
				_originalStringsOffset = Convert.ToInt32(ReadWord());
				_translatedStringsOffset = Convert.ToInt32(ReadWord());

				// Each message and translation is referenced by two words
				// containing its length and offset.
				_messages = new Dictionary<string, string>(_numberOfStrings);

				for (uint i = 0; i < _numberOfStrings; ++i)
				{
					int length;
					long offset;

					// Message pointer.
					Seek(_originalStringsOffset + (i * (sizeof(uint) * 2)));

					length = Convert.ToInt32(ReadWord());
					offset = Convert.ToInt64(ReadWord());

					var message = new MachineObjectStringPointer(offset, (int)length);

					// Translation pointer.
					Seek(_translatedStringsOffset + (i * (sizeof(uint) * 2)));

					length = Convert.ToInt32(ReadWord());
					offset = Convert.ToInt64(ReadWord());

					var translation = new MachineObjectStringPointer(offset, (int)length);

					// Combination.
					_messages.Add
					(
						ReadString(message.Offset, message.Length),
						ReadString(translation.Offset, translation.Length)
					);
				}

				return new MachineObject
				(
					new Version(_major, _minor),
					Encoding,
					new ReadOnlyDictionary<string, string>(_messages)
				);
			}
			catch (Exception e)
			{
				throw new MachineObjectReadException("Can not read machine object.", e);
			}
		}

		/// <summary>
		///		Reverses the given byte array if the current runtime
		///		environment endianness does not match that of the input stream.
		/// </summary>
		/// <param name="content">
		///		The content to reverse, if applicable.
		/// </param>
		/// <returns>
		///		The content.
		/// </returns>
		private byte[] FixEndian(byte[] content)
		{
			if (!(BitConverter.IsLittleEndian == _littleEndian))
				Array.Reverse(content);

			return content;
		}

		/// <summary>
		///		Reads multiple bytes from the input stream while performing
		///		common read validations and keeping track of the its current
		///		position.
		/// </summary>
		/// <param name="byteCount">
		///		The number of bytes to read.
		/// </param>
		/// <returns>
		///		The bytes read.
		/// </returns>
		private byte[] Read(int byteCount)
		{
			if (!(_position == Stream.Position))
				throw new Exception("Stream position has changed while reading the machine object.");

			var buffer = new byte[byteCount];

			Stream.Read(buffer, 0, byteCount);

			_position += byteCount;

			return buffer;
		}

		/// <summary>
		///		Reads an unsigned short integer from the input stream.
		/// </summary>
		/// <returns>
		///		The unsigned short integer.
		/// </returns>
		private ushort ReadShort() =>

			BitConverter.ToUInt16(FixEndian(Read(sizeof(ushort))));

		/// <summary>
		///		Reads a string from the input stream.
		/// </summary>
		/// <param name="position">
		///		The position to start reading from.
		/// </param>
		/// <param name="byteCount">
		///		The number of bytes to read.
		///	</param>
		/// <returns>
		///		The string.
		/// </returns>
		private string ReadString(long position, int byteCount)
		{
			Seek(position);

			return Encoding.GetString(Read(byteCount));
		}

		/// <summary>
		///		Reads an unsigned integer from the input stream.
		/// </summary>
		/// <returns>
		///		The unsigned integer.
		/// </returns>
		private uint ReadWord() =>

			BitConverter.ToUInt32(FixEndian(Read(sizeof(uint))));

		/// <summary>
		///		Changes the input stream position.
		/// </summary>
		/// <param name="position">
		///		The new position.
		/// </param>
		private void Seek(long position)
		{
			Stream.Seek(position, SeekOrigin.Begin);

			_position = position;
		}
	}
}

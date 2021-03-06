using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace IO.Helpers
{
	/// <summary>
	/// Serializes and deserializes strcutures
	/// </summary>
	public static class StructSerializer
	{
		/// <summary>Serializes into a stream</summary>
		public static void Serialize<T>(Stream stream, T data)
			where T : struct
		{
			var tSpan = MemoryMarshal.CreateSpan(ref data, 1);
			var span = MemoryMarshal.AsBytes(tSpan);
			stream.Write(span);
		}

		/// <summary>Serializes into a byte array</summary>
		public static byte[] Serialize<T>(T data)
			where T : struct
		{
			var type = typeof(T);
			if (type.IsEnum)
				throw EnumNotSupportedException(type);
			//type = Enum.GetUnderlyingType(type);

			var size = Marshal.SizeOf(type);
			var array = new byte[size];
			var ptr = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(data!, ptr, true);
			Marshal.Copy(ptr, array, 0, size);
			Marshal.FreeHGlobal(ptr);
			return array;
		}

		/// <summary>Deserializes from a stream</summary>
		public static T Deserialize<T>(Stream stream)
			where T : struct
		{
			var result = default(T);
			var tSpan = MemoryMarshal.CreateSpan(ref result, 1);
			var span = MemoryMarshal.AsBytes(tSpan);
			stream.Read(span);
			return result;
		}

		/// <summary>Deserializes from a memory</summary>
		public static T Deserialize<T>(ReadOnlyMemory<byte> memory)
			where T : struct => Deserialize<T>(memory.Span);

		/// <summary>Deserializes from a span</summary>
		public static T Deserialize<T>(ReadOnlySpan<byte> span)
			where T : struct =>
			MemoryMarshal.Read<T>(span);

		/// <summary>Deserializes from a byte array</summary>
		public static T Deserialize<T>(byte[] array)
		{
			var type = typeof(T);
			if (type.IsEnum)
				throw EnumNotSupportedException(type);
			//type = Enum.GetUnderlyingType(type);

			var size = Marshal.SizeOf(type);
			var ptr = Marshal.AllocHGlobal(size);
			Marshal.Copy(array, 0, ptr, size);
			var s = Marshal.PtrToStructure<T>(ptr)!;
			Marshal.FreeHGlobal(ptr);
			return s!;
		}

		private static Exception EnumNotSupportedException(MemberInfo type) =>
			new ArgumentException($"Type {type.Name} is an enum type which is currently not supported for serialization.");
	}
}
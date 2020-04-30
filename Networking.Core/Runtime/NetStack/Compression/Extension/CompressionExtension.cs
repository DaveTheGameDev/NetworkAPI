using Installation01.Networking.NetStack.Serialization;
using UnityEngine;

namespace Installation01.Networking.NetStack.Compression.Extension
{
	// TODO: Determine if to use bounded Vector3 or half-precision implementation
	public static class CompressionExtension
	{
		#region Float

		public static void AddFloat(this BitBuffer buffer, float value)
		{
			buffer.AddUShort(HalfPrecision.Compress(value));
		}

		public static float ReadFloat(this BitBuffer buffer)
		{
			return HalfPrecision.Decompress(buffer.ReadUShort());
		}

		public static void AddFloatBounded(this BitBuffer buffer, float value, float min = 0f, float max = 1f,
			float precision = 100f)
		{
			BoundedRange br = new BoundedRange(min, max, precision);
			buffer.AddUInt(br.Compress(value));
		}

		public static float ReadFloatBounded(this BitBuffer buffer, float min = 0f, float max = 1f,
			float precision = 100f)
		{
			BoundedRange br = new BoundedRange(min, max, precision);
			return br.Decompress(buffer.ReadUInt());
		}

		#endregion

		#region Vector2

		public static void AddVector2(this BitBuffer buffer, Vector2 value)
		{
			buffer.AddUShort(HalfPrecision.Compress(value.x));
			buffer.AddUShort(HalfPrecision.Compress(value.y));
		}

		public static Vector2 ReadVector2(this BitBuffer buffer)
		{
			return new Vector2
			{
				x = HalfPrecision.Decompress(buffer.ReadUShort()),
				y = HalfPrecision.Decompress(buffer.ReadUShort())
			};
		}

		#endregion

		#region Vector3

		public static void AddVector3(this BitBuffer buffer, Vector3 value, BoundedRange[] ranges)
		{
			CompressedVector3 compressed = BoundedRange.Compress(value, ranges);

			buffer.AddUInt(compressed.x);
			buffer.AddUInt(compressed.y);
			buffer.AddUInt(compressed.z);
		}

		public static Vector3 ReadVector3(this BitBuffer buffer, BoundedRange[] ranges)
		{
			CompressedVector3 compressed = new CompressedVector3
			{
				x = buffer.ReadUInt(),
				y = buffer.ReadUInt(),
				z = buffer.ReadUInt()
			};

			return BoundedRange.Decompress(compressed, ranges);
		}

		public static void AddVector3(this BitBuffer buffer, Vector3 value)
		{
			buffer.AddUShort(HalfPrecision.Compress(value.x));
			buffer.AddUShort(HalfPrecision.Compress(value.y));
			buffer.AddUShort(HalfPrecision.Compress(value.z));
		}

		public static Vector3 ReadVector3(this BitBuffer buffer)
		{
			return new Vector3
			{
				x = HalfPrecision.Decompress(buffer.ReadUShort()),
				y = HalfPrecision.Decompress(buffer.ReadUShort()),
				z = HalfPrecision.Decompress(buffer.ReadUShort())
			};
		}

		#endregion

		#region Quaternion

		public static void AddQuaternion(this BitBuffer buffer, Quaternion value)
		{
			CompressedQuaternion compressed = SmallestThree.Compress(value);

			buffer.AddByte(compressed.m);
			buffer.AddShort(compressed.a);
			buffer.AddShort(compressed.b);
			buffer.AddShort(compressed.c);
		}

		public static Quaternion ReadQuaternion(this BitBuffer buffer)
		{
			CompressedQuaternion compressed = new CompressedQuaternion
			{
				m = buffer.ReadByte(),
				a = buffer.ReadShort(),
				b = buffer.ReadShort(),
				c = buffer.ReadShort()
			};

			return SmallestThree.Decompress(compressed);
		}

		#endregion
	}
}
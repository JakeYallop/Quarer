using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Quarer.Tests;

public class BitBufferTests
{
    [Fact]
    public void Ctor_InitialBitCountLessThanZero_ThrowsArgumentOutOfRangeException()
        => Assert.Throws<ArgumentOutOfRangeException>(() => new BitBuffer(-1));

    [Fact]
    public void WriteBitsBigEndian_ValidValue_IsSuccessful()
    {
        var bitBuffer = new BitBuffer();
        ushort validValue = 1023;
        bitBuffer.WriteBitsBigEndian(0, validValue, 10);
        bitBuffer.WriteBitsBigEndian(5, 0b11011, 5);
    }

    public static TheoryData<object, int> ValidData_FitsWithin64Bits()
      => new()
      {
            { 0xBEEF_FACE_BEAD_FADEu, 64 },
            { ulong.MaxValue, 64 },
            { 0xFFFF_FFFF_FFFF_FFFEu, 64 },
            { 0x7FFF_FFFF_FFFF_FFFFu, 64 },
            { 0x7FFF_FFFF_FFFF_FFFFu, 63 },
      };

    public static TheoryData<object, int> ValidData_FitsWithin32Bits()
        => new()
        {
            { 0xBEEF_FACEu, 32 },
            { uint.MaxValue, 32 },
            { 0xFFFF_FFFEu, 32 },
            { 0x7FFF_FFFFu, 32 },
            { 0x7FFF_FFFFu, 31 },
        };

    public static TheoryData<object, int> ValidData_FitsWithin16Bits()
        => new()
        {
            { (ushort)0xBEEF, 16 },
            { ushort.MaxValue, 16 },
            { (ushort)0xFFFE, 16 },
            { (ushort)0x7FFF, 16 },
            { (ushort)0x7FFF, 15 },
            { (ushort)1, 1 },
            { (ushort)0, 0 },
            { (ushort)3, 2 },
            { (ushort)0, 4 }, //e.g writing the terminator block
        };

    private static T UnboxInto<T>(object obj) where T : IBinaryInteger<T>
    {
        unchecked
        {
            return obj switch
            {
                ushort @ushort => GetValue(@ushort),
                uint @uint => GetValue(@uint),
                ulong @ulong => GetValue(@ulong),
                _ => throw new NotSupportedException($"Numeric type {obj.GetType().Name} not supported for unboxing.")
            };

            static T GetValue<TUnboxedValue>(TUnboxedValue value) where TUnboxedValue : IBinaryInteger<TUnboxedValue>
            {
                return T.CreateTruncating(value);
            }
        }
    }

#pragma warning disable xUnit1045 // Avoid using TheoryData type arguments that might not be serializable
    [Theory]
    [MemberData(nameof(ValidData_FitsWithin16Bits))]
    public void WriteBitsBigEndian_ValidValue_IsSuccessful_short(object value, int bitCount)
    {
        var bitBuffer = new BitBuffer();
        var bits = UnboxInto<short>(value);
        bitBuffer.WriteBitsBigEndian(0, bits, bitCount);
        Assert.Equal(bitCount, bitBuffer.Count);
        var expected = bitCount == 0 ? "" : bits.ToString($"B{bitCount}");
        AssertExtensions.BitsEqual(expected, bitBuffer.AsBitEnumerable());
    }

    [Theory]
    [MemberData(nameof(ValidData_FitsWithin16Bits))]
    public void WriteBitsBigEndian_ValidValue_IsSuccessful_ushort(object value, int bitCount)
    {
        var bitBuffer = new BitBuffer();
        var bits = UnboxInto<ushort>(value);
        bitBuffer.WriteBitsBigEndian(0, bits, bitCount);
        Assert.Equal(bitCount, bitBuffer.Count);
        var expected = bitCount == 0 ? "" : bits.ToString($"B{bitCount}");
        AssertExtensions.BitsEqual(expected, bitBuffer.AsBitEnumerable());
    }

    [Theory]
    [MemberData(nameof(ValidData_FitsWithin16Bits))]
    [MemberData(nameof(ValidData_FitsWithin32Bits))]
    public void WriteBitsBigEndian_ValidValue_IsSuccessful_int(object value, int bitCount)
    {
        var bitBuffer = new BitBuffer();
        var bits = UnboxInto<int>(value);
        bitBuffer.WriteBitsBigEndian(0, bits, bitCount);
        Assert.Equal(bitCount, bitBuffer.Count);
        var expected = bitCount == 0 ? "" : bits.ToString($"B{bitCount}");
        AssertExtensions.BitsEqual(expected, bitBuffer.AsBitEnumerable());
    }

    [Theory]
    [MemberData(nameof(ValidData_FitsWithin16Bits))]
    [MemberData(nameof(ValidData_FitsWithin32Bits))]
    public void WriteBitsBigEndian_ValidValue_IsSuccessful_uint(object value, int bitCount)
    {
        var bitBuffer = new BitBuffer();
        var bits = UnboxInto<uint>(value);
        bitBuffer.WriteBitsBigEndian(0, bits, bitCount);
        Assert.Equal(bitCount, bitBuffer.Count);
        var expected = bitCount == 0 ? "" : bits.ToString($"B{bitCount}");
        AssertExtensions.BitsEqual(expected, bitBuffer.AsBitEnumerable());
    }

    [Theory]
    [MemberData(nameof(ValidData_FitsWithin16Bits))]
    [MemberData(nameof(ValidData_FitsWithin32Bits))]
    [MemberData(nameof(ValidData_FitsWithin64Bits))]
    public void WriteBitsBigEndian_ValidValue_IsSuccessful_long(object value, int bitCount)
    {
        var bitBuffer = new BitBuffer();
        var bits = UnboxInto<long>(value);
        bitBuffer.WriteBitsBigEndian(0, bits, bitCount);
        Assert.Equal(bitCount, bitBuffer.Count);
        var expected = bitCount == 0 ? "" : bits.ToString($"B{bitCount}");
        AssertExtensions.BitsEqual(expected, bitBuffer.AsBitEnumerable());
    }

    [Theory]
    [MemberData(nameof(ValidData_FitsWithin16Bits))]
    [MemberData(nameof(ValidData_FitsWithin32Bits))]
    [MemberData(nameof(ValidData_FitsWithin64Bits))]
    public void WriteBitsBigEndian_ValidValue_IsSuccessful_ulong(object value, int bitCount)
    {
        var bitBuffer = new BitBuffer();
        var bits = UnboxInto<ulong>(value);
        bitBuffer.WriteBitsBigEndian(0, bits, bitCount);
        Assert.Equal(bitCount, bitBuffer.Count);
        var expected = bitCount == 0 ? "" : bits.ToString($"B{bitCount}");
        AssertExtensions.BitsEqual(expected, bitBuffer.AsBitEnumerable());
    }

    [Theory]
    [InlineData(1024, 10)]
    [InlineData(256, 8)]
    [InlineData(16, 4)]
    [InlineData(7, 2)]
    public void WriteBitsBigEndian_ValueLargerThanSpecifiedBits_ShouldThrowArgumentOutOfRangeException(ushort value, int bitCount)
    {
        var bitBuffer = new BitBuffer();
        Assert.Throws<ArgumentOutOfRangeException>(() => bitBuffer.WriteBitsBigEndian(0, value, bitCount));
    }
#pragma warning restore xUnit1045 // Avoid using TheoryData type arguments that might not be serializable

    [Fact]
    public void WriteBitsBigEndian_WritesCorrectValues()
    {
        var writer = new BitWriter();
        ushort value1 = 0b1000_0000_00;
        ushort value2 = 0b0100;
        ushort value3 = 0b0010_000;
        ushort value4 = 0b0010_0101_0101_1;
        ushort value5 = 0b0001_00;
        ushort value6 = 0b0100;

        writer.WriteBitsBigEndian(value1, 10);
        writer.WriteBitsBigEndian(value2, 4);
        writer.WriteBitsBigEndian(value3, 7);
        writer.WriteBitsBigEndian(value4, 13);
        writer.WriteBitsBigEndian(value5, 6);
        writer.WriteBitsBigEndian(value6, 4);

        var bits = writer.Buffer.AsBitEnumerable();

        Assert.Equal(44, writer.Buffer.Count);
        AssertExtensions.BitsEqual($"{value1:B10}{value2:B4}{value3:B7}{value4:B13}{value5:B6}{value6:B4}", bits);
    }

    [Fact]
    public void WriteBitsBigEndian_WritesCorrectValues_Bytes()
    {
        var writer = new BitWriter();
        ushort value1 = 0b1000_0000_00;
        ushort value2 = 0b0100;
        ushort value3 = 0b0010_000;
        ushort value4 = 0b0010_0101_0101_1;
        ushort value5 = 0b0001_00;

        writer.WriteBitsBigEndian(value1, 10);
        writer.WriteBitsBigEndian(value2, 4);
        writer.WriteBitsBigEndian(value3, 7);
        writer.WriteBitsBigEndian(value4, 13);
        writer.WriteBitsBigEndian(value5, 6);

        var bytes = writer.Buffer.AsByteEnumerable().ToArray();

        Assert.Equal(5, bytes.Length);
        Assert.Equal(0x80, bytes[0]);
        Assert.Equal(0x10, bytes[1]);
        Assert.Equal(0x81, bytes[2]);
        Assert.Equal(0x2A, bytes[3]);
        Assert.Equal(0xC4, bytes[4]);
    }

    [Fact]
    public void AsByteEnumerable_ReturnsZeroPaddedLastByte()
    {
        var bitBuffer = new BitBuffer();
        var value1 = 0xCACA_CACA;
        var value2 = 0b0100_1;

        bitBuffer.WriteBitsBigEndian(0, value1, 32);
        bitBuffer.WriteBitsBigEndian(32, value2, 5);

        var bytes = bitBuffer.AsByteEnumerable().ToArray();

        Assert.Equal(5, bytes.Length);
        Assert.Equal(0xCA, bytes[0]);
        Assert.Equal(0xCA, bytes[1]);
        Assert.Equal(0xCA, bytes[2]);
        Assert.Equal(0xCA, bytes[3]);
        Assert.Equal(0b0100_1000, bytes[4]);
    }

    [Fact]
    public void WriteBitsBigEndian_HasCorrectBitCountAfterAddingMultipleValues_OverwritesForSamePosition()
    {
        var bitBuffer = new BitBuffer();
        ushort value1 = 0b1000_0000_00;
        ushort value2 = 0b0100_0000_00;
        ushort value3 = 0b0010_1010_10;

        bitBuffer.WriteBitsBigEndian(0, value1, 10);
        bitBuffer.WriteBitsBigEndian(0, value2, 10);
        bitBuffer.WriteBitsBigEndian(0, value3, 10);

        Assert.Equal(10, bitBuffer.Count);
        AssertExtensions.BitsEqual($"{value3:B10}", bitBuffer.AsBitEnumerable());
    }

    [Fact]
    public void WriteBitsBigEndian_HasCorrectBitCountAfterAddingMultipleValues()
    {
        var bitBuffer = new BitBuffer();
        ushort value1 = 0b1000;
        ushort value2 = 0b0100;
        ushort value3 = 0b0010;

        bitBuffer.WriteBitsBigEndian(0, value1, 4);
        bitBuffer.WriteBitsBigEndian(4, value2, 4);
        bitBuffer.WriteBitsBigEndian(8, value3, 4);

        Assert.Equal(12, bitBuffer.Count);
        AssertExtensions.BitsEqual($"{value1:B4}{value2:B4}{value3:B4}", bitBuffer.AsBitEnumerable());
    }

    [Fact]
    public void WriteBitsBigEndian_HasCorrectBitCountAfterWritingAtForwardPosition()
    {
        var bitBuffer = new BitBuffer();
        ushort value1 = 0b1000_0000_00;
        ushort value2 = 0b0100_0000_00;

        bitBuffer.WriteBitsBigEndian(0, value1, 10);
        bitBuffer.WriteBitsBigEndian(20, value2, 10);

        Assert.Equal(30, bitBuffer.Count);
        AssertExtensions.BitsEqual($"{value1:B10}{0:B10}{value2:B10}", bitBuffer.AsBitEnumerable());
    }

    [Fact]
    public void ByteCount_ReturnsExpectedResult()
    {
        var bitBuffer = new BitBuffer();
        bitBuffer.WriteBitsBigEndian(0, 0, 32);
        bitBuffer.WriteBitsBigEndian(32, 0, 5);

        Assert.Equal(5, bitBuffer.ByteCount);
    }

    [Theory]
    [InlineData(new byte[] { 0b0000_0000, 0b0000_0000 }, 14, true, new byte[] { 0b0000_0000, 0b0000_0010 })]
    [InlineData(new byte[] { 0b0101_0101, 0b1111_0000 }, 9, false, new byte[] { 0b0101_0101, 0b1011_0000 })]
    [InlineData(new byte[] { 0b0101_0101, 0b1111_0000 }, 1, false, new byte[] { 0b0001_0101, 0b1111_0000 })]
    [InlineData(new byte[] { 0b0101_0101, 0b1111_0000 }, 0, true, new byte[] { 0b1101_0101, 0b1111_0000 })]
    [InlineData(new byte[] { 0xFA, 0xFA, 0xFA, 0xFA, 0b0000_0000, 0b0101_0101, 0b1111_0000 }, 55, true, new byte[] { 0xFA, 0xFA, 0xFA, 0xFA, 0b0000_0000, 0b0101_0101, 0b1111_0001 })]
    [InlineData(new byte[] { 0xFA, 0xFA, 0xFA, 0xFA, 0b0000_0000, 0b0101_0101, 0b1111_0001 }, 55, false, new byte[] { 0xFA, 0xFA, 0xFA, 0xFA, 0b0000_0000, 0b0101_0101, 0b1111_0000 })]
    [InlineData(new byte[] { 0xFA, 0xFA, 0xFA, 0xFA, 0b0000_0000, 0b0101_0101, 0b1101_0001 }, 50, false, new byte[] { 0xFA, 0xFA, 0xFA, 0xFA, 0b0000_0000, 0b0101_0101, 0b1101_0001 })]
    public void SetBit(byte[] data, int index, bool value, byte[] expected)
    {
        var bitBuffer = Buffer(data);
        bitBuffer[index] = value;
        Assert.Equal(expected, bitBuffer.AsByteEnumerable());
    }

    [Theory]
    [InlineData(new byte[] { 0b0000_0000, 0b0000_0000 }, 14, false)]
    [InlineData(new byte[] { 0b0101_0101, 0b1111_0000 }, 9, true)]
    [InlineData(new byte[] { 0b0101_0101, 0b1111_0000 }, 1, true)]
    [InlineData(new byte[] { 0xFA, 0xFA, 0xFA, 0xFA, 0b0000_0000, 0b0101_0101, 0b1111_0000 }, 55, false)]
    [InlineData(new byte[] { 0xFA, 0xFA, 0xFA, 0xFA, 0b0000_0000, 0b0101_0101, 0b1111_0001 }, 55, true)]
    [InlineData(new byte[] { 0xFA, 0xFA, 0xFA, 0xFA, 0b0000_0000, 0b0101_0101, 0b1010_0001 }, 50, true)]
    public void GetBit(byte[] data, int index, bool expected)
    {
        var bitBuffer = Buffer(data);
        var actual = bitBuffer[index];
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(5, 10)]
    [InlineData(5, 0)]
    [InlineData(31, 33)]
    [InlineData(5, 33)]
    [InlineData(10, 5)]
    public void SetCapacity(int initial, int newCapacity)
    {
        var bitBuffer = new BitBuffer(initial);
        Assert.Equal(0, bitBuffer.Count);
        bitBuffer.SetCapacity(newCapacity);
        Assert.Equal(0, bitBuffer.Count);
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field)]
    private static extern ref List<byte> _buffer(BitBuffer @this);

    [Fact]
    public void SetCapacity_SetToSmallerValue_TrimsUnderlyingBuffer()
    {
        var bitBuffer = new BitBuffer(100 << 3);
        var buffer = _buffer(bitBuffer);
        Assert.True(buffer.Capacity >= 100);
        bitBuffer.SetCapacity(8);
        Assert.Equal(1, buffer.Capacity);
    }

    [Fact]
    public void SetCapacity_CapacityLessThanZero_ThrowsArgumentOutOfRangeException()
    {
        var bitBuffer = new BitBuffer();
        Assert.Throws<ArgumentOutOfRangeException>(() => bitBuffer.SetCapacity(-1));
    }

    [Fact]
    public void SetCapacity_CapacityLessThanCount_ThrowsArgumentException()
    {
        var bitBuffer = new BitBuffer();
        BitBufferMarshal.SetCount(bitBuffer, 10);
        Assert.Throws<ArgumentException>(() => bitBuffer.SetCapacity(5));
    }

    [Fact]
    public void CopyTo_CopiesCorrectValues()
    {
        var bitBuffer = new BitBuffer();
        bitBuffer.WriteBitsBigEndian(0, 0b1010_1010, 8);
        var destination = new BitBuffer();
        BitBufferMarshal.SetCount(destination, 8);
        bitBuffer.CopyTo(destination);
        AssertExtensions.BitsEqual("1010 1010", destination.AsBitEnumerable());
    }

    [Fact]
    public void CopyTo_WhenDestinationIsNull_ThrowsArgumentNullException()
    {
        var bitBuffer = new BitBuffer();
        Assert.Throws<ArgumentNullException>(() => bitBuffer.CopyTo(null!));
    }

    [Fact]
    public void CopyTo_WhenDestinationIsTooSmall_ThrowsArgumentException()
    {
        var bitBuffer = new BitBuffer();
        bitBuffer.WriteBitsBigEndian(0, 0b1010_1010, 8);
        var destination = new BitBuffer();
        Assert.Throws<ArgumentException>(() => bitBuffer.CopyTo(destination));
    }

    [Theory]
    [InlineData("")]
    [InlineData("0000")]
    [InlineData("0101")]
    [InlineData("1111_1111_1111_1111_1111_1111_1111_1111_0001")]
    public void Equals_ReturnsExpectedResult(string contents)
    {
        var buffer1 = InputToBuffer(contents);
        var buffer2 = InputToBuffer(contents);
        var result = buffer1.Equals((object)buffer2);
        Assert.True(result);
    }

    [Fact]
    public void Equals_AfterSetCount_WithUnusedNonZeroBits_ReturnsExpectedResult()
    {
        var buffer1 = InputToBuffer("1111_1111_1111_1111_1111_1111_1111_1111_0001"); //36 bits
        var buffer2 = InputToBuffer("1111_1111_1111_1111"); //16 bits

        BitBufferMarshal.SetCount(buffer1, 16);

        Assert.True(buffer1.Equals(buffer2));
    }

    [Theory]
    [InlineData("")]
    [InlineData("0000")]
    [InlineData("0101")]
    [InlineData("1111_1111_1111_1111_1111_1111_1111_1111_0001")]
    [InlineData("1111_1111_1111_1111_1111_1111_1111_1111_0001_1000_0001_1001_1010_1010_11")] //32 + 16 + 8 + 2 bits
    public void Equals_IEquatable_ReturnsExpectedResult(string contents)
    {
        var buffer1 = InputToBuffer(contents);
        var buffer2 = InputToBuffer(contents);
        var result = buffer1.Equals(buffer2);
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("0000")]
    [InlineData("0101")]
    [InlineData("1111_1111_1111_1111_1111_1111_1111_1111_0001")]
    public void Op_Equality_ReturnsExpectedResult(string contents)
    {
        var buffer1 = InputToBuffer(contents);
        var buffer2 = InputToBuffer(contents);
        var result = buffer1 == buffer2;
        Assert.True(result);
    }

    [Theory]
    [InlineData("", "1")]
    [InlineData("0000", "0101")]
    [InlineData("1111_1111_1111_1111_1111_1111_1111_1110", "1111_1111_1111_1111_1111_1111_1111_1111")]
    [InlineData("1111_1111_1111_1111_1111_1111_1111_1111_1", "1111_1111_1111_1111_1111_1111_1111_1111_0")]
    public void Op_Inequality_ReturnsExpectedResult(string contents, string contents2)
    {
        var buffer1 = InputToBuffer(contents);
        var buffer2 = InputToBuffer(contents2);
        var result = buffer1 != buffer2;
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("0000")]
    [InlineData("0101")]
    [InlineData("1111_1111_1111_1111_1111_1111_1111_1111_0001")]
    [InlineData("1111_1111_1111_1111_1111_1111_1111_1111_0001_1000_0001_1001_1010_1010_11")] //32 + 16 + 8 + 2 bits
    public void GetHashCode_Equal_ReturnsSameHashcode(string contents)
    {
        var buffer1 = InputToBuffer(contents);
        var buffer2 = InputToBuffer(contents);
        var hashCode1 = buffer1.GetHashCode();
        var hashCode2 = buffer2.GetHashCode();
        Assert.Equal(hashCode1, hashCode2);
    }

    [Theory]
    [InlineData("", "1")]
    [InlineData("0000", "0101")]
    [InlineData("1111_1111_1111_1111_1111_1111_1111_1110", "1111_1111_1111_1111_1111_1111_1111_1111")]
    [InlineData("1111_1111_1111_1111_1111_1111_1111_1111_0001_1000_0001_1001_1010_1010_11", "1111_1111_1111_1111_1111_1111_1111_1111_0001_1000_0001_1001_1010_1010_10")]
    [InlineData("1111_1111_1111_1111_1111_1111_1111_1111_0001_1000_0001_1001_1010_1010_0100_1", "1111_1111_1111_1111_1111_1111_1111_1111_0001_1000_0001_1001_1010_1010_1000_0")]
    public void GetHashCode_NotEqual_ReturnsDifferentHashcode(string contents, string contents2)
    {
        var buffer1 = InputToBuffer(contents);
        var buffer2 = InputToBuffer(contents2);
        var hashCode1 = buffer1.GetHashCode();
        var hashCode2 = buffer2.GetHashCode();
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void HashCode_AfterSetCount_WithUnusedNonZeroBits_ReturnsExpectedResult()
    {
        var buffer1 = InputToBuffer("1111_1111_1111_1111_1111_1111_1111_1111_0001"); //36 bits
        var buffer2 = InputToBuffer("1111_1111_1111_1111"); //16 bits

        BitBufferMarshal.SetCount(buffer1, 16);

        var hashCode1 = buffer1.GetHashCode();
        var hashCode2 = buffer2.GetHashCode();
        Assert.Equal(hashCode1, hashCode2);
    }

    private static BitBuffer Buffer(ReadOnlySpan<byte> bytes)
    {
        var bitBuffer = new BitBuffer(bytes.Length * 8);
        var writer = new BitWriter(bitBuffer);
        foreach (var b in bytes)
        {
            writer.WriteBitsBigEndian(b, 8);
        }
        return bitBuffer;
    }

    private static BitBuffer InputToBuffer(string input)
    {
        var bitbuffer = new BitBuffer(input.Length);
        var writer = new BitWriter(bitbuffer);
        foreach (var c in input)
        {
            if (c is '_' or ' ')
            {
                continue;
            }

            if (c is not '0' and not '1')
            {
                throw new ArgumentException("Input must be a binary string.", nameof(input));
            }

            writer.WriteBitsBigEndian(c is '1' ? 1 : 0, 1);
        }

        return bitbuffer;
    }
}

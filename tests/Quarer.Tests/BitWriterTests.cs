using System.Numerics;

namespace Quarer.Tests;

public class BitWriterTests
{
    [Fact]
    public void WriteBits_ValidValue_IsSuccessful()
    {
        var bitBuffer = new BitBuffer();
        ushort validValue = 1023;
        bitBuffer.WriteBits(validValue, 10);
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
    public void WriteBits_ValidValue_IsSuccessful_short(object value, int bitCount)
    {
        var bitBuffer = new BitBuffer();
        var bits = UnboxInto<short>(value);
        bitBuffer.WriteBits(bits, bitCount);
    }

    [Theory]
    [MemberData(nameof(ValidData_FitsWithin16Bits))]
    public void WriteBits_ValidValue_IsSuccessful_ushort(object value, int bitCount)
    {
        var bitBuffer = new BitBuffer();
        var bits = UnboxInto<ushort>(value);
        bitBuffer.WriteBits(bits, bitCount);
    }

    [Theory]
    [MemberData(nameof(ValidData_FitsWithin16Bits))]
    [MemberData(nameof(ValidData_FitsWithin32Bits))]
    public void WriteBits_ValidValue_IsSuccessful_int(object value, int bitCount)
    {
        var bitBuffer = new BitBuffer();
        var bits = UnboxInto<int>(value);
        bitBuffer.WriteBits(bits, bitCount);
    }

    [Theory]
    [MemberData(nameof(ValidData_FitsWithin16Bits))]
    [MemberData(nameof(ValidData_FitsWithin32Bits))]
    public void WriteBits_ValidValue_IsSuccessful_uint(object value, int bitCount)
    {
        var bitBuffer = new BitBuffer();
        var bits = UnboxInto<uint>(value);
        bitBuffer.WriteBits(bits, bitCount);
    }

    [Theory]
    [MemberData(nameof(ValidData_FitsWithin16Bits))]
    [MemberData(nameof(ValidData_FitsWithin32Bits))]
    [MemberData(nameof(ValidData_FitsWithin64Bits))]
    public void WriteBits_ValidValue_IsSuccessful_long(object value, int bitCount)
    {
        var bitBuffer = new BitBuffer();
        var bits = UnboxInto<long>(value);
        bitBuffer.WriteBits(bits, bitCount);
    }

    [Theory]
    [MemberData(nameof(ValidData_FitsWithin16Bits))]
    [MemberData(nameof(ValidData_FitsWithin32Bits))]
    [MemberData(nameof(ValidData_FitsWithin64Bits))]
    public void WriteBits_ValidValue_IsSuccessful_ulong(object value, int bitCount)
    {
        var bitBuffer = new BitBuffer();
        var bits = UnboxInto<ulong>(value);
        bitBuffer.WriteBits(bits, bitCount);
    }

    [Theory]
    [InlineData(1024, 10)]
    [InlineData(256, 8)]
    [InlineData(16, 4)]
    [InlineData(7, 2)]
    public void WriteBits_ValueLargerThanSpecifiedBits_ShouldThrowArgumentOutOfRangeException(ushort value, int bitCount)
    {
        var bitBuffer = new BitBuffer();
        Assert.Throws<ArgumentOutOfRangeException>(() => bitBuffer.WriteBits(value, bitCount));
    }
#pragma warning restore xUnit1045 // Avoid using TheoryData type arguments that might not be serializable

    [Fact]
    public void WriteBits_WritesCorrectValues()
    {
        var bitBuffer = new BitBuffer();
        ushort value1 = 0b1000_0000_00;
        ushort value2 = 0b0100;
        ushort value3 = 0b0010_000;
        ushort value4 = 0b0010_0101_0101_1;
        ushort value5 = 0b0001_00;
        ushort value6 = 0b0100;

        bitBuffer.WriteBits(value1, 10);
        bitBuffer.WriteBits(value2, 4);
        bitBuffer.WriteBits(value3, 7);
        bitBuffer.WriteBits(value4, 13);
        bitBuffer.WriteBits(value5, 6);
        bitBuffer.WriteBits(value6, 4);

        var bitStream = bitBuffer.GetBitStream();

        AssertExtensions.BitsEqual($"{value1:B10}{value2:B4}{value3:B7}{value4:B13}{value5:B6}{value6:B4}", bitStream);
    }

    [Fact]
    public void WriteBits_WritesCorrectValues_ByteStream()
    {
        var bitBuffer = new BitBuffer();
        ushort value1 = 0b1000_0000_00;
        ushort value2 = 0b0100;
        ushort value3 = 0b0010_000;
        ushort value4 = 0b0010_0101_0101_1;
        ushort value5 = 0b0001_00;

        bitBuffer.WriteBits(value1, 10);
        bitBuffer.WriteBits(value2, 4);
        bitBuffer.WriteBits(value3, 7);
        bitBuffer.WriteBits(value4, 13);
        bitBuffer.WriteBits(value5, 6);

        var byteStream = bitBuffer.GetByteStream().ToArray();

        Assert.Equal(5, byteStream.Length);
        Assert.Equal(0x80, byteStream[0]);
        Assert.Equal(0x10, byteStream[1]);
        Assert.Equal(0x81, byteStream[2]);
        Assert.Equal(0x2A, byteStream[3]);
        Assert.Equal(0xC4, byteStream[4]);
    }

    [Fact]
    public void ByteStream_ReturnsZeroPaddedLastByte()
    {
        var bitBuffer = new BitBuffer();
        var value1 = 0xCACA_CACA;
        var value2 = 0b0100_1;

        bitBuffer.WriteBits(value1, 32);
        bitBuffer.WriteBits(value2, 5);

        var byteStream = bitBuffer.GetByteStream().ToArray();

        Assert.Equal(5, byteStream.Length);
        Assert.Equal(0xCA, byteStream[0]);
        Assert.Equal(0xCA, byteStream[1]);
        Assert.Equal(0xCA, byteStream[2]);
        Assert.Equal(0xCA, byteStream[3]);
        Assert.Equal(0b0100_1000, byteStream[4]);
    }

    [Fact]
    public void WriteBits_HasCorrectBitCountAfterAddingMultipleValues()
    {
        var bitBuffer = new BitBuffer();
        ushort value1 = 0b1000_0000_00;
        ushort value2 = 0b0100_0000_00;
        ushort value3 = 0b0010_0000_00;

        bitBuffer.WriteBits(value1, 10);
        bitBuffer.WriteBits(value2, 10);
        bitBuffer.WriteBits(value3, 10);

        Assert.Equal(30, bitBuffer.Count);
    }

    [Fact]
    public void ByteCount_ReturnsExpectedResult()
    {
        var bitBuffer = new BitBuffer();
        bitBuffer.WriteBits(0, 32);
        bitBuffer.WriteBits(0, 5);

        Assert.Equal(5, bitBuffer.ByteCount);
    }

    private static byte[] Bytes => [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19];
    public static TheoryData<byte[], int, bool, int, bool, byte[]> BytesData()
    {
        var data = new TheoryData<byte[], int, bool, int, bool, byte[]>();
        //start isFromEnd: false, end isFromEnd: false
        for (var start = 0; start < 5; start++)
        {
            for (var end = start; end < 10; end++)
            {
                var range = start..(start + end);
                data.Add(Bytes, range.Start.Value, range.Start.IsFromEnd, range.End.Value, range.End.IsFromEnd, Bytes[range]);
            }
        }

        //start isFromEnd: true, end isFromEnd: false
        for (var start = 0; start < 3; start++)
        {
            for (var end = Bytes.Length - start; end < Math.Min(end, Bytes.Length - 1); end++)
            {
                var range = ^start..(start + end);
                data.Add(Bytes, range.Start.Value, range.Start.IsFromEnd, range.End.Value, range.End.IsFromEnd, Bytes[range]);
            }
        }

        //start isFromEnd: false, end isFromEnd: true
        for (var start = 0; start < 3; start++)
        {
            for (var end = start; end < 3; end++)
            {
                var range = start..^(start + end);
                data.Add(Bytes, range.Start.Value, range.Start.IsFromEnd, range.End.Value, range.End.IsFromEnd, Bytes[range]);
            }
        }

        //start isFromEnd: true, end isFromEnd: true
        for (var end = 0; end < 3; end++)
        {
            for (var start = end + 1; start < 3; start++)
            {
                var range = ^start..^end;
                data.Add(Bytes, range.Start.Value, range.Start.IsFromEnd, range.End.Value, range.End.IsFromEnd, Bytes[range]);
            }
        }

        data.Add([0xFA, 0xCE], 0, false, 1, false, [0xFA]);
        data.Add([0xFA, 0xCE], 0, false, 2, false, [0xFA, 0xCE]);
        data.Add([0xFA, 0xCE], 1, false, 1, false, [0xCE]);
        return data;
    }

    [Theory]
    [MemberData(nameof(BytesData))]
    public void GetBytes(byte[] bytes, int start, bool fromEnd, int end, bool fromEnd2, byte[] expectedBytes)
    {
        var bitBuffer = new BitBuffer();
        foreach (var b in bytes)
        {
            bitBuffer.WriteBits(b, 8);
        }

        var destination = new byte[expectedBytes.Length];
        var written = bitBuffer.GetBytes(new(new(start, fromEnd), new(end, fromEnd2)), destination);

        Assert.Equal(expectedBytes.Length, written);
        Assert.Equal(expectedBytes, destination);
    }

    [Fact]
    public void GetBytes_WhenStartIsLessThanZero_ThrowsArgumentOutOfRangeException()
    {
        var bitBuffer = new BitBuffer();
        Assert.Throws<ArgumentOutOfRangeException>(() => bitBuffer.GetBytes(-1, 0, []));
    }

    [Fact]
    public void GetBytes_WhenLengthLessThanZero_ThrowsArgumentOutOfRangeException()
    {
        var bitBuffer = new BitBuffer();
        Assert.Throws<ArgumentOutOfRangeException>(() => bitBuffer.GetBytes(0, -1, []));
    }

    [Fact]
    public void GetBytes_WhenStartPlusLengthGreaterThanBitWriterByteCount_ThrowsArgumentOutOfRangeException()
    {
        var bitBuffer = new BitBuffer();
        bitBuffer.WriteBits(0xFA, 8);
        bitBuffer.WriteBits(0xCE, 8);
        Assert.Throws<ArgumentOutOfRangeException>(() => bitBuffer.GetBytes(1, 2, [0]));
    }

    [Fact]
    public void GetBytes_WhenDestinationIsNull_ThrowsArgumentException()
    {
        var bitBuffer = new BitBuffer();
        bitBuffer.WriteBits(0, 8);
        Assert.Throws<ArgumentException>(() => bitBuffer.GetBytes(0, 1, []));
    }
}

using System.Numerics;

namespace Quarer.Tests;

public class BitWriterTests
{
    [Fact]
    public void WriteBits_ValidValue_IsSuccessful()
    {
        var bitWriter = new BitWriter();
        ushort validValue = 1023;
        bitWriter.WriteBits(validValue, 10);
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
        var bitWriter = new BitWriter();
        var bits = UnboxInto<short>(value);
        bitWriter.WriteBits(bits, bitCount);
    }

    [Theory]
    [MemberData(nameof(ValidData_FitsWithin16Bits))]
    public void WriteBits_ValidValue_IsSuccessful_ushort(object value, int bitCount)
    {
        var bitWriter = new BitWriter();
        var bits = UnboxInto<ushort>(value);
        bitWriter.WriteBits(bits, bitCount);
    }

    [Theory]
    [MemberData(nameof(ValidData_FitsWithin16Bits))]
    [MemberData(nameof(ValidData_FitsWithin32Bits))]
    public void WriteBits_ValidValue_IsSuccessful_int(object value, int bitCount)
    {
        var bitWriter = new BitWriter();
        var bits = UnboxInto<int>(value);
        bitWriter.WriteBits(bits, bitCount);
    }

    [Theory]
    [MemberData(nameof(ValidData_FitsWithin16Bits))]
    [MemberData(nameof(ValidData_FitsWithin32Bits))]
    public void WriteBits_ValidValue_IsSuccessful_uint(object value, int bitCount)
    {
        var bitWriter = new BitWriter();
        var bits = UnboxInto<uint>(value);
        bitWriter.WriteBits(bits, bitCount);
    }

    [Theory]
    [MemberData(nameof(ValidData_FitsWithin16Bits))]
    [MemberData(nameof(ValidData_FitsWithin32Bits))]
    [MemberData(nameof(ValidData_FitsWithin64Bits))]
    public void WriteBits_ValidValue_IsSuccessful_long(object value, int bitCount)
    {
        var bitWriter = new BitWriter();
        var bits = UnboxInto<long>(value);
        bitWriter.WriteBits(bits, bitCount);
    }

    [Theory]
    [MemberData(nameof(ValidData_FitsWithin16Bits))]
    [MemberData(nameof(ValidData_FitsWithin32Bits))]
    [MemberData(nameof(ValidData_FitsWithin64Bits))]
    public void WriteBits_ValidValue_IsSuccessful_ulong(object value, int bitCount)
    {
        var bitWriter = new BitWriter();
        var bits = UnboxInto<ulong>(value);
        bitWriter.WriteBits(bits, bitCount);
    }

    [Theory]
    [InlineData(1024, 10)]
    [InlineData(256, 8)]
    [InlineData(16, 4)]
    [InlineData(7, 2)]
    public void WriteBits_ValueLargerThanSpecifiedBits_ShouldThrowArgumentOutOfRangeException(ushort value, int bitCount)
    {
        var bitWriter = new BitWriter();
        Assert.Throws<ArgumentOutOfRangeException>(() => bitWriter.WriteBits(value, bitCount));
    }
#pragma warning restore xUnit1045 // Avoid using TheoryData type arguments that might not be serializable

    [Fact]
    public void WriteBits_WritesCorrectValues()
    {
        var bitWriter = new BitWriter();
        ushort value1 = 0b1000_0000_00;
        ushort value2 = 0b0100;
        ushort value3 = 0b0010_000;
        ushort value4 = 0b0010_0101_0101_1;
        ushort value5 = 0b0001_00;
        ushort value6 = 0b0100;

        bitWriter.WriteBits(value1, 10);
        bitWriter.WriteBits(value2, 4);
        bitWriter.WriteBits(value3, 7);
        bitWriter.WriteBits(value4, 13);
        bitWriter.WriteBits(value5, 6);
        bitWriter.WriteBits(value6, 4);

        var bitStream = bitWriter.GetBitStream();

        AssertExtensions.BitsEqual($"{value1:B10}{value2:B4}{value3:B7}{value4:B13}{value5:B6}{value6:B4}", bitStream);
    }

    [Fact]
    public void WriteBits_WritesCorrectValues_ByteStream()
    {
        var bitWriter = new BitWriter();
        ushort value1 = 0b1000_0000_00;
        ushort value2 = 0b0100;
        ushort value3 = 0b0010_000;
        ushort value4 = 0b0010_0101_0101_1;
        ushort value5 = 0b0001_00;

        bitWriter.WriteBits(value1, 10);
        bitWriter.WriteBits(value2, 4);
        bitWriter.WriteBits(value3, 7);
        bitWriter.WriteBits(value4, 13);
        bitWriter.WriteBits(value5, 6);

        var byteStream = bitWriter.GetByteStream().ToArray();

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
        var bitWriter = new BitWriter();
        var value1 = 0xCACA_CACA;
        var value2 = 0b0100_1;

        bitWriter.WriteBits(value1, 32);
        bitWriter.WriteBits(value2, 5);

        var byteStream = bitWriter.GetByteStream().ToArray();

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
        var bitWriter = new BitWriter();
        ushort value1 = 0b1000_0000_00;
        ushort value2 = 0b0100_0000_00;
        ushort value3 = 0b0010_0000_00;

        bitWriter.WriteBits(value1, 10);
        bitWriter.WriteBits(value2, 10);
        bitWriter.WriteBits(value3, 10);

        Assert.Equal(30, bitWriter.Count);
    }

    [Fact]
    public void ByteCount_ReturnsExpectedResult()
    {
        var bitWriter = new BitWriter();
        bitWriter.WriteBits(0, 32);
        bitWriter.WriteBits(0, 5);

        Assert.Equal(5, bitWriter.ByteCount);
    }
}

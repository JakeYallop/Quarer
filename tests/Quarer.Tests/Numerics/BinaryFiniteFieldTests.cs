using Quarer.Numerics;

namespace Quarer.Tests.Numerics;

public class BinaryFiniteFieldTests
{
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(0b1111_0000, 0b0000_1111, 0b1111_1111)]
    [InlineData(0b1111_1111, 0b1111_1111, 0b0000_0000)]
    [InlineData(0b1010_1010, 0b0101_0101, 0b1111_1111)]
    [InlineData(0b1010_1010, 0b1010_1010, 0b0000_0000)]
    [InlineData(0b0000_0001, 0b0000_0000, 0b0000_0001)]
    public void AddSubtract_ReturnsExpectedResult(byte a, byte b, byte expected)
    {
        Assert.Equal(expected, BinaryFiniteField.Add(a, b));
        Assert.Equal(expected, BinaryFiniteField.Subtract(a, b));
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(0, 1, 0)]
    [InlineData(1, 0, 0)]
    [InlineData(20, 20, 13)]
    [InlineData(9, 9, 65)]
    [InlineData(255, 255, 226)]
    public void Multiply_ReturnsExpectedResult(byte a, byte b, byte expected) => Assert.Equal(expected, BinaryFiniteField.Multiply(a, b));

    [Theory]
    [InlineData(0, 1, 0)]
    [InlineData(255, 1, 255)]
    [InlineData(255, 255, 1)]
    [InlineData(30, 40, 201)]
    public void Divide_returnsExpectedResult(byte a, byte b, byte expected) => Assert.Equal(expected, BinaryFiniteField.Divide(a, b));

    [Fact]
    public void Divide_DivisorIsZero_ThrowsDivideByZeroException() => Assert.Throws<DivideByZeroException>(() => BinaryFiniteField.Divide(1, 0));

    [Theory]
    [InlineData(0, 0, 1)]
    [InlineData(255, 255, 1)]
    [InlineData(1, 1, 1)]
    [InlineData(30, 40, 235)]
    public void Pow_ReturnsExpectedResult(byte value, byte power, byte expected) => Assert.Equal(expected, BinaryFiniteField.Pow(value, power));

    [Theory]
    [InlineData(new byte[] { 0b01, 0b10, 0b11 }, new byte[] { 0b01, 0b10, 0b11 }, new byte[] { 0, 0, 0 })]
    [InlineData(new byte[] { 0b100, 0b001, 0b010, 0b011 }, new byte[] { 0b001, 0b010, 0b011 }, new byte[] { 4, 0, 0, 0 })]
    [InlineData(new byte[] { 0b1_0000, 0b01000, 0b0100, 0b0010 }, new byte[] { 0b0010, 0b0100, 0b0111 }, new byte[] { 16, 10, 0, 5 })]
    public void Add_ReturnsExpectedValues(byte[] p, byte[] q, byte[] expected)
    {
        var destination = new byte[expected.Length];
        var written = BinaryFiniteField.Add(p, q, destination);
        Assert.Equal(expected.Length, written);
        Assert.Equal(expected, destination);
    }

    [Fact]
    public void Multiply_ReturnsExpectedValues()
    {
        var p = new byte[] { 16, 8, 4, 2 };
        var q = new byte[] { 2, 4, 3 };
        var destination = new byte[6];
        var written = BinaryFiniteField.Multiply(p, q, destination);
        Assert.Equal(6, written);
        Assert.Equal((ReadOnlySpan<byte>)[32, 80, 24, 12, 4, 6], destination);
    }

    [Fact]
    public void Divide_ReturnsExpectedValues()
    {
        ReadOnlySpan<byte> message = [0x10, 0x20, 0x0C, 0x56, 0x61, 0x80, 0xEC, 0x11, 0xEC, 0x11, 0xEC, 0x11, 0xEC, 0x11, 0xEC, 0x11];
        var generator = BinaryFiniteField.G10;

        Span<byte> destination = stackalloc byte[128];
        var (written, separator) = BinaryFiniteField.Divide(message, generator, destination);

        var data = destination[0..written];
        var quotient = data[..separator];
        var remainder = data[separator..];

        Assert.Equal(26, written);
        Assert.Equal(16, separator);
        Assert.Equal((ReadOnlySpan<byte>)[0x10, 0x21, 0x6a, 0xd1, 0xf7, 0xd3, 0x75, 0x11, 0x01, 0xe9, 0xa5, 0x0f, 0x7a, 0xa2, 0x3f, 0x1a], quotient);
        Assert.Equal((ReadOnlySpan<byte>)[0xa5, 0x24, 0xd4, 0xc1, 0xed, 0x36, 0xc7, 0x87, 0x2c, 0x55], remainder);
    }

    [Theory]
    [InlineData(new byte[] { 1, 2, 3 }, 1, 0)]
    [InlineData(new byte[] { 1, 2, 3, 4 }, 1, 4)]
    [InlineData(new byte[] { 1, 2, 3, 1, 4 }, 1, 5)]
    [InlineData(new byte[] { 1, 2, 3 }, 2, 9)]
    [InlineData(new byte[] { 10, 20, 40, 60 }, 1, 10)]
    [InlineData(new byte[] { 10, 20, 40, 60 }, 2, 127)]
    public void Evaluate_ReturnsExpectedResult(byte[] polynomial, byte x, byte expected)
        => Assert.Equal(expected, BinaryFiniteField.Evaluate(polynomial, x));

}

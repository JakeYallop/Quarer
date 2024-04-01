using System.Numerics;

namespace Quarer.Tests;

public sealed class NumericModeEncoderTests
{
    private class TestBitWriter : BitWriter
    {
        public List<(ushort value, int bitCount)> WrittenBits { get; } = new List<(ushort, int)>();

        public override void WriteBits<T>(T value, int bitCount)
        {
            if (typeof(T) == typeof(ushort))
            {
                WrittenBits.Add((ushort.CreateChecked(value), bitCount));
                return;
            }
            throw new InvalidOperationException($"Expected '{nameof(UInt16)}', found '{typeof(T).Name}'");
        }
    }

    [Fact]
    public void Encode_WithNoRemainder_WritesCorrectValues()
    {
        var writer = new TestBitWriter();
        var encoder = new NumericModeEncoder(writer);
        encoder.Encode([1, 2, 3, 4, 5, 6]);

        Assert.Equal(2, writer.WrittenBits.Count);
        Assert.Equal((123, 10), writer.WrittenBits[0]);
        Assert.Equal((456, 10), writer.WrittenBits[1]);
    }

    [Fact]
    public void Encode_WithOneRemainder_WritesCorrectValues()
    {
        var writer = new TestBitWriter();
        var encoder = new NumericModeEncoder(writer);
        encoder.Encode([1, 2, 3, 4]);

        Assert.Equal(2, writer.WrittenBits.Count);
        Assert.Equal((123, 10), writer.WrittenBits[0]);
        Assert.Equal((4, 4), writer.WrittenBits[1]);
    }

    [Fact]
    public void Encode_WithTwoRemainders_WritesCorrectValues()
    {
        var writer = new TestBitWriter();
        var encoder = new NumericModeEncoder(writer);
        encoder.Encode([1, 2, 3, 4, 5]);

        Assert.Equal(2, writer.WrittenBits.Count);
        Assert.Equal((123, 10), writer.WrittenBits[0]);
        Assert.Equal((45, 7), writer.WrittenBits[1]);
    }
}

namespace Quarer.Tests;

public sealed class QrTerminatorBlockTests
{
    private static BitWriter GetWriter(int byteCount)
    {
        var writer = new BitWriter();
        for (var i = 0; i < byteCount; i++)
        {
            writer.WriteBits(byte.MaxValue, 8);
        }
        return writer;
    }

    [Fact]
    public void WriteTerminator_WritesTerminator()
    {
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.L);
        var initialCapacity = version.DataCodewordsCapacity - 3;
        var writer = GetWriter(initialCapacity);

        var count = writer.Count;
        QrTerminatorBlock.WriteTerminator(writer, version);
        Assert.Equal(initialCapacity + 1, writer.ByteCount);
        Assert.Equal(count + 4, writer.Count);
        AssertExtensions.BitsEqual("0000", writer.GetBitStream().TakeLast(4));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void WriteTerminator_AtCodewordCapacity_ButLastPartialByteHasSpace_WritesTerminator(int filledBitsInLastByte)
    {
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.M);
        var initialCapacity = version.DataCodewordsCapacity - 1;
        var writer = GetWriter(initialCapacity);
        writer.WriteBits(1 << (filledBitsInLastByte - 1), filledBitsInLastByte);

        var count = writer.Count;
        QrTerminatorBlock.WriteTerminator(writer, version);
        Assert.Equal(initialCapacity + 1, writer.ByteCount);
        Assert.Equal(count + 4, writer.Count);
        AssertExtensions.BitsEqual("0000", writer.GetBitStream().TakeLast(4));
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void WriteTerminator_AtCodewordCapacity_LastPartialByteHas4BitsOrLessSpace_FillsRemainingSpace(int filledBitsInLastByte)
    {
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.Q);
        var initialCapacity = version.DataCodewordsCapacity - 1;
        var writer = GetWriter(initialCapacity);
        writer.WriteBits(1 << (filledBitsInLastByte - 1), filledBitsInLastByte);
        var remainingBits = 8 - filledBitsInLastByte;

        var count = writer.Count;
        QrTerminatorBlock.WriteTerminator(writer, version);
        Assert.Equal(initialCapacity + 1, writer.ByteCount);
        Assert.Equal(count + remainingBits, writer.Count);
        AssertExtensions.BitsEqual(new string('0', remainingBits), writer.GetBitStream().TakeLast(remainingBits));
    }

    [Fact]
    public void WriteTerminator_AtCodewordCapacity_NoSpaceForTerminator_DoesNotWriteAnything()
    {
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.H);
        var writer = GetWriter(version.DataCodewordsCapacity);

        var count = writer.Count;
        QrTerminatorBlock.WriteTerminator(writer, version);
        Assert.Equal(version.DataCodewordsCapacity, writer.ByteCount);
        Assert.Equal(count, writer.Count);
    }

}

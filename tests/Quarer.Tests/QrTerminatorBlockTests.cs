namespace Quarer.Tests;

public sealed class QrTerminatorBlockTests
{
    private static BitBuffer GetWriter(int byteCount)
    {
        var buffer = new BitBuffer();
        for (var i = 0; i < byteCount; i++)
        {
            buffer.WriteBits(byte.MaxValue, 8);
        }
        return buffer;
    }

    [Fact]
    public void WriteTerminator_WritesTerminator()
    {
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.L);
        var initialCapacity = version.DataCodewordsCapacity - 3;
        var buffer = GetWriter(initialCapacity);

        var count = buffer.Count;
        QrTerminatorBlock.WriteTerminator(buffer, version);
        Assert.Equal(initialCapacity + 1, buffer.ByteCount);
        Assert.Equal(count + 4, buffer.Count);
        AssertExtensions.BitsEqual("0000", buffer.GetBitStream().TakeLast(4));
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
        var buffer = GetWriter(initialCapacity);
        buffer.WriteBits(1 << (filledBitsInLastByte - 1), filledBitsInLastByte);

        var count = buffer.Count;
        QrTerminatorBlock.WriteTerminator(buffer, version);
        Assert.Equal(initialCapacity + 1, buffer.ByteCount);
        Assert.Equal(count + 4, buffer.Count);
        AssertExtensions.BitsEqual("0000", buffer.GetBitStream().TakeLast(4));
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
        var buffer = GetWriter(initialCapacity);
        buffer.WriteBits(1 << (filledBitsInLastByte - 1), filledBitsInLastByte);
        var remainingBits = 8 - filledBitsInLastByte;

        var count = buffer.Count;
        QrTerminatorBlock.WriteTerminator(buffer, version);
        Assert.Equal(initialCapacity + 1, buffer.ByteCount);
        Assert.Equal(count + remainingBits, buffer.Count);
        AssertExtensions.BitsEqual(new string('0', remainingBits), buffer.GetBitStream().TakeLast(remainingBits));
    }

    [Fact]
    public void WriteTerminator_AtCodewordCapacity_NoSpaceForTerminator_DoesNotWriteAnything()
    {
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.H);
        var buffer = GetWriter(version.DataCodewordsCapacity);

        var count = buffer.Count;
        QrTerminatorBlock.WriteTerminator(buffer, version);
        Assert.Equal(version.DataCodewordsCapacity, buffer.ByteCount);
        Assert.Equal(count, buffer.Count);
    }

}

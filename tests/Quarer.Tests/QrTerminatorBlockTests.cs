﻿namespace Quarer.Tests;

public sealed class QrTerminatorBlockTests
{
    private static BitWriter GetBuffer(int byteCount)
    {
        var writer = new BitWriter();
        for (var i = 0; i < byteCount; i++)
        {
            writer.WriteBitsBigEndian(byte.MaxValue, 8);
        }
        return writer;
    }

    [Fact]
    public void WriteTerminator_WritesTerminator()
    {
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.L);
        var initialCapacity = version.DataCodewordsCapacity - 3;
        var writer = GetBuffer(initialCapacity);

        var count = writer.BitsWritten;
        QrTerminatorBlock.WriteTerminator(writer, version);
        Assert.Equal(initialCapacity + 1, writer.BytesWritten);
        Assert.Equal(count + 4, writer.BitsWritten);
        AssertExtensions.BitsEqual("0000", writer.Buffer.AsBitEnumerable().TakeLast(4));
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
        var writer = GetBuffer(initialCapacity);
        writer.WriteBitsBigEndian(1 << (filledBitsInLastByte - 1), filledBitsInLastByte);

        var count = writer.BitsWritten;
        QrTerminatorBlock.WriteTerminator(writer, version);
        Assert.Equal(initialCapacity + 1, writer.Buffer.ByteCount);
        Assert.Equal(count + 4, writer.BitsWritten);
        AssertExtensions.BitsEqual("0000", writer.Buffer.AsBitEnumerable().TakeLast(4));
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
        var writer = GetBuffer(initialCapacity);
        writer.WriteBitsBigEndian(1 << (filledBitsInLastByte - 1), filledBitsInLastByte);
        var remainingBits = 8 - filledBitsInLastByte;

        var count = writer.BitsWritten;
        QrTerminatorBlock.WriteTerminator(writer, version);
        Assert.Equal(initialCapacity + 1, writer.Buffer.ByteCount);
        Assert.Equal(count + remainingBits, writer.BitsWritten);
        AssertExtensions.BitsEqual(new string('0', remainingBits), writer.Buffer.AsBitEnumerable().TakeLast(remainingBits));
    }

    [Fact]
    public void WriteTerminator_AtCodewordCapacity_NoSpaceForTerminator_DoesNotWriteAnything()
    {
        var version = QrVersion.GetVersion(1, ErrorCorrectionLevel.H);
        var writer = GetBuffer(version.DataCodewordsCapacity);

        var count = writer.BitsWritten;
        QrTerminatorBlock.WriteTerminator(writer, version);
        Assert.Equal(version.DataCodewordsCapacity, writer.Buffer.ByteCount);
        Assert.Equal(count, writer.BitsWritten);
    }

}

using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace Quarer.Tests;

public class BitBufferMarshalTests
{
    [Theory]
    [InlineData(5, 10)]
    [InlineData(5, 0)]
    [InlineData(31, 33)]
    [InlineData(5, 33)]
    [InlineData(10, 5)]
    public void SetCount(int initial, int newCount)
    {
        var bitBuffer = new BitBuffer(initial);
        Assert.Equal(0, bitBuffer.Count);
        BitBufferMarshal.SetCount(bitBuffer, newCount);
        Assert.Equal(newCount, bitBuffer.Count);
    }

    [Fact]
    public void SetCount_BitCountLessThanZero_ThrowsArgumentOutOfRangeException()
    {
        var bitBuffer = new BitBuffer();
        Assert.Throws<ArgumentOutOfRangeException>(() => BitBufferMarshal.SetCount(bitBuffer, -1));
    }

    [Fact]
    public void ReadBytes_ReturnsExpectedBytes()
    {
        ReadOnlySpan<byte> data = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20];
        var buffer = Buffer(data);
        var expected = data.Slice(5, 12);
        var actual = BitBufferMarshal.GetBytes(buffer, 5, 12);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ReadBytes_WhenStartGreaterThanBufferLength_ThrowsArgumentOutOfRangeException()
    {
        var bitBuffer = Buffer([1, 2, 3]);
        Assert.Throws<ArgumentOutOfRangeException>(() => BitBufferMarshal.GetBytes(bitBuffer, 4, 1));
    }

    [Fact]
    public void ReadBytes_WhenLengthGreaterThanBufferLength_ThrowsArgumentOutOfRangeException()
    {
        var bitBuffer = Buffer([1, 2, 3]);
        Assert.Throws<ArgumentOutOfRangeException>(() => BitBufferMarshal.GetBytes(bitBuffer, 0, 5));
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
        data.Add([0xFA, 0xCE], 1, false, 0, true, [0xCE]);
        data.Add([0xFA, 0xCE], 1, false, 1, false, []);
        return data;
    }

    [Theory]
    [MemberData(nameof(BytesData))]
    public void GetBytes(byte[] bytes, int start, bool fromEnd, int end, bool fromEnd2, byte[] expectedBytes)
    {
        var bitBuffer = Buffer(bytes);
        var actualBytes = BitBufferMarshal.GetBytes(bitBuffer, new(new(start, fromEnd), new(end, fromEnd2)));

        Assert.Equal(expectedBytes.Length, actualBytes.Length);
        Assert.Equal(expectedBytes, actualBytes);
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
}

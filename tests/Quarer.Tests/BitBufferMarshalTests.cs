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
        ReadOnlySpan<byte> actual;

        if (BitConverter.IsLittleEndian)
        {
            actual = BitBufferMarshal.GetBytes(buffer, 5, 12, new byte[12]);
        }
        else
        {
            expected = ReverseEndiannessEvery8Bytes(data, 5, 12);
            actual = BitBufferMarshal.GetBytes(buffer, 5, 12);
        }

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

    [Theory]
    [InlineData(1, 3)]
    [InlineData(0, 4)]
    [InlineData(2, 2)]
    [InlineData(3, 1)]
    public void ReadBytes_WhenStartPlusLengthGreaterThanBufferLength_ThrowsArgumentOutOfRangeException(int start, int length)
    {
        var bitBuffer = Buffer([1, 2, 3]);
        Assert.Throws<ArgumentOutOfRangeException>(() => BitBufferMarshal.GetBytes(bitBuffer, start, length));
    }

    [Fact]
    public void ReadBytes_DestinationOverload_WhenStartGreaterThanBufferLength_ThrowsArgumentOutOfRangeException()
    {
        var bitBuffer = Buffer([1, 2, 3]);
        Assert.Throws<ArgumentOutOfRangeException>(() => BitBufferMarshal.GetBytes(bitBuffer, 4, 1, new byte[5]));
    }

    [Fact]
    public void ReadBytes_DestinationOverload_WhenLengthGreaterThanBufferLength_ThrowsArgumentOutOfRangeException()
    {
        var bitBuffer = Buffer([1, 2, 3]);
        Assert.Throws<ArgumentOutOfRangeException>(() => BitBufferMarshal.GetBytes(bitBuffer, 0, 5, new byte[5]));
    }

    [Theory]
    [InlineData(1, 3)]
    [InlineData(0, 4)]
    [InlineData(2, 2)]
    [InlineData(3, 1)]
    public void ReadBytes_DestinationOverload_WhenStartPlusLengthGreaterThanBufferLength_ThrowsArgumentOutOfRangeException(int start, int length)
    {
        var bitBuffer = Buffer([1, 2, 3]);
        Assert.Throws<ArgumentOutOfRangeException>(() => BitBufferMarshal.GetBytes(bitBuffer, start, length, new byte[5]));
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
        var destination = new byte[expectedBytes.Length];
        var written = BitBufferMarshal.GetBytes(bitBuffer, new(new(start, fromEnd), new(end, fromEnd2)), destination);

        Assert.Equal(expectedBytes.Length, written.Length);
        Assert.Equal(expectedBytes, destination);
    }

    public static readonly TheoryData<int, int> ValidStartAndLengthData = new()
    {
        { 0, 0 },
        { 0, 2 },
        { 0, 5 },
        { 1, 9 },
        { 1, 6 },
        { 1, 1 },
        { 1, 2 },
        { 1, 0 },
        { 1, 5 },
        { 5, 12 },
        { 5, 0 },
        { 5, 2 },
        { 5, 3 },
        { 7, 8 },
        { 7, 13 },
        { 15, 20 },
        { 16, 8 },
        { 16, 16 },
        { 17, 20 },
    };

    [Theory]
    [MemberData(nameof(ValidStartAndLengthData))]
    public void ReadBytesBigEndian_ReturnsExpectedBytes(int start, int length)
    {
        var data = Enumerable.Range(0, start + length).Select(x => (byte)x).ToArray().AsSpan();
        var buffer = Buffer(data);
        ReadOnlySpan<byte> expected = data.Slice(start, length);

        if (BitConverter.IsLittleEndian)
        {
            expected = ReverseEndiannessEvery8Bytes(data, start, length);
        }
        var actual = BitBufferMarshal.ReadBytesBigEndian(buffer, start, length);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(ValidStartAndLengthData))]
    public void ReadBytesLittleEndian_ReturnsExpectedBytes(int start, int length)
    {
        var data = Enumerable.Range(0, start + length).Select(x => (byte)x).ToArray().AsSpan();
        var buffer = Buffer(data);
        ReadOnlySpan<byte> expected = data.Slice(start, length);
        if (!BitConverter.IsLittleEndian)
        {
            expected = ReverseEndiannessEvery8Bytes(data, start, length);
        }

        var actual = BitBufferMarshal.ReadBytesLittleEndian(buffer, start, length, new byte[length]);

        Assert.Equal(expected, actual);
    }

    private static ReadOnlySpan<byte> ReverseEndiannessEvery8Bytes(ReadOnlySpan<byte> data, int start, int length)
    {
        var (totalUlongs, remainder) = int.DivRem(data.Length, 8);
        var totalLength = totalUlongs + (remainder == 0 ? 0 : 1);
        var alignedSize = new byte[totalLength * 8];
        data.CopyTo(alignedSize);
        var ulongs = MemoryMarshal.Cast<byte, ulong>(alignedSize);
        BinaryPrimitives.ReverseEndianness(ulongs, ulongs);
        return MemoryMarshal.Cast<ulong, byte>(ulongs).Slice(start, length);
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

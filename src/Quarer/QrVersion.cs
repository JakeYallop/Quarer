using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Quarer;

public class QrVersion : IEquatable<QrVersion>, IComparable<QrVersion>
{
    public const byte MinVersion = 1;
    public const byte MaxVersion = 40;
    public const int MaxModulesPerSide = 17 + (4 * MaxVersion);

    private ushort _totalCodewordsCapacity = 0;
    private ushort _dataCodewordsCapacity = 0;

    internal QrVersion(byte version, ErrorCorrectionLevel errorCorrectionLevel, QrErrorCorrectionBlocks errorCorrectionBlocks)
    {
        Version = version;
        AlignmentPatternCenters = AlignmentPatternCentersLookup[version - 1];
        ErrorCorrectionLevel = errorCorrectionLevel;
        ErrorCorrectionBlocks = errorCorrectionBlocks;
    }

    public static QrVersion GetVersion(byte version, ErrorCorrectionLevel errorCorrectionLevel)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(version, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(version, 40);

        return QrVersionLookup.GetVersion(version, errorCorrectionLevel);
    }

    public byte Version { get; }
    public ImmutableArray<byte> AlignmentPatternCenters { get; }
    public ErrorCorrectionLevel ErrorCorrectionLevel { get; }
    public ushort TotalCodewords
    {
        get
        {
            if (_totalCodewordsCapacity is not 0)
            {
                return _totalCodewordsCapacity;
            }

            var totalDataCodewords = 0;
            var totalCount = 0;
            foreach (var item in ErrorCorrectionBlocks.Blocks)
            {
                totalDataCodewords += item.DataCodewordsPerBlock * item.BlockCount;
                totalCount += item.BlockCount;
            }
            var totalCodewords = (ushort)(totalDataCodewords + (totalCount * ErrorCorrectionBlocks.ErrorCorrectionCodewordsPerBlock));
            Interlocked.CompareExchange(ref _totalCodewordsCapacity, totalCodewords, 0);
            return _totalCodewordsCapacity;
        }
    }

    public ushort DataCodewordsCapacity
    {
        get
        {
            if (_dataCodewordsCapacity is not 0)
            {
                return _dataCodewordsCapacity;
            }

            var total = 0;
            foreach (var item in ErrorCorrectionBlocks.Blocks)
            {
                total += item.DataCodewordsPerBlock * item.BlockCount;
            }
            Interlocked.CompareExchange(ref _dataCodewordsCapacity, (ushort)total, 0);
            return _dataCodewordsCapacity;
        }
    }

    public byte ModulesPerSide => (byte)(17 + (byte)(4 * Version));

    public QrErrorCorrectionBlocks ErrorCorrectionBlocks { get; }

    public static bool operator ==(QrVersion left, QrVersion right) => left.Equals(right);
    public static bool operator !=(QrVersion left, QrVersion right) => !(left == right);

    public int CompareTo(QrVersion? other)
    {
        if (other is null)
        {
            return 1;
        }

        var versionComparison = Version.CompareTo(other.Version);
        return versionComparison is not 0 ? versionComparison : ErrorCorrectionLevel.CompareTo(other.ErrorCorrectionLevel);
    }

    /// <inheritdoc cref="object.Equals(object?)" />
    public override bool Equals([NotNullWhen(true)] object? obj) => obj != null && obj is QrVersion qrVersion && Equals(qrVersion);

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true" /> if the current object is equal to <paramref name="other"/>; otherwise, <see langword="false" />.</returns>
    public bool Equals([NotNullWhen(true)] QrVersion? other) => other is not null && Version == other.Version && ErrorCorrectionLevel == other.ErrorCorrectionLevel;
    public override int GetHashCode() => Version.GetHashCode();

    public class QrErrorCorrectionBlock(byte blockCount, ushort dataCodewordsPerBlock) : IEquatable<QrErrorCorrectionBlock>
    {
        //TODO: Rename to Count?
        public byte BlockCount { get; } = blockCount;
        public ushort DataCodewordsPerBlock { get; } = dataCodewordsPerBlock;

        public static bool operator ==(QrErrorCorrectionBlock left, QrErrorCorrectionBlock right) => left.Equals(right);
        public static bool operator !=(QrErrorCorrectionBlock left, QrErrorCorrectionBlock right) => !(left == right);
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is QrErrorCorrectionBlock block && Equals(block);
        public bool Equals([NotNullWhen(true)] QrErrorCorrectionBlock? other) => other is not null && BlockCount == other.BlockCount && DataCodewordsPerBlock == other.DataCodewordsPerBlock;
        public override int GetHashCode() => HashCode.Combine(BlockCount, DataCodewordsPerBlock);
    }

    public class QrErrorCorrectionBlocks : IEquatable<QrErrorCorrectionBlocks>
    {
        private ushort _blockCount = 0;
        private ushort _maxDataCodewordsInBlock = 0;

        public QrErrorCorrectionBlocks(ushort errorCorrectionCodewordsPerBlock, ImmutableArray<QrErrorCorrectionBlock> blocks)
        {
            ErrorCorrectionCodewordsPerBlock = errorCorrectionCodewordsPerBlock;
            Blocks = blocks;
        }

        public ushort ErrorCorrectionCodewordsPerBlock { get; }
        public ImmutableArray<QrErrorCorrectionBlock> Blocks { get; }
        public ushort TotalBlockCount
        {
            get
            {
                if (_blockCount is not 0)
                {
                    return _blockCount;
                }

                ushort count = 0;
                foreach (var item in Blocks)
                {
                    count += item.BlockCount;
                }
                Interlocked.CompareExchange(ref _blockCount, count, 0);
                return _blockCount;
            }
        }

        public ushort MaxDataCodewordsInBlock
        {
            get
            {
                if (_maxDataCodewordsInBlock is not 0)
                {
                    return _maxDataCodewordsInBlock;
                }

                ushort max = 0;
                foreach (var item in Blocks)
                {
                    if (item.DataCodewordsPerBlock > max)
                    {
                        max = item.DataCodewordsPerBlock;
                    }
                }
                Interlocked.CompareExchange(ref _maxDataCodewordsInBlock, max, 0);
                return _maxDataCodewordsInBlock;
            }
        }

        public IEnumerable<QrErrorCorrectionBlock> EnumerateIndividualBlocks()
        {
            foreach (var b in Blocks)
            {
                for (var i = 0; i < b.BlockCount; i++)
                {
                    yield return b;
                }
            }
        }

        public static bool operator ==(QrErrorCorrectionBlocks left, QrErrorCorrectionBlocks right) => left.Equals(right);
        public static bool operator !=(QrErrorCorrectionBlocks left, QrErrorCorrectionBlocks right) => !(left == right);
        public override bool Equals(object? obj) => obj is QrErrorCorrectionBlocks blocks && Equals(blocks);
        public bool Equals(QrErrorCorrectionBlocks? other) => other is not null && ErrorCorrectionCodewordsPerBlock == other.ErrorCorrectionCodewordsPerBlock;
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(ErrorCorrectionCodewordsPerBlock);
            foreach (var b in Blocks)
            {
                hashCode.Add(b);
            }
            return hashCode.ToHashCode();
        }
    }

    private static readonly ImmutableArray<ImmutableArray<byte>> AlignmentPatternCentersLookup =
    [
        [], // version 1
        [6, 18], // version 2
        [6, 22], // version 3
        [6, 26], // version 4
        [6, 30], // version 5
        [6, 34], // version 6
        [6, 22, 38], // version 7
        [6, 24, 42], // version 8
        [6, 26, 46], // version 9
        [6, 28, 50], // version 10
        [6, 30, 54], // version 11
        [6, 32, 58], // version 12
        [6, 34, 62], // version 13
        [6, 26, 46, 66], // version 14
        [6, 26, 48, 70], // version 15
        [6, 26, 50, 74], // version 16
        [6, 30, 54, 78], // version 17
        [6, 30, 56, 82], // version 18
        [6, 30, 58, 86], // version 19
        [6, 34, 62, 90], // version 20
        [6, 28, 50, 72, 94], // version 21
        [6, 26, 50, 74, 98], // version 22
        [6, 30, 54, 78, 102], // version 23
        [6, 28, 54, 80, 106], // version 24
        [6, 32, 58, 84, 110], // version 25
        [6, 30, 58, 86, 114], // version 26
        [6, 34, 62, 90, 118], // version 27
        [6, 26, 50, 74, 98, 122], // version 28
        [6, 30, 54, 78, 102, 126], // version 29
        [6, 26, 52, 78, 104, 130], // version 30
        [6, 30, 56, 82, 108, 134], // version 31
        [6, 34, 60, 86, 112, 138], // version 32
        [6, 30, 58, 86, 114, 142], // version 33
        [6, 34, 62, 90, 118, 146], // version 34
        [6, 30, 54, 78, 102, 126, 150], // version 35
        [6, 24, 50, 76, 102, 128, 154], // version 36
        [6, 28, 54, 80, 106, 132, 158], // version 37
        [6, 32, 58, 84, 110, 136, 162], // version 38
        [6, 26, 54, 82, 110, 138, 166], // version 39
        [6, 30, 58, 86, 114, 142, 170], // version 40
    ];
}

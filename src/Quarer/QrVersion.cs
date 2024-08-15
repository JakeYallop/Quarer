using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Quarer;

public class QrVersion : IEquatable<QrVersion>, IComparable<QrVersion>
{
    public const byte MinVersion = 1;
    public const byte MaxVersion = 40;

    private ushort _totalCodewordsCapacity = 0;
    private ushort _dataCodewordsCapacity = 0;

    internal QrVersion(byte version, ErrorCorrectionLevel errorCorrectionLevel, QrErrorCorrectionBlocks errorCorrectionBlocks)
    {
        Version = version;
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
        //TODO: Tests
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
}

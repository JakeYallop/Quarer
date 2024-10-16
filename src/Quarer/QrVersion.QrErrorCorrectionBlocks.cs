using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Quarer;

public sealed partial class QrVersion
{
    /// <summary>
    /// A set of error correction blocks of potentially different sizes that appear within a QR code.
    /// </summary>
    public class QrErrorCorrectionBlocks : IEquatable<QrErrorCorrectionBlocks>
    {
#pragma warning disable IDE0032 // Use auto property
        private int _blockCount = 0;
        private int _maxDataCodewordsInBlock = 0;
        private int _totalDataCodewords = 0;
#pragma warning restore IDE0032 // Use auto property

        internal QrErrorCorrectionBlocks(ushort errorCorrectionCodewordsPerBlock, ImmutableArray<QrErrorCorrectionBlock> blocks)
        {
            ErrorCorrectionCodewordsPerBlock = errorCorrectionCodewordsPerBlock;
            Blocks = blocks;
        }

        /// <summary>
        /// The number of error correction codewords in all of the error blocks.
        /// </summary>
        public int ErrorCorrectionCodewordsPerBlock { get; }
        /// <summary>
        /// The set of error correction blocks.
        /// </summary>
        public ImmutableArray<QrErrorCorrectionBlock> Blocks { get; }
        /// <summary>
        /// The total number of error correction blocks in this set.
        /// </summary>
        public int TotalBlockCount
        {
            get
            {
                if (_blockCount is not 0)
                {
                    return _blockCount;
                }

                var count = 0;
                foreach (var item in Blocks)
                {
                    count += item.Count;
                }
                Interlocked.CompareExchange(ref _blockCount, count, 0);
                return _blockCount;
            }
        }

        /// <summary>
        /// The maximum number of data codewords in a single block out of all the blocks in the set.
        /// </summary>
        public int MaxDataCodewordsInBlock
        {
            get
            {
                if (_maxDataCodewordsInBlock is not 0)
                {
                    return _maxDataCodewordsInBlock;
                }

                var max = 0;
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

        /// <summary>
        /// The total number of data codewords between all of the error blocks after repetition.
        /// </summary>
        public int DataCodewordsCount
        {
            get
            {
                if (_totalDataCodewords is not 0)
                {
                    return _totalDataCodewords;
                }

                var total = 0;
                foreach (var item in Blocks)
                {
                    total += item.Count * item.DataCodewordsPerBlock;
                }
                Interlocked.CompareExchange(ref _totalDataCodewords, total, 0);
                return _totalDataCodewords;
            }
        }

        /// <summary>
        /// Enumerates each individual block up to <see cref="QrErrorCorrectionBlock.Count"/> times, in round-robin order.
        /// </summary>
        public IEnumerable<QrErrorCorrectionBlock> EnumerateIndividualBlocks() =>
            new IndividualBlocksEnumerator(Blocks);

        private struct IndividualBlocksEnumerator(ImmutableArray<QrErrorCorrectionBlock> blocks) : IEnumerable<QrErrorCorrectionBlock>, IEnumerator<QrErrorCorrectionBlock>
        {
            private readonly ImmutableArray<QrErrorCorrectionBlock> _blocks = blocks;
            private int _blockIndex;
            private int _currentIndex;
            public readonly QrErrorCorrectionBlock Current => _blocks[_blockIndex];
            readonly object? IEnumerator.Current => Current;
            public readonly void Dispose() { }

            public bool MoveNext()
            {
                if (_currentIndex >= _blocks[_blockIndex].Count)
                {
                    _blockIndex++;
                    _currentIndex = 0;
                }
                _currentIndex++;

                return _blockIndex < _blocks.Length;
            }

            public void Reset()
            {
                _blockIndex = 0;
                _currentIndex = 0;
            }

            public readonly IEnumerator<QrErrorCorrectionBlock> GetEnumerator() => this;

            readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        /// <summary>
        /// Returns a value indicating if two <see cref="QrErrorCorrectionBlocks"/> instances are equal.
        /// </summary>
        public static bool operator ==(QrErrorCorrectionBlocks? left, QrErrorCorrectionBlocks? right) => left is null ? right is null : left.Equals(right);
        /// <summary>
        /// Returns a value indicating if two <see cref="QrErrorCorrectionBlocks"/> instances are not equal.
        /// </summary>
        public static bool operator !=(QrErrorCorrectionBlocks? left, QrErrorCorrectionBlocks? right) => !(left == right);

        /// <inheritdoc />
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is QrErrorCorrectionBlocks blocks && Equals(blocks);
        /// <inheritdoc />
        public bool Equals([NotNullWhen(true)] QrErrorCorrectionBlocks? other) =>
            other is not null &&
            ErrorCorrectionCodewordsPerBlock == other.ErrorCorrectionCodewordsPerBlock &&
            TotalBlockCount == other.TotalBlockCount &&
            DataCodewordsCount == other.DataCodewordsCount;
        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(ErrorCorrectionCodewordsPerBlock);
            hashCode.Add(TotalBlockCount);
            hashCode.Add(DataCodewordsCount);
            foreach (var b in Blocks)
            {
                hashCode.Add(b);
            }
            return hashCode.ToHashCode();
        }
    }
}

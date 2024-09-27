﻿using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Quarer;

public sealed partial class QrVersion
{
    public class QrErrorCorrectionBlocks(ushort errorCorrectionCodewordsPerBlock, ImmutableArray<QrErrorCorrectionBlock> blocks) : IEquatable<QrErrorCorrectionBlocks>
    {
        private ushort _blockCount = 0;
        private ushort _maxDataCodewordsInBlock = 0;

        public ushort ErrorCorrectionCodewordsPerBlock { get; } = errorCorrectionCodewordsPerBlock;
        public ImmutableArray<QrErrorCorrectionBlock> Blocks { get; } = blocks;
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
                    count += item.Count;
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

        public static bool operator ==(QrErrorCorrectionBlocks? left, QrErrorCorrectionBlocks? right) => left is null ? right is null : left.Equals(right);
        public static bool operator !=(QrErrorCorrectionBlocks? left, QrErrorCorrectionBlocks? right) => !(left == right);
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is QrErrorCorrectionBlocks blocks && Equals(blocks);
        public bool Equals([NotNullWhen(true)] QrErrorCorrectionBlocks? other) => other is not null && ErrorCorrectionCodewordsPerBlock == other.ErrorCorrectionCodewordsPerBlock;
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

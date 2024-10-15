using System.Diagnostics.CodeAnalysis;

namespace Quarer;

public sealed partial class QrVersion
{
    /// <summary>
    /// Represents an error correction block in a QR Code. A block with <see cref="DataCodewordsPerBlock"/> is repeated up to <see cref="Count"/> times.
    /// </summary>
    public class QrErrorCorrectionBlock : IEquatable<QrErrorCorrectionBlock>
    {
        internal QrErrorCorrectionBlock(byte blockCount, ushort dataCodewordsPerBlock)
        {
            Count = blockCount;
            DataCodewordsPerBlock = dataCodewordsPerBlock;
        }

        /// <summary>
        /// The number of times an error correction block with these attributes is repeated.
        /// </summary>
        public byte Count { get; }
        /// <summary>
        /// The number of data codewords in this block.
        /// </summary>
        public ushort DataCodewordsPerBlock { get; }

        /// <summary>
        /// Returns a value indicating if two <see cref="QrErrorCorrectionBlock"/> instances are equal.
        /// </summary>
        public static bool operator ==(QrErrorCorrectionBlock? left, QrErrorCorrectionBlock? right) => left is null ? right is null : left.Equals(right);
        /// <summary>
        /// Returns a value indicating if two <see cref="QrErrorCorrectionBlock"/> instances are not equal.
        /// </summary>
        public static bool operator !=(QrErrorCorrectionBlock? left, QrErrorCorrectionBlock? right) => !(left == right);

        /// <inheritdoc cref="object.Equals(object?)" />
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is QrErrorCorrectionBlock block && Equals(block);

        /// <inheritdoc />
        public bool Equals([NotNullWhen(true)] QrErrorCorrectionBlock? other) => other is not null && Count == other.Count && DataCodewordsPerBlock == other.DataCodewordsPerBlock;
        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Count, DataCodewordsPerBlock);
    }
}

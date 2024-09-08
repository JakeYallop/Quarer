using System.Diagnostics.CodeAnalysis;

namespace Quarer;

public sealed partial class QrVersion
{
    public class QrErrorCorrectionBlock(byte blockCount, ushort dataCodewordsPerBlock) : IEquatable<QrErrorCorrectionBlock>
    {
        public byte Count { get; } = blockCount;
        public ushort DataCodewordsPerBlock { get; } = dataCodewordsPerBlock;

        public static bool operator ==(QrErrorCorrectionBlock? left, QrErrorCorrectionBlock? right) => left is null ? right is null : left.Equals(right);
        public static bool operator !=(QrErrorCorrectionBlock? left, QrErrorCorrectionBlock? right) => !(left == right);
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is QrErrorCorrectionBlock block && Equals(block);
        public bool Equals([NotNullWhen(true)] QrErrorCorrectionBlock? other) => other is not null && Count == other.Count && DataCodewordsPerBlock == other.DataCodewordsPerBlock;
        public override int GetHashCode() => HashCode.Combine(Count, DataCodewordsPerBlock);
    }
}

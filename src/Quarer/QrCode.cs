using System.Diagnostics.CodeAnalysis;

namespace Quarer;

public sealed class QrCode(QrVersion version, MaskPattern maskPattern, BitMatrix data) : IEquatable<QrCode>
{
    public QrVersion Version { get; } = version;
    public MaskPattern MaskPattern { get; } = maskPattern;
    public BitMatrix Data { get; } = data;
    public ErrorCorrectionLevel ErrorCorrectionLevel => Version.ErrorCorrectionLevel;
    public int Width => Version.ModulesPerSide;
    public int Height => Version.ModulesPerSide;

    public static bool operator ==(QrCode? left, QrCode? right) => left is null ? right is null : left.Equals(right);
    public static bool operator !=(QrCode? left, QrCode? right) => !(left == right);

    public bool Equals([NotNullWhen(true)] QrCode? other) => other is not null && Version == other.Version && MaskPattern == other.MaskPattern && Data == other.Data;
    public override bool Equals(object? obj) => obj is QrCode other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Version, MaskPattern, Data);
}

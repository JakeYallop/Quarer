using System.Collections.Immutable;
using System.Diagnostics;

namespace Quarer;

/// <summary>
/// Used for determining which modules within a QR Code for a given QR Code version are function modules.
/// </summary>
/// <remarks>
/// Maintains a cache of the information for each QR Code version, that is lazily populated the first time a version is requested.
/// </remarks>
public sealed class FunctionModules
{
    private readonly ByteMatrix _matrix;
    private readonly ImmutableArray<ImmutableArray<Range>> _maskableRowSegments;
    private FunctionModules(ByteMatrix matrix, ImmutableArray<ImmutableArray<Range>> maskableRowSegments)
    {
        _matrix = matrix;
        _maskableRowSegments = maskableRowSegments;
    }

    /// <summary>
    /// Determines if the module at the specified <paramref name="x"/> and <paramref name="y"/> coordinates is a function module.
    /// Also returns true for the blocked version and format information modules.
    /// </summary>
    public bool IsFunctionModule(int x, int y) => _matrix[x, y] != 0;
    /// <summary>
    /// Gets a set of segments that represent the maskable parts of a given row within a symbol. This will include all modules that
    /// are not function modules, or version and format information.
    /// </summary>
    public ReadOnlySpan<Range> GetMaskableSegments(int y) => _maskableRowSegments[y].AsSpan();

    /// <summary>
    /// Gets the function modules (and version and format information) matrix for the specified <paramref name="version"/>.
    /// </summary>
    public static FunctionModules GetForVersion(QrVersion version) => FunctionModuleMatrixCache.GetFunctionModulesMatrix(version);

    internal static class FunctionModuleMatrixCache
    {
#if NET9_0_OR_GREATER
        private static readonly Lock Lock = new();
#else
        private static readonly object Lock = new();
#endif
        public static readonly FunctionModules[] Cache = new FunctionModules[40];

        public static FunctionModules GetFunctionModulesMatrix(QrVersion version)
        {
            var index = version.Version - 1;
            var matrix = Cache[index];
            if (matrix is not null)
            {
                return matrix;
            }

            lock (Lock)
            {
                var newMatrix = new TrackingMatrix(version.Width, version.Height);

                QrSymbolBuilder.EncodePositionDetectionPattern(newMatrix, PositionDetectionPatternLocation.TopLeft);
                QrSymbolBuilder.EncodePositionDetectionPattern(newMatrix, PositionDetectionPatternLocation.TopRight);
                QrSymbolBuilder.EncodePositionDetectionPattern(newMatrix, PositionDetectionPatternLocation.BottomLeft);
                QrSymbolBuilder.EncodePositionAdjustmentPatterns(newMatrix, version);
                QrSymbolBuilder.EncodeTimingPatterns(newMatrix);
                QrSymbolBuilder.EncodeStaticDarkModule(newMatrix);
                QrSymbolBuilder.EncodeFormatInformation(newMatrix, ErrorCorrectionLevel.M, MaskPattern.PatternZero_Checkerboard);
                QrSymbolBuilder.EncodeVersionInformation(newMatrix, version);

                var maskableRowSegmentsBuilder = ImmutableArray.CreateBuilder<ImmutableArray<Range>>(newMatrix.Height);
                var rowBuilder = ImmutableArray.CreateBuilder<Range>(8);
                for (var y = 0; y < newMatrix.Height; y++)
                {
                    var isAtEnd = false;
                    var reader = new RowModuleReader(newMatrix.Matrix.GetRow(y));
                    while (!isAtEnd)
                    {
                        isAtEnd = !reader.TryAdvanceToNextEmptyModule(out var segmentStart);

                        if (!isAtEnd)
                        {
                            isAtEnd = !reader.TryConsumeEmptyModules(out var endIndex);
                            rowBuilder.Add(new Range(new(segmentStart), new(endIndex)));
                        }
                    }

                    maskableRowSegmentsBuilder.Add(rowBuilder.DrainToImmutable());
                }

                Debug.Assert(maskableRowSegmentsBuilder.Count == newMatrix.Height);
                var functionModulesMatrix = new FunctionModules(newMatrix.Matrix, maskableRowSegmentsBuilder.ToImmutable());
                Cache[index] = functionModulesMatrix;
                return functionModulesMatrix;
            }
        }

        private ref struct RowModuleReader(ReadOnlySpan<byte> row)
        {
            private int _index;
            private readonly ReadOnlySpan<byte> _row = row;

            public bool TryAdvanceToNextEmptyModule(out int index)
            {
                while (_index < _row.Length && _row[_index] != 0)
                {
                    _index++;
                }
                index = _index;
                return index < _row.Length;
            }

            public bool TryConsumeEmptyModules(out int endIndex)
            {
                while (_index < _row.Length && _row[_index] == 0)
                {
                    _index++;
                }

                endIndex = _index;
                return true;
            }
        }

        private class TrackingMatrix(int width, int height) : ByteMatrix(width, height)
        {
            public override byte this[int x, int y]
            {
                get => base[x, y];
                set => Matrix[x, y] = 1;
            }

            public ByteMatrix Matrix { get; } = new(width, height);
        }
    }
}

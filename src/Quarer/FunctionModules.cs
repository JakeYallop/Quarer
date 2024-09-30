using System.Collections.Immutable;
using System.Diagnostics;

namespace Quarer;

public sealed class FunctionModules
{
    private readonly ByteMatrix _matrix;
    private readonly ImmutableArray<ImmutableArray<Range>> _maskableRowSegments;
    private FunctionModules(ByteMatrix matrix, ImmutableArray<ImmutableArray<Range>> maskableRowSegments)
    {
        _matrix = matrix;
        _maskableRowSegments = maskableRowSegments;
    }

    public bool IsFunctionModule(int x, int y) => _matrix[x, y] != 0;
    public ReadOnlySpan<Range> GetMaskableSegments(int row) => _maskableRowSegments[row].AsSpan();
    public static FunctionModules GetForVersion(QrVersion version) => FunctionModuleMatrixCache.GetFunctionModulesMatrix(version);

    private readonly struct Segment(int start, int end)
    {
        public int Start { get; } = start;
        public int End { get; } = end;

        public Range ToRange() => new(new(Start), new(End));
    }

    internal static class FunctionModuleMatrixCache
    {
        private static readonly Lock Lock = new();
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
                var newMatrix = new TrackingMatrix(version.ModulesPerSide, version.ModulesPerSide);

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
                    var reader = new RowModuleReader(newMatrix.GetRow(y));
                    while (!isAtEnd)
                    {
                        isAtEnd = reader.TryAdvanceToNextEmptyModule(out var segmentStart);

                        if (!isAtEnd)
                        {
                            isAtEnd = reader.TryConsumeEmptyModules(out var endIndex);
                            rowBuilder.Add(new Range(new(segmentStart), new(endIndex)));
                        }
                    }

                    maskableRowSegmentsBuilder.Add(rowBuilder.ToImmutable());
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

namespace Quarer;

public sealed class FunctionModules
{
    private readonly ByteMatrix _matrix;
    private FunctionModules(ByteMatrix matrix)
    {
        _matrix = matrix;
    }

    public bool IsFunctionModule(int x, int y) => _matrix[x, y] != 0;
    public static FunctionModules GetForVersion(QrVersion version) => FunctionModuleMatrixCache.GetFunctionModulesMatrix(version);

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

                var functionModulesMatrix = new FunctionModules(newMatrix.Matrix);
                Cache[index] = functionModulesMatrix;
                return functionModulesMatrix;
            }
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

using static Quarer.Tests.MatrixTestUtilities;

namespace Quarer.Tests;

public partial class QrCodeTests
{
    [Fact]
    public void Create_1M()
    {
        var data = "01234567";
        var code = QrCode.Create(data);
        Assert.Equal(ErrorCorrectionLevel.M, code.ErrorCorrectionLevel);
        Assert.Equal(1, code.Version.Version);
        Assert.Equal(MaskPattern.PatternZero_Checkerboard, code.MaskPattern);
        Assert.Equal("""
            X X X X X X X - - - X X X - X X X X X X X
            X - - - - - X - X X X - - - X - - - - - X
            X - X X X - X - - X X - - - X - X X X - X
            X - X X X - X - - X - X X - X - X X X - X
            X - X X X - X - X X - X X - X - X X X - X
            X - - - - - X - - - - X - - X - - - - - X
            X X X X X X X - X - X - X - X X X X X X X
            - - - - - - - - - - - - - - - - - - - - -
            X - X - X - X - - - X - X - - - X - - X -
            X X - X - - - - X - X X - X - X - - - X -
            - - - X X - X X X - X X - X X X - X X X -
            X X - - X X - X - X - X X X - X X - - X -
            - - X - - X X X - X X X - X X X - - - - X
            - - - - - - - - X - X - - - X - - - - X -
            X X X X X X X - - - - - X - - - X - - - X
            X - - - - - X - - - X - - - X - - X - X X
            X - X X X - X - X X X - X - X - X X X - X
            X - X X X - X - - X - X - X - X - X X X -
            X - X X X - X - X X - X - X X X - - X - X
            X - - - - - X - - - - X X X - X X X - - -
            X X X X X X X - X - - X - X X X - - X - X
            """, MatrixToString(code.Data));
    }

    [Fact]
    public void TryCreate_1M()
    {
        var data = "01234567";
        var result = QrCode.TryCreate(data);
        Assert.True(result.Success);
        var code = result.Value;
        Assert.Equal(ErrorCorrectionLevel.M, code.ErrorCorrectionLevel);
        Assert.Equal(1, code.Version.Version);
        Assert.Equal(MaskPattern.PatternZero_Checkerboard, code.MaskPattern);
        Assert.Equal("""
            X X X X X X X - - - X X X - X X X X X X X
            X - - - - - X - X X X - - - X - - - - - X
            X - X X X - X - - X X - - - X - X X X - X
            X - X X X - X - - X - X X - X - X X X - X
            X - X X X - X - X X - X X - X - X X X - X
            X - - - - - X - - - - X - - X - - - - - X
            X X X X X X X - X - X - X - X X X X X X X
            - - - - - - - - - - - - - - - - - - - - -
            X - X - X - X - - - X - X - - - X - - X -
            X X - X - - - - X - X X - X - X - - - X -
            - - - X X - X X X - X X - X X X - X X X -
            X X - - X X - X - X - X X X - X X - - X -
            - - X - - X X X - X X X - X X X - - - - X
            - - - - - - - - X - X - - - X - - - - X -
            X X X X X X X - - - - - X - - - X - - - X
            X - - - - - X - - - X - - - X - - X - X X
            X - X X X - X - X X X - X - X - X X X - X
            X - X X X - X - - X - X - X - X - X X X -
            X - X X X - X - X X - X - X X X - - X - X
            X - - - - - X - - - - X X X - X X X - - -
            X X X X X X X - X - - X - X X X - - X - X
            """, MatrixToString(code.Data));
    }
}

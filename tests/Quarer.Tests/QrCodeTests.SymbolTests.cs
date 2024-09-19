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
            XXXXXXX---XXX-XXXXXXX
            X-----X-XXX---X-----X
            X-XXX-X--XX---X-XXX-X
            X-XXX-X--X-XX-X-XXX-X
            X-XXX-X-XX-XX-X-XXX-X
            X-----X----X--X-----X
            XXXXXXX-X-X-X-XXXXXXX
            ---------------------
            X-X-X-X---X-X---X--X-
            XX-X----X-XX-X-X---X-
            ---XX-XXX-XX-XXX-XXX-
            XX--XX-X-X-XXX-XX--X-
            --X--XXX-XXX-XXX----X
            --------X-X---X----X-
            XXXXXXX-----X---X---X
            X-----X---X---X--X-XX
            X-XXX-X-XXX-X-X-XXX-X
            X-XXX-X--X-X-X-X-XXX-
            X-XXX-X-XX-X-XXX--X-X
            X-----X----XXX-XXX---
            XXXXXXX-X--X-XXX--X-X
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
            XXXXXXX---XXX-XXXXXXX
            X-----X-XXX---X-----X
            X-XXX-X--XX---X-XXX-X
            X-XXX-X--X-XX-X-XXX-X
            X-XXX-X-XX-XX-X-XXX-X
            X-----X----X--X-----X
            XXXXXXX-X-X-X-XXXXXXX
            ---------------------
            X-X-X-X---X-X---X--X-
            XX-X----X-XX-X-X---X-
            ---XX-XXX-XX-XXX-XXX-
            XX--XX-X-X-XXX-XX--X-
            --X--XXX-XXX-XXX----X
            --------X-X---X----X-
            XXXXXXX-----X---X---X
            X-----X---X---X--X-XX
            X-XXX-X-XXX-X-X-XXX-X
            X-XXX-X--X-X-X-X-XXX-
            X-XXX-X-XX-X-XXX--X-X
            X-----X----XXX-XXX---
            XXXXXXX-X--X-XXX--X-X
            """, MatrixToString(code.Data));
    }
}

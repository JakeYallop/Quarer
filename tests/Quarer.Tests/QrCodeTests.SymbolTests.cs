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
    public void Create_2H()
    {
        var data = "HELLO WORLD";
        var code = QrCode.Create(data, ErrorCorrectionLevel.H);
        Assert.Equal(ErrorCorrectionLevel.H, code.ErrorCorrectionLevel);
        Assert.Equal(2, code.Version.Version);
        Assert.Equal("""
            X X X X X X X - X X X - X X - X X - X X X X X X X
            X - - - - - X - - - X - X X - X X - X - - - - - X
            X - X X X - X - X - X - - - - - X - X - X X X - X
            X - X X X - X - - X X - - X X X X - X - X X X - X
            X - X X X - X - X X - - X - X X - - X - X X X - X
            X - - - - - X - - X - - - - - X - - X - - - - - X
            X X X X X X X - X - X - X - X - X - X X X X X X X
            - - - - - - - - X - X - X X - - X - - - - - - - -
            - - - - - X X - - - X X X X - X - - X - X - X - X
            X X - X X - - X X X X - - - X - X - X X - X - - X
            - - - - X - X - - - - X - - - X - - X - X - - - -
            X - X X X - - X - X - X - - - X - X X X X - - X -
            X - X - X X X - - X X X - - - - - X X X X - X - X
            X X X - X X - - X - - - - X - - - X X - - X - X -
            X - - - X - X X - - - - X - X X X X - - - - X - -
            X - - X X - - - - X - X X - X - X - - X X - X - X
            X - X - X - X - - X - - - X - X X X X X X X X - X
            - - - - - - - - X X - - X - X - X - - - X X X X -
            X X X X X X X - - - X X - X X X X - X - X - X X -
            X - - - - - X - X - - X - X X X X - - - X X X X X
            X - X X X - X - - - - - - X X - X X X X X X - X X
            X - X X X - X - - - X X X - - - - X - X - X X - X
            X - X X X - X - - X - X - - X - X - X - - X - - X
            X - - - - - X - - - - - X X X X - - X - - X X - -
            X X X X X X X - - - X X X - - X X - X - X - X X X
            """, MatrixToString(code.Data));
    }

    [Fact]
    public void Create2_5H()
    {
        var data = "HELLO WORLD WITH A LONGER TEST STRING TEST STRING TEST ST";
        var code = QrCode.Create(data, ErrorCorrectionLevel.H);
        Assert.Equal(ErrorCorrectionLevel.H, code.ErrorCorrectionLevel);
        Assert.Equal(5, code.Version.Version);
        Assert.Equal("""
            X X X X X X X - - X X X X X - X - - X - - - - X - - X X - - X X X X X X X
            X - - - - - X - X - X X X - X X - - X X X - X - X X X X X - X - - - - - X
            X - X X X - X - - X X X X - - X X - - X X X X - - X X - X - X - X X X - X
            X - X X X - X - - X - X X - - - X X - - - - X - X - X X X - X - X X X - X
            X - X X X - X - - X - X - X - - - - X - X - - - - - X - X - X - X X X - X
            X - - - - - X - X - - X - X X X - - - X X X X - - - - - - - X - - - - - X
            X X X X X X X - X - X - X - X - X - X - X - X - X - X - X - X X X X X X X
            - - - - - - - - X X X - - - X X X X X - - X - - X X - X - - - - - - - - -
            - - - - X X X X - - X - - X X - X X - X - X - X X X - - - - X X - - - X -
            X - X X X X - X - - X X - X - - X - X - X - X X X - X - - X - - X X X X X
            - - X X X - X - - X X X X - - X X X X X X - - - - - X X X - - - X - - - -
            - - X - X X - X - X X X - - X - X - - - X - - - - X X X X - X - - - X X -
            - X X - X X X X X X - X X X X - X X X X X - X X - - X - - - X - - - - - -
            - X X X - - - - X X X - - X - X - - - - - - X - X X - - - - - X - - - - -
            - - X X X X X - X X - X X X - - - X X X X - X - X - X X - X - - X X X - -
            X X - - - X - X X - - - - X - - - X X X - X - - - X - X - - X - - X - - -
            - X X X X - X X X X - - - X X X X X X X X X X X - - - X - X - - X X X X X
            X X X - X X - X X X X X - X - X X - X - - - X - X - - X X X X X X X - X -
            X X - X - X X - X - - X X - X - X X X X - - - X X X X - X - X X X - X X X
            X X X X X X - - - X X X - - - - - - - - - - X X X - - X - - - - - - - X -
            - X - X - X X - X X X - - X X X - X X X - X X X - X X - X - X - X - X X -
            - X X - - X - - X - X - X - X - - - X X - - X X - X - - X - - X X - - - -
            X X X X X - X - - X - - X - - - - X - X - X - - X - - - X X - X - - - X X
            X X X - X - - X - X - - X - X X X X - X X X - X X - X X - - X X X - X X -
            X X X X X X X X - - X X X - - X - X X - - - - X - X X - - - - X X - - - X
            X - - X - - - - - X - X X - - - X X X X X - - - X X - X - - X - X - X X -
            - - - X X X X - - - X - X X X X X X X - - - - X X X X - - - X X X X - X -
            - - X X X X - - - - - - X X X - X - - - X - X X - X - - X - - X X - X - X
            X X - X X - X - - - - - - - - X X X X X X - - - X X - - X X X X X - - X X
            - - - - - - - - X X X X X - - X X X - - X - X X X X X X X - - - X X - X X
            X X X X X X X - X - X X X - X - X X - X X - - - X - X - X - X - X - - X X
            X - - - - - X - X - X - X - X X X X X X X - X X - - - X X - - - X X - X X
            X - X X X - X - X X - X - X - - - X X - - - X - X - X - X X X X X - - - -
            X - X X X - X - - - X - X - - X X X - X X - X - - X X X - X - X X X X X X
            X - X X X - X - - X X - X X - - - X X - X X - - - - X X - X - - X X X X -
            X - - - - - X - - X X - - - X - - X - - - - X - X X - - - - - - X - - X X
            X X X X X X X - - X - X - - - - X - - - X X X X - X - X X - - - X X - - X
            """, MatrixToString(code.Data));
    }

    [Fact]
    public void Create3_7H()
    {
        var data = "HELLO WORLD WITH A LONGER TEST STRING TEST STRING TEST STRING STRING STRING STRING STRING";
        var code = QrCode.Create(data, ErrorCorrectionLevel.H);
        Assert.Equal(ErrorCorrectionLevel.H, code.ErrorCorrectionLevel);
        Assert.Equal(7, code.Version.Version);
        Assert.Equal("""
            X X X X X X X - X X - X - X X X - - - - - - - - X - - X - X X X - X - - X - X X X X X X X
            X - - - - - X - X - X - X X - - - - - - - X - X X X - X - - - X - - - X - - X - - - - - X
            X - X X X - X - X - - - X X X X X X X X X - - X X X X X - X X - - X - X - - X - X X X - X
            X - X X X - X - - X X X X X - X - - X X X X X - - - X - - - - - - - - X X - X - X X X - X
            X - X X X - X - - X - X X X X X - X - - X X X X X - - - X - - - - X X X X - X - X X X - X
            X - - - - - X - X - - - X X - - - - - - X - - - X X - X - - X - - X - - - - X - - - - - X
            X X X X X X X - X - X - X - X - X - X - X - X - X - X - X - X - X - X - X - X X X X X X X
            - - - - - - - - X - - - X - - - - - X X X - - - X X - - X - X - X - - - X - - - - - - - -
            - - X X X - X - X - - X - X - X - X X - X X X X X - X X X - - - X X - X X X X X - - X X X
            - X - - X - - X X - X X - - X X X - X X X - - X - - X X - X X X - - - - X X X X - X - X X
            X - X X - X X - X X - X X X X X - X X - - X X X X X - X - X - - X X - X X - - X X - - - -
            X - - X X X - X X - - - - X - - - - - - - - X - X - X - - - X - - - - X - X - - - - - X -
            X - X X - X X - - - X X - - X X X - X X X - - X - - - X - X - X - X - - - X X - X X X X -
            - - X - - - - X X X - - - X - X - X X X - - X X X - - X - X X X X X - - - - - - X - X - X
            - - - X X - X X - X X X X X - X X - - - - X - - - - X - - - X X X X X X X - X X X - X - -
            - - X - X - - X - - X - - X - X - X X - - - X X X - X X X X - - - X - - X X - X - - - X -
            X - - X X - X X X - - - X X X - - - X X X - - X - - X - - - - X - X - X - - - - X X - X X
            X X - X - - - - X - - - - X X - - - - - - - - - X X X - X X - X X X X X - X X - - - X X -
            - - - - - - X X - X X X X X X - - - - X - - - - X X X - - - X - - X X - X - X X X X - X X
            - - - X X X - X - X X - - - X - - X - X - - - X X - - X X - X X - X X X X X X - X - X - -
            - X - - X X X X X - - - - - X X - - - - X X X X X X - X - X X - X X - X X X X X X X X X X
            X - X - X - - - X - - - - - X X X X - - X - - - X - X - - - X - X X - - X - - - X - - X X
            X - - - X - X - X X - - X X - X - - X X X - X - X - - - X X X - X - X X X - X - X X X - X
            - X - - X - - - X X - - - X - X - - - X X - - - X - - X - - - - - - X X X - - - X - X - -
            - X X - X X X X X - X X - - X - - X X - X X X X X - - - X X X - - - - - X X X X X X - X X
            - X X X X X - X - X - X X X - - X - X X X X X X - X - - X X X - X X - - - - X - X X X X -
            X - - - - X X - X - - - - - - X X - X X - - - - - X X - - - X - X - - - X - - X X - X X -
            X - - X X - - - X - X - X X - - - X - - X - X - X X - X X - - - X - X X - X - - - X X X -
            X - X X X X X - X - - X X X - X X - - X - X - X - - - - X - - - - - - X X - - - - - - - -
            X - - X X - - - - - - - X X X X X X - X - - X X - X X - - - - - - X X - - - X X - X X - -
            X - X X X X X - X X - X - - - X X X X - X X - X - X - X - - X X X - X X X - X - - - X - X
            - X X X - X - X - X - - X - - X - - - X X - X X - X - - X X X - - - - - - - - - X - X X X
            - - - - - X X - X - X X X - - - - X X X - - - X X X X X X - X X - - X X X - - X X - - - -
            X - - - X X - - - X - X - - X X X - - - X X - X X X - X - - - - X X - - - X - - - X X X X
            - - - - X - X X X - - - X - - X X - - - - X - X X X X - X X - - X - X - - - X X X X - - X
            - X X X X - - - - X X X - - X X - X X - X - - X - - X X - X X X - - X - - X X - - - - - X
            X - - X X - X X X - - X X - X X X - X - X X X X X X - - X X - X - - X X X X X X X - X - -
            - - - - - - - - X - X - - - X X - X - X X - - - X X X - X X X - - - - - X - - - X - X - X
            X X X X X X X - - X - X - X - X - X - - X - X - X - X - X X - - X X - - X - X - X X - X X
            X - - - - - X - - - - X - X - X - X - X X - - - X X - X X X X - - - - X X - - - X X - X -
            X - X X X - X - X - - X - X - X - - X X X X X X X X X - X X - X X - X - X X X X X X X X -
            X - X X X - X - X X X - - - - - - X - X X - X X - X X X - X X - X X X - - X X X X X - - X
            X - X X X - X - X - X - - X X X X - X X - - - - - - - - X - X X - - - - - X - X - X X - -
            X - - - - - X - - X - - X - - - - X X X X - X - X X - - - X - - - X X X - X X X - - X - X
            X X X X X X X - - X X - X - X - X X X - X X - X X X X - - - X - - X X - X - - X X - X - -
            """, MatrixToString(code.Data));
    }

    [Fact]
    public void Create_4M_WithMaximumLengthLatin1DataForVersion_FitsIntoQrCode()
    {
        // This string completely fills the QR Code capacity - it cannot fit ECI information as well
        var code = QrCode.Create("Hello world with a longer string using Latin1 Encoding û þ ç Ã");
        Assert.Equal(ErrorCorrectionLevel.M, code.ErrorCorrectionLevel);
        Assert.Equal(4, code.Version.Version);
        Assert.Equal("""
            X X X X X X X - - X X X - - X X - X X X - - X X - - X X X X X X X
            X - - - - - X - - - - - X - - X X - X - X X X X X - X - - - - - X
            X - X X X - X - - X X X X X - X X X X X X - - - - - X - X X X - X
            X - X X X - X - - X X X - X X X - - X - - - - X X - X - X X X - X
            X - X X X - X - - X - X X - X X - X X - - X - X X - X - X X X - X
            X - - - - - X - X X X X - X X X X X X X - X - - - - X - - - - - X
            X X X X X X X - X - X - X - X - X - X - X - X - X - X X X X X X X
            - - - - - - - - - X - X X - X - - X - - - X - - X - - - - - - - -
            X - - X - X X - X - - X X X X - - - - X - X - - - X - X - - - - -
            - X - X - X - - - X - - X X - - X X - - X X X - X X X - - - - - X
            - - X X X X X X X X - - - - X - - X - X X - - - X X X X X X X - X
            X - X X X - - - - - - - - X - X X - X X - X - - - X X - - X - X X
            X - - X X X X - X - X X X - X - X X X X X - - - X X X - - - - - X
            X X - X - - - X X X - - - X - - - X - X X X X - X - - X X - - - X
            X X - - X - X X X X - X - - - X X - - - - - - X - X X X - - X X -
            X - X X - - - X X X X X - - - X - X X X X X - X - - X - - - - X -
            X X X - - X X X X - X X X X X - X X - X X X X X X - X X X X - X -
            X X X X - - - X - - X - - - - - X - - - - X - X - - - - - X X X -
            - X X - X - X - X - X - - - - - - - X - X - - - X X - - X - X - X
            X X X - - - - X - X - - X X X - X - - - - X - X X - - X X - - X X
            X X - - - X X - - - X - X X - - X - - - - X - X X X - - - X - X -
            - X - - - X - X - X X X X X X - - X X - X - - - X X X - - X - X -
            X X X X X X X X X - - X - - X - - - - X - - - - X X X - X X - - X
            - X - - - - - X X X X X - X - - X - X - - X - - X X - - X X - X -
            X - - - - - X X - - - X - - X X - X - X X X X - X X X X X - - X -
            - - - - - - - - X - X - - - - - - - X X - - X X X - - - X - X - X
            X X X X X X X - - - - X X X - - X - X - X X - - X - X - X X X X -
            X - - - - - X - X X - X X - X X - X X X - X - - X - - - X - - X X
            X - X X X - X - - X - X X - X - X X X X X X X - X X X X X - - - -
            X - X X X - X - X - X X X X - - X - - - - X - - - - X X X - X - -
            X - X X X - X - - - X X X X - X X X X - - - X X - - - - X X X - X
            X - - - - - X - - - X X X - - X X X - X - X X - X - - - - - - - -
            X X X X X X X - X - - - - - - - X - X - - X - - X X - X - X - X -
            """, MatrixToString(code.Data));
    }

    [Fact]
    public void TryCreate_WithMaxLengthDataThatWouldNotFitEciCode_ReturnsDataTooLarge()
    {
        // This string completely fills the QR Code capacity - it cannot fit ECI information as well
        // Γ = 2 bytes when encoded as UTF-8
        var result = QrCode.TryCreate(new string('a', 2951) + "Γ");
        Assert.False(result.Success);
        Assert.Equal(QrCreationResult.DataTooLargeSimple, result.Reason);
    }

    [Fact]
    public void TryCreate_40L_WithMaxLengthDataThatWouldNotFitEciCode_ReturnsDataTooLarge()
    {
        // This string completely fills the QR Code capacity - it cannot fit ECI information as well
        // Γ = 2 bytes when encoded as UTF-8
        var result = QrCode.TryCreate(new string('a', 2951) + "Γ", QrVersion.GetVersion(40), ErrorCorrectionLevel.L);
        Assert.False(result.Success);
        Assert.Equal(QrCreationResult.DataTooLargeSimple, result.Reason);
    }
}

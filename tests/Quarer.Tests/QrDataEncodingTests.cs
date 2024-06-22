namespace Quarer.Tests;
public sealed class QrDataEncodingTests
{
    [Theory]
    [InlineData(QrAnalysisResult.DataTooLarge)]
    public void Invalid_ReturnsDefaults_WithExpectedResult(QrAnalysisResult result)
    {
        var encoding = QrDataEncoding.Invalid(result);

        Assert.Equal(default, encoding.Version);
        Assert.Equal(default, encoding.DataSegments);
        Assert.Equal(result, encoding.Result);
    }

    [Fact]
    public void Invalid_ThrowsForSuccessResult()
        => Assert.Throws<ArgumentException>(() => QrDataEncoding.Invalid(QrAnalysisResult.Success));
}

namespace Quarer.Tests;
public sealed class QrAnalysisResultTests
{
    [Theory]
    [InlineData(AnalysisResult.DataTooLarge)]
    public void Invalid(AnalysisResult result)
    {
        var encoding = QrAnalysisResult.Invalid(result);
        Assert.False(encoding.Success);
        Assert.Equal(result, encoding.AnalysisResult);
    }

    [Fact]
    public void Invalid_ThrowsForSuccessResult()
        => Assert.Throws<ArgumentException>(() => QrAnalysisResult.Invalid(AnalysisResult.Success));
}

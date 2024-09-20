namespace Quarer.Tests;
public sealed class QrAnalysisResultTests
{
    [Theory]
    [InlineData(AnalysisResult.DataTooLarge)]
    public void Invalid(AnalysisResult result)
    {
        var encoding = DataAnalysisResult.Invalid(result);
        Assert.False(encoding.Success);
        Assert.Equal(result, encoding.Reason);
    }

    [Fact]
    public void Invalid_ThrowsForSuccessResult()
        => Assert.Throws<ArgumentException>(() => DataAnalysisResult.Invalid(AnalysisResult.Success));
}

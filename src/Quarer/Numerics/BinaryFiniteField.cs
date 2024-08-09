using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Quarer.Numerics;

/// <summary>
/// Finite field (a.k.a Galois Field) optimised for QR encoding. Characteristic 2,  order 256,  primitive 0x011D (285).
/// </summary>
internal static class BinaryFiniteField
{
    internal static ReadOnlySpan<byte> ExpTable =>
    [
        1, 2, 4, 8, 16, 32, 64, 128, 29, 58, 116, 232, 205, 135, 19, 38,
        76, 152, 45, 90, 180, 117, 234, 201, 143, 3, 6, 12, 24, 48, 96, 192,
        157, 39, 78, 156, 37, 74, 148, 53, 106, 212, 181, 119, 238, 193, 159, 35,
        70, 140, 5, 10, 20, 40, 80, 160, 93, 186, 105, 210, 185, 111, 222, 161,
        95, 190, 97, 194, 153, 47, 94, 188, 101, 202, 137, 15, 30, 60, 120, 240,
        253, 231, 211, 187, 107, 214, 177, 127, 254, 225, 223, 163, 91, 182, 113, 226,
        217, 175, 67, 134, 17, 34, 68, 136, 13, 26, 52, 104, 208, 189, 103, 206,
        129, 31, 62, 124, 248, 237, 199, 147, 59, 118, 236, 197, 151, 51, 102, 204,
        133, 23, 46, 92, 184, 109, 218, 169, 79, 158, 33, 66, 132, 21, 42, 84,
        168, 77, 154, 41, 82, 164, 85, 170, 73, 146, 57, 114, 228, 213, 183, 115,
        230, 209, 191, 99, 198, 145, 63, 126, 252, 229, 215, 179, 123, 246, 241, 255,
        227, 219, 171, 75, 150, 49, 98, 196, 149, 55, 110, 220, 165, 87, 174, 65,
        130, 25, 50, 100, 200, 141, 7, 14, 28, 56, 112, 224, 221, 167, 83, 166,
        81, 162, 89, 178, 121, 242, 249, 239, 195, 155, 43, 86, 172, 69, 138, 9,
        18, 36, 72, 144, 61, 122, 244, 245, 247, 243, 251, 235, 203, 139, 11, 22,
        44, 88, 176, 125, 250, 233, 207, 131, 27, 54, 108, 216, 173, 71, 142, 1,
        2, 4, 8, 16, 32, 64, 128, 29, 58, 116, 232, 205, 135, 19, 38, 76,
        152, 45, 90, 180, 117, 234, 201, 143, 3, 6, 12, 24, 48, 96, 192, 157,
        39, 78, 156, 37, 74, 148, 53, 106, 212, 181, 119, 238, 193, 159, 35, 70,
        140, 5, 10, 20, 40, 80, 160, 93, 186, 105, 210, 185, 111, 222, 161, 95,
        190, 97, 194, 153, 47, 94, 188, 101, 202, 137, 15, 30, 60, 120, 240, 253,
        231, 211, 187, 107, 214, 177, 127, 254, 225, 223, 163, 91, 182, 113, 226, 217,
        175, 67, 134, 17, 34, 68, 136, 13, 26, 52, 104, 208, 189, 103, 206, 129,
        31, 62, 124, 248, 237, 199, 147, 59, 118, 236, 197, 151, 51, 102, 204, 133,
        23, 46, 92, 184, 109, 218, 169, 79, 158, 33, 66, 132, 21, 42, 84, 168,
        77, 154, 41, 82, 164, 85, 170, 73, 146, 57, 114, 228, 213, 183, 115, 230,
        209, 191, 99, 198, 145, 63, 126, 252, 229, 215, 179, 123, 246, 241, 255, 227,
        219, 171, 75, 150, 49, 98, 196, 149, 55, 110, 220, 165, 87, 174, 65, 130,
        25, 50, 100, 200, 141, 7, 14, 28, 56, 112, 224, 221, 167, 83, 166, 81,
        162, 89, 178, 121, 242, 249, 239, 195, 155, 43, 86, 172, 69, 138, 9, 18,
        36, 72, 144, 61, 122, 244, 245, 247, 243, 251, 235, 203, 139, 11, 22, 44,
        88, 176, 125, 250, 233, 207, 131, 27, 54, 108, 216, 173, 71, 142, 1, 2
    ];

    internal static ReadOnlySpan<byte> LogTable =>
    [
        0, 0, 1, 25, 2, 50, 26, 198, 3, 223, 51, 238, 27, 104, 199, 75,
        4, 100, 224, 14, 52, 141, 239, 129, 28, 193, 105, 248, 200, 8, 76, 113,
        5, 138, 101, 47, 225, 36, 15, 33, 53, 147, 142, 218, 240, 18, 130, 69,
        29, 181, 194, 125, 106, 39, 249, 185, 201, 154, 9, 120, 77, 228, 114, 166,
        6, 191, 139, 98, 102, 221, 48, 253, 226, 152, 37, 179, 16, 145, 34, 136,
        54, 208, 148, 206, 143, 150, 219, 189, 241, 210, 19, 92, 131, 56, 70, 64,
        30, 66, 182, 163, 195, 72, 126, 110, 107, 58, 40, 84, 250, 133, 186, 61,
        202, 94, 155, 159, 10, 21, 121, 43, 78, 212, 229, 172, 115, 243, 167, 87,
        7, 112, 192, 247, 140, 128, 99, 13, 103, 74, 222, 237, 49, 197, 254, 24,
        227, 165, 153, 119, 38, 184, 180, 124, 17, 68, 146, 217, 35, 32, 137, 46,
        55, 63, 209, 91, 149, 188, 207, 205, 144, 135, 151, 178, 220, 252, 190, 97,
        242, 86, 211, 171, 20, 42, 93, 158, 132, 60, 57, 83, 71, 109, 65, 162,
        31, 45, 67, 216, 183, 123, 164, 118, 196, 23, 73, 236, 127, 12, 111, 246,
        108, 161, 59, 82, 41, 157, 85, 170, 251, 96, 134, 177, 187, 204, 62, 90,
        203, 89, 95, 176, 156, 169, 160, 81, 11, 245, 22, 235, 122, 117, 44, 215,
        79, 174, 213, 233, 230, 231, 173, 232, 116, 214, 244, 234, 168, 80, 88, 175
    ];

    internal static ReadOnlySpan<byte> G7 => [1, 127, 122, 154, 164, 11, 68, 117];
    internal static ReadOnlySpan<byte> G10 => [1, 216, 194, 159, 111, 199, 94, 95, 113, 157, 193];
    internal static ReadOnlySpan<byte> G13 => [1, 137, 73, 227, 17, 177, 17, 52, 13, 46, 43, 83, 132, 120];
    internal static ReadOnlySpan<byte> G15 => [1, 29, 196, 111, 163, 112, 74, 10, 105, 105, 139, 132, 151, 32, 134, 26];
    internal static ReadOnlySpan<byte> G16 => [1, 59, 13, 104, 189, 68, 209, 30, 8, 163, 65, 41, 229, 98, 50, 36, 59];
    internal static ReadOnlySpan<byte> G17 => [1, 119, 66, 83, 120, 119, 22, 197, 83, 249, 41, 143, 134, 85, 53, 125, 99, 79];
    internal static ReadOnlySpan<byte> G18 => [1, 239, 251, 183, 113, 149, 175, 199, 215, 240, 220, 73, 82, 173, 75, 32, 67, 217, 146];
    internal static ReadOnlySpan<byte> G20 => [1, 152, 185, 240, 5, 111, 99, 6, 220, 112, 150, 69, 36, 187, 22, 228, 198, 121, 121, 165, 174];
    internal static ReadOnlySpan<byte> G22 => [1, 89, 179, 131, 176, 182, 244, 19, 189, 69, 40, 28, 137, 29, 123, 67, 253, 86, 218, 230, 26, 145, 245];
    internal static ReadOnlySpan<byte> G24 => [1, 122, 118, 169, 70, 178, 237, 216, 102, 115, 150, 229, 73, 130, 72, 61, 43, 206, 1, 237, 247, 127, 217, 144, 117];
    internal static ReadOnlySpan<byte> G26 => [1, 246, 51, 183, 4, 136, 98, 199, 152, 77, 56, 206, 24, 145, 40, 209, 117, 233, 42, 135, 68, 70, 144, 146, 77, 43, 94];
    internal static ReadOnlySpan<byte> G28 => [1, 252, 9, 28, 13, 18, 251, 208, 150, 103, 174, 100, 41, 167, 12, 247, 56, 117, 119, 233, 127, 181, 100, 121, 147, 176, 74, 58, 197];
    internal static ReadOnlySpan<byte> G30 => [1, 212, 246, 77, 73, 195, 192, 75, 98, 5, 70, 103, 177, 22, 217, 138, 51, 181, 246, 72, 25, 18, 46, 228, 74, 216, 195, 11, 106, 130, 150];

    private static ReadOnlySpan<byte> GetGeneratorPolynomial(int numberOfErrorCorrectionCodewords)
    {
        return numberOfErrorCorrectionCodewords switch
        {
            7 => G7,
            10 => G10,
            13 => G13,
            15 => G15,
            16 => G16,
            17 => G17,
            18 => G18,
            20 => G20,
            22 => G22,
            24 => G24,
            26 => G26,
            28 => G28,
            30 => G30,
            _ => throw new ArgumentOutOfRangeException(nameof(numberOfErrorCorrectionCodewords)),
        };
    }

    public static byte Add(byte a, byte b) => (byte)(a ^ b);
    public static byte Subtract(byte a, byte b) => Add(a, b);
    public static byte Multiply(byte a, byte b) => a is 0 || b is 0 ? (byte)0 : ExpTable[LogTable[a] + LogTable[b]];
    public static byte Divide(byte a, byte b)
    {
        return b is 0
        ? ThrowDivideByZeroException()
        : a is 0
            ? (byte)0
            : ExpTable[LogTable[a] + 255 - LogTable[b]];
    }
    public static byte Pow(byte a, byte power) => ExpTable[LogTable[a] * power % 255];

    //returns a byte so that we can use it in the `Divide` method
    [DoesNotReturn]
    private static byte ThrowDivideByZeroException() => throw new DivideByZeroException();

    /// <summary>
    /// Adds two polynomials <paramref name="p"/> and <paramref name="q"/> and stores the result in <paramref name="destination"/>.
    /// <para>
    /// Expects <paramref name="p"/> and <paramref name="q"/> to be represented in power order with the largest power first.
    /// </para>
    /// </summary>
    /// <param name="p"></param>
    /// <param name="q"></param>
    /// <param name="destination"></param>
    /// <returns>The size of the data stored in <paramref name="destination"/>.</returns>
    internal static int Add(ReadOnlySpan<byte> p, ReadOnlySpan<byte> q, Span<byte> destination)
    {
        ReadOnlySpan<byte> smaller;
        ReadOnlySpan<byte> larger;

        if (p.Length < q.Length)
        {
            smaller = p;
            larger = q;
        }
        else
        {
            smaller = q;
            larger = p;
        }

        Debug.Assert(larger.Length <= destination.Length, "Not enough space in destination");

        ArgumentOutOfRangeException.ThrowIfLessThan(destination.Length, larger.Length);
        larger.CopyTo(destination);

        var diff = int.Abs(p.Length - q.Length);
        for (var i = 0; i < smaller.Length; i++)
        {
            destination[i + diff] = Add(destination[i + diff], smaller[i]);
        }

        return larger.Length;
    }

    /// <summary>
    /// Multiplies two polynomials <paramref name="p"/> and <paramref name="q"/> and stores the result in <paramref name="destination"/>.
    /// <para>
    /// Expects <paramref name="p"/> and <paramref name="q"/> to be represented in power order with the largest power first.
    /// </para>
    /// </summary>
    /// <param name="p"></param>
    /// <param name="q"></param>
    /// <param name="destination"></param>
    /// <returns>The size of the data stored in <paramref name="destination"/>.</returns>
    internal static int Multiply(ReadOnlySpan<byte> p, ReadOnlySpan<byte> q, Span<byte> destination)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(destination.Length, p.Length + q.Length - 1);

        for (var i = 0; i < p.Length; i++)
        {
            for (var j = 0; j < q.Length; j++)
            {
                var offset = i + j;
                destination[offset] = Add(destination[offset], Multiply(p[i], q[j]));
            }
        }

        return p.Length + q.Length - 1;
    }

    /// <summary>
    /// Returns the value of the polynomial <paramref name="polynomial"/> at <paramref name="x"/>, under the finite field.#
    /// <para>
    /// Expects <paramref name="polynomial"/> to be represented in power order with the largest power first.
    /// </para>
    /// </summary>
    /// <param name="polynomial"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    internal static byte Evaluate(ReadOnlySpan<byte> polynomial, byte x)
    {
        var y = polynomial[^1];
        for (var i = polynomial.Length - 2; i >= 0; i--)
        {
            y = Add(Multiply(y, x), polynomial[i]);
        }

        return y;
    }

    /// <summary>
    /// Implements Extended Synthetic Division to divide the <paramref name="dividend"/> polynomial by the <paramref name="divisor"/> polynomial
    /// and stores the result and the remainder in <paramref name="destination"/>.
    /// </summary>
    /// <param name="dividend"></param>
    /// <param name="divisor"></param>
    /// <param name="destination"></param>
    /// <returns>
    /// The size of the data stored in <paramref name="destination"/>, and the size of the quotient.
    /// The size of the quotient is effectively zero-indexed separator splitting the quotient from the remainder.
    /// </returns>
    internal static (byte Size, byte QuotientSize) Divide(ReadOnlySpan<byte> dividend, ReadOnlySpan<byte> divisor, Span<byte> destination)
    {
        if (destination.Length < dividend.Length + divisor.Length)
        {
            throw new ArgumentException("Destination too small.");
        }

        dividend.CopyTo(destination);
        for (var i = 0; i < dividend.Length; i++)
        {
            var coefficient = destination[i];
            if (coefficient is 0)
            {
                continue;
            }

            for (var j = 1; j < divisor.Length; j++)
            {
                Debug.Assert(divisor[j] is not 0, $"divisor is never expected to be 0.");
                var offset = i + j;
                destination[i + j] = Add(destination[offset], Multiply(coefficient, divisor[j]));
            }
        }

        return ((byte)(divisor.Length + dividend.Length - 1), (byte)dividend.Length);
    }
}

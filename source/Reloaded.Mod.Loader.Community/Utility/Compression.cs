namespace Reloaded.Mod.Loader.Community.Utility;

/// <summary>
/// Compresses arbitrary files using the brotli compression algorithm.
/// </summary>
public static class Compression
{
    /// <summary>
    /// Compresses a given stream of bytes.
    /// </summary>
    /// <param name="input">The data to compress.</param>
    public static byte[] Compress(byte[] input)
    {
        using var inputStream = new MemoryStream(input);
        return Compress(inputStream);
    }

    /// <summary>
    /// Compresses a given stream of bytes.
    /// </summary>
    /// <param name="input">The data to compress.</param>
    public static byte[] Compress(Stream input)
    {
        using var output = new MemoryStream();
        var compressor   = new BrotliStream(output, CompressionLevel.Optimal);
        input.CopyTo(compressor);
        compressor.Dispose();

        return output.ToArray();
    }

    /// <summary>
    /// Decompresses a given stream of bytes.
    /// </summary>
    /// <param name="input">The data to decompress.</param>
    public static Memory<byte> Decompress(byte[] input)
    {
        using var inputStream = new MemoryStream(input);
        return Decompress(inputStream);
    }

    /// <summary>
    /// Decompresses a given stream of bytes.
    /// </summary>
    /// <param name="input">The data to decompress.</param>
    public static Memory<byte> Decompress(Stream input)
    {
        var decompressor = new BrotliStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        decompressor.CopyTo(output);
        decompressor.Dispose();

        return output.GetBuffer().AsMemory(0, (int) output.Length);
    }
}
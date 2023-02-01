namespace Reloaded.Mod.Loader.Update.Utilities;

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
        return DecompressToMemory(inputStream);
    }    
    
    /// <summary>
    /// Decompresses a given stream of bytes.
    /// </summary>
    /// <param name="input">The data to decompress.</param>
    public static byte[] DecompressToArray(byte[] input)
    {
        using var inputStream = new MemoryStream(input);
        return DecompressToArray(inputStream);
    }

    /// <summary>
    /// Decompresses a given stream of bytes.
    /// </summary>
    /// <param name="input">The data to decompress.</param>
    public static Memory<byte> DecompressToMemory(Stream input)
    {
        using var result = DecompressToStream(input);
        return result.GetBuffer().AsMemory(0, (int)result.Length);
    }

    /// <summary>
    /// Decompresses a given stream of bytes.
    /// </summary>
    /// <param name="input">The data to decompress.</param>
    public static byte[] DecompressToArray(Stream input)
    {
        using var result = DecompressToStream(input);
        return result.ToArray();
    }

    /// <summary>
    /// Decompresses a given stream of bytes.
    /// </summary>
    /// <param name="input">The data to decompress.</param>
    /// <returns>Stream with the decompressed data. Pointing to said data.</returns>
    public static MemoryStream DecompressToStream(Stream input)
    {
        var decompressor = new BrotliStream(input, CompressionMode.Decompress);
        var output = new MemoryStream();
        decompressor.CopyTo(output);
        decompressor.Dispose();
        output.Position = 0;
        return output;
    }
}
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace KungFuArchiveEditor.Tools;
/// <summary>
/// 存档读写工具
/// </summary>
public static class ArchiveTool
{
    /// <summary>
    /// 存档文件格式分类
    /// </summary>
    internal enum ArchiveFileType
    {
        Json,
        Data,
        Unknown
    }
    /// <summary>
    /// 根据文件名判断存档文件格式
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private static ArchiveFileType GetArchiveFileType(string name)
    {
        if (name.EndsWith(".json"))
        {
            return ArchiveFileType.Json;
        }
        else if (name.EndsWith(".data"))
        {
            return ArchiveFileType.Data;
        }
        return ArchiveFileType.Unknown;
    }
    public static async Task<JObject> LoadArchiveAsync(FileInfo file)
    {
        var fileType = GetArchiveFileType(file.Name);
        using var stream = file.OpenRead();
        return await LoadArchiveAsync(stream, fileType);
    }

    public static async Task<JObject> LoadArchiveAsync(IStorageFile file)
    {
        var fileType = GetArchiveFileType(file.Name);
        using var stream = await file.OpenReadAsync();
        return await LoadArchiveAsync(stream, fileType);
    }

    private static async Task<JObject> LoadArchiveAsync(Stream stream, ArchiveFileType fileType)
    {
        if (fileType == ArchiveFileType.Unknown)
        {
            throw new Exception("unknown archive file type");
        }
        string fileContent;
        if (fileType == ArchiveFileType.Json)
        {
            var encoding = Encoding.GetEncoding("utf-16");
            using var streamReader = new StreamReader(stream, encoding);
            fileContent = await streamReader.ReadToEndAsync();
        }
        else
        {
            fileContent = await ReadArchiveDataAsync(stream);
        }
        return JObject.Parse(fileContent);
    }

    /// <summary>
    /// 解析data文件中的字符串
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    private static async Task<string> ReadArchiveDataAsync(Stream stream)
    {
        using var reader = new BinaryReader(stream);
        using var outStream = new MemoryStream();
        while (stream.Position < stream.Length)
        {
            await LoadArchiveDataNodeAsync(reader, outStream);
        }
        var outData = outStream.ToArray();
        var packTotalSize = BitConverter.ToInt32(outData, 0);
        var strBytesLength = BitConverter.ToInt32(outData, 4);
        Encoding encoding;
        if (strBytesLength < 0)
        {
            strBytesLength = (~strBytesLength) << 1;
            encoding = Encoding.GetEncoding("utf-16");
        }
        else
        {
            strBytesLength -= 1;
            encoding = Encoding.UTF8;
        }
        return encoding.GetString(outData, 8, strBytesLength);
    }
    private static async Task LoadArchiveDataNodeAsync(BinaryReader reader, Stream outStream)
    {
        var magic = reader.ReadUInt32();
        if (magic != 0x9E2A83C1)
        {
            throw new Exception("invalid data file type");
        }
        reader.ReadBytes(12);
        //
        var compressedSize = reader.ReadInt32();
        reader.ReadBytes(12 + 16);
        var compressedData = reader.ReadBytes(compressedSize);
        using var stream = new MemoryStream(compressedData);
        using var decompressor = new GZipStream(stream, CompressionMode.Decompress);
        await decompressor.CopyToAsync(outStream);
        await decompressor.FlushAsync();
    }
    /// <summary>
    /// 保存存档
    /// </summary>
    /// <param name="file"></param>
    /// <param name="jsonData"></param>
    /// <returns></returns>
    public static async Task SaveArchiveAsync(FileInfo file, JObject jsonData)
    {
        var fileType = GetArchiveFileType(file.Name);
        using var stream = file.OpenWrite();
        await SaveArchiveAsync(stream, fileType, jsonData);
    }

    /// <summary>
    /// 保存存档
    /// </summary>
    /// <param name="file"></param>
    /// <param name="jsonData"></param>
    /// <returns></returns>
    public static async Task SaveArchiveAsync(IStorageFile file, JObject jsonData)
    {
        var fileType = GetArchiveFileType(file.Name);
        using var stream = await file.OpenWriteAsync();
        await SaveArchiveAsync(stream, fileType, jsonData);
    }

    private static async Task SaveArchiveAsync(Stream stream, ArchiveFileType fileType, JObject jsonData)
    {
        if (fileType == ArchiveFileType.Unknown)
        {
            throw new Exception("unknown archive file type");
        }
        var jsonContent = jsonData.ToString(Formatting.None);
        if (fileType == ArchiveFileType.Json)
        {
            //bom头
            stream.Write(new byte[] { 0xFF, 0xFE });
            var encoding = Encoding.GetEncoding("utf-16");
            var strData = encoding.GetBytes(jsonContent);
            await stream.WriteAsync(strData);
        }
        else
        {
            await SaveArchiveDataAsync(stream, jsonContent);
        }
    }

    private static async Task SaveArchiveDataAsync(Stream stream, string jsonContent)
    {
        var strLength = jsonContent.Length;
        var cap = 8 + 2 * (strLength + 1);
        using var dataStream = new MemoryStream(cap);
        using var binWriter = new BinaryWriter(dataStream);
        binWriter.Write(cap - 4);
        binWriter.Write(~strLength);
        var encoding = Encoding.GetEncoding("utf-16");
        var strData = encoding.GetBytes(jsonContent);
        binWriter.Write(strData);
        binWriter.Write(new byte[] { 0, 0 });
        var packData = dataStream.ToArray();
        var packIndex = 0;
        using var fileDataWriter = new BinaryWriter(stream);
        while (packIndex < packData.Length)
        {
            packIndex += await SaveArchiveDataNodeAsync(fileDataWriter, packData[packIndex..]);
        }
    }

    private static async Task<int> SaveArchiveDataNodeAsync(BinaryWriter writer, byte[] packData)
    {
        int procLimit = 0x020000;
        var procCount = Math.Min(packData.Length, procLimit);
        using var destStream = new MemoryStream();
        using (var compressor = new GZipStream(destStream, CompressionMode.Compress))
        {
            await compressor.WriteAsync(packData, 0, procCount);
            await compressor.FlushAsync();
        }
        var compressedData = destStream.ToArray();
        compressedData[9] = 0x0B;
        //
        uint magic = 0x9E2A83C1;
        int emptyValue = 0;
        writer.Write(magic);
        writer.Write(emptyValue);
        writer.Write(procLimit);
        writer.Write(emptyValue);
        //
        for (var i = 0; i < 2; i++)
        {
            writer.Write(compressedData.Length);
            writer.Write(emptyValue);
            writer.Write(procCount);
            writer.Write(emptyValue);
        }
        writer.Write(compressedData);
        return procCount;
    }
}

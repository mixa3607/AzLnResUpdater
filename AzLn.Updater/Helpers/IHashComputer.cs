using System.IO;

namespace AzLn.Updater
{
    public interface IHashComputer
    {
        string GetB64Md5(byte[] input);
        byte[] GetMd5(byte[] input);
        byte[] GetMd5(Stream stream);
        string GetMd5B64(Stream stream);
        string GetMd5B64(byte[] input);
        string GetMd5Hex(Stream stream);
        string GetMd5Hex(byte[] input);
    }
}
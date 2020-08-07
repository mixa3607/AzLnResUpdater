using System;
using System.IO;
using System.Security.Cryptography;
using AzLn.Contract.Extensions;

namespace AzLn.Updater
{
    public class HashComputer : IHashComputer
    {
        public string GetB64Md5(byte[] input)
        {
            return Convert.ToBase64String(GetMd5(input)).ToLower();
        }

        public byte[] GetMd5(byte[] input)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(input);

            return hash;
        }

        public byte[] GetMd5(Stream stream)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(stream);

            return hash;
        }

        public string GetMd5B64(Stream stream)
        {
            return Convert.ToBase64String(GetMd5(stream)).ToLower();
        }

        public string GetMd5B64(byte[] input)
        {
            return Convert.ToBase64String(GetMd5(input)).ToLower();
        }

        public string GetMd5Hex(Stream stream)
        {
            return GetMd5(stream).ToHexStr();
        }

        public string GetMd5Hex(byte[] input)
        {
            return GetMd5(input).ToHexStr();
        }
    }
}
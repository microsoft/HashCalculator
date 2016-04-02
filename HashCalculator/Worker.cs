using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace HashCalculator
{
    class Worker
    {
        public static string[] GetHashingAlgorithms(bool onlySHA = false)
        {
            if (onlySHA)
            {
                return new[] {
                    HashAlgorithmNames.Sha1,
                    HashAlgorithmNames.Sha256,
                    HashAlgorithmNames.Sha384,
                    HashAlgorithmNames.Sha512
                    };
            }

            return new[] {
                HashAlgorithmNames.Md5,
                HashAlgorithmNames.Sha1,
                HashAlgorithmNames.Sha256,
                HashAlgorithmNames.Sha384,
                HashAlgorithmNames.Sha512
                };
        }

        public static uint GetDigestSizeForAlgorithm(string algorithm)
        {
            return HashAlgorithmProvider.OpenAlgorithm(algorithm).HashLength;
        }

        public static string GetZeroDigestForAlgorithm(string algorithm)
        {
            uint digestSize = GetDigestSizeForAlgorithm(algorithm);

            byte[] zeroDigest = new byte[digestSize];
            Array.Clear(zeroDigest, 0, (int)digestSize);

            var buffer = CryptographicBuffer.CreateFromByteArray(zeroDigest);
            var ret = CryptographicBuffer.EncodeToHexString(buffer);

            return ret;
        }

        public static byte[] ComputeHash(string algorithm, byte[] input)
        {
            HashAlgorithmProvider sha = HashAlgorithmProvider.OpenAlgorithm(algorithm);

            var output = sha.HashData(CryptographicBuffer.CreateFromByteArray(input));

            byte [] ret;
            CryptographicBuffer.CopyToByteArray(output, out ret);

            return ret;
        }

        public static string ComputeHash(string algorithm, string input, bool isByteArray)
        {
            byte[] a;
            if (isByteArray)
            {
                // convert from characters to values ('0' -> 0, etc.)
                var inBuffer = CryptographicBuffer.DecodeFromHexString(input);
                CryptographicBuffer.CopyToByteArray(inBuffer, out a);
            }
            else
            {
                int i = 0;
                a = new byte[input.Length];
                foreach (char c in input)
                {
                    a[i++] = (byte)c;
                }
            }

            if (a.Length == 0)
                return "";

            byte[] encoded = ComputeHash(algorithm, a);
            if (encoded == null)
                return "";

            var buffer = CryptographicBuffer.CreateFromByteArray(encoded);
            var ret = CryptographicBuffer.EncodeToHexString(buffer);

            return ret;
        }
    }
}

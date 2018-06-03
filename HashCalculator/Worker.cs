using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace TPMPCRCalculator
{
    class Worker
    {
        public static string[] GetHashingAlgorithms()
        {
            return new[] {
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

        public static string GetZeroDigestForAlgorithm(string algorithm, int initialValue = 0)
        {
            uint digestSize = GetDigestSizeForAlgorithm(algorithm);

            byte[] zeroDigest = new byte[digestSize];
            Array.Clear(zeroDigest, 0, (int)digestSize);

            var buffer = CryptographicBuffer.CreateFromByteArray(zeroDigest);
            var ret = CryptographicBuffer.EncodeToHexString(buffer);

            if (initialValue != 0)
            {
                string initialString = String.Format("{0:x}", initialValue);
                int start = ret.Length - initialString.Length;
                ret = ret.Remove(start, initialString.Length).Insert(start, initialString);
            }

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
            byte[] a = ConvertStringToByteArray(input, isByteArray);

            byte[] encoded = ComputeHash(algorithm, a);
            if (encoded == null)
            {
                throw new Exception("Could not compute hash from \'" + input + "\'.");
            }

            var buffer = CryptographicBuffer.CreateFromByteArray(encoded);
            var ret = CryptographicBuffer.EncodeToHexString(buffer);

            return ret;
        }

        public static void ValidateIsHash(string algorithm, string input)
        {
            if (GetDigestSizeForAlgorithm(algorithm) != input.Length / 2)
            {
                throw new Exception("\'" + input + "\' is not the right length for hashing algorithm " + algorithm + ".");
            }
            // will throw exception if not valid byte string
            byte[] a = ConvertStringToByteArray(input, true);
        }

        private static byte[] ConvertStringToByteArray(string input, bool isByteArray)
        {
            byte[] byteArray = null;
            if (isByteArray)
            {
                if (input.Length % 2 == 1)
                {
                    throw new Exception("\'" + input + "\' is missing a character to be a valid byte string.");
                }
                IBuffer inBuffer = null;
                try
                {
                    // convert from characters to values ('0' -> 0, etc.)
                    inBuffer = CryptographicBuffer.DecodeFromHexString(input);
                }
                catch (Exception e)
                {
                    throw new Exception("\'" + input + "\' is not a valid byte string.", e);
                }
                CryptographicBuffer.CopyToByteArray(inBuffer, out byteArray);
                if (byteArray == null)
                {
                    throw new Exception("\'" + input + "\' could not be converted into a byte stream.");
                }
            }
            else
            {
                int i = 0;
                byteArray = new byte[input.Length];
                foreach (char c in input)
                {
                    byteArray[i++] = (byte)c;
                }
            }
            if (byteArray.Length == 0)
            {
                throw new Exception("Byte array generated from \'" + input + "\' is empty.");
            }
            return byteArray;
        }

        public static byte[] ComputeHmacHash(string algorithm, byte[] input, byte[] key)
        {
            if (algorithm == MacAlgorithmNames.HmacSha1)
            {
                HMACSHA1 hmac = new HMACSHA1(key);
                return hmac.ComputeHash(input);
            }
            else if (algorithm == MacAlgorithmNames.HmacSha256)
            {
                HMACSHA256 hmac = new HMACSHA256(key);
                return hmac.ComputeHash(input);
            }
            else if (algorithm == MacAlgorithmNames.HmacSha384)
            {
                HMACSHA384 hmac = new HMACSHA384(key);
                return hmac.ComputeHash(input);
            }
            else if (algorithm == MacAlgorithmNames.HmacSha512)
            {
                HMACSHA512 hmac = new HMACSHA512(key);
                return hmac.ComputeHash(input);
            }

            return null;
        }

        public static string ComputeHmacHash(string algorithm, string input, string key, bool isByteArray)
        {
            byte[] inputArray = ConvertStringToByteArray(input, isByteArray);
            byte[] keyArray = ConvertStringToByteArray(key, isByteArray);

            byte[] encoded = ComputeHmacHash(algorithm, inputArray, keyArray);
            if (encoded == null)
            {
                throw new Exception("Could not compute HMAC hash from \'" + input + "\' with key \'" + key + "\'.");
            }

            var buffer = CryptographicBuffer.CreateFromByteArray(encoded);
            var ret = CryptographicBuffer.EncodeToHexString(buffer);

            return ret;
        }

        public static int CheckIfRightHashOrder(string algorithm, int startValue, string[] hashes, string expectedResult)
        {
            string currentValue = GetZeroDigestForAlgorithm(algorithm, startValue);

            for (int index = 0; index < hashes.Length; index++)
            {
                currentValue = ComputeHash(algorithm, currentValue + hashes[index], true);
                if (String.Compare(currentValue, expectedResult) == 0)
                {
                    return index;
                }
            }
            return -1;
        }
    }
}

using System;
using System.Security.Cryptography;
using System.Text;
using RIS.Randomizing;

namespace DestroyComments.Utils
{
    public static class RandomUtils
    {
        private static readonly char[] StringGeneratorChars;
        private static readonly RNGCryptoServiceProvider RandomGenerator;
        private static readonly Random Random;

        static RandomUtils()
        {
            StringGeneratorChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            RandomGenerator = new RNGCryptoServiceProvider();
            Random = Rand.CreateRandom();
        }

        public static int RandomInt()
        {
            return Random.Next();
        }
        public static int RandomInt(int max)
        {
            return Random.Next(max);
        }
        public static int RandomInt(int min, int max)
        {
            return Random.Next(min, max);
        }

        public static int GenerateInt()
        {
            return RandomNumberGenerator.GetInt32(int.MaxValue);
        }
        public static int GenerateInt(int max)
        {
            return RandomNumberGenerator.GetInt32(max);
        }
        public static int GenerateInt(int min, int max)
        {
            return RandomNumberGenerator.GetInt32(min, max);
        }

        public static byte[] GenerateBytes(int size)
        {
            byte[] result = new byte[size];

            RandomGenerator.GetBytes(result, 0, size);

            return result;
        }

        public static string GenerateString(int size)
        {
            StringBuilder result = new StringBuilder(size);
            byte[] randomBytes = new byte[size * 4];

            randomBytes = GenerateBytes(randomBytes.Length);

            for (int i = 0; i < size; ++i)
            {
                uint randomNumber = BitConverter.ToUInt32(randomBytes, i * 4);
                long charIndex = randomNumber % StringGeneratorChars.Length;

                result.Append(StringGeneratorChars[charIndex]);
            }

            return result.ToString();
        }
        public static string GenerateString(int minSize, int maxSize)
        {
            int size = minSize < maxSize
                ? GenerateInt(minSize, maxSize)
                : minSize;

            return GenerateString(size);
        }
    }
}

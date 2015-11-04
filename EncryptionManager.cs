using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ShookaLogReader
{
	public static class EncryptionManager
	{
		private const string HASH_ALGORITM = "SHA1";//SHA1 or MD5
		private const String SALT_VALUE = "Abc1@#23!56GH";//any character
		private const string PASSPHRASE = "la12ie58@#$6&";//any character
		private const int PASSWORD_ITERATION = 2;//one or two iteration is enough
		private const string INIT_VECTOR = "gdhety#@!$%^1235";//must be exactly 16 ascii character
		private const int KEY_SIZE = 256;// it can be 128 , 192 and 256 , longer keys are more secure than shorter keys

		/// <summary>
		/// to encrypt text
		/// </summary>
		/// <param name="plainText">text that should be encrypt</param>
		/// <param name="passPhrase">any character</param>
		/// <param name="saltValue">any character</param>
		/// <param name="hashAlgorithm">SHA1, MD5</param>
		/// <param name="passwordIterations">one or two</param>
		/// <param name="initVector">must be exactly 16 ascii character</param>
		/// <param name="keySize">128,192 and 256 , longer is better</param>
		/// <returns></returns>
		public static string Encrypt(string plainText)
		{
			var initVectorBytes = Encoding.ASCII.GetBytes(INIT_VECTOR);
			var saltValueBytes = Encoding.ASCII.GetBytes(SALT_VALUE);

			var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

			var password = new PasswordDeriveBytes(PASSPHRASE, saltValueBytes, HASH_ALGORITM, PASSWORD_ITERATION);

			var keyBytes = password.GetBytes(KEY_SIZE / 8);

			var symmetricKey = new RijndaelManaged();

			symmetricKey.Mode = CipherMode.CBC;

			var encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);

			var memoryStream = new MemoryStream();

			var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

			cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);

			cryptoStream.FlushFinalBlock();
			cryptoStream.Clear();

			var cipherTextBytes = memoryStream.ToArray();

			memoryStream.Close();
			cryptoStream.Close();

			var cipherText = Convert.ToBase64String(cipherTextBytes);

			return cipherText;
		}

		public static string Decrypt(string cipherText)
		{
			var initVectorBytes = Encoding.ASCII.GetBytes(INIT_VECTOR);
			var saltValueBytes = Encoding.ASCII.GetBytes(SALT_VALUE);

			var cipherTextBytes = Convert.FromBase64String(cipherText);

			var password = new PasswordDeriveBytes(PASSPHRASE, saltValueBytes, HASH_ALGORITM, PASSWORD_ITERATION);

			var keyBytes = password.GetBytes(KEY_SIZE / 8);

			var symmetricKey = new RijndaelManaged();
			symmetricKey.Mode = CipherMode.CBC;
            symmetricKey.Padding = PaddingMode.None;
			var decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);

			var memoryStream = new MemoryStream(cipherTextBytes);
			var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

			var plainTextBytes = new byte[cipherTextBytes.Length];

			int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

			cryptoStream.Clear();

			memoryStream.Close();
			cryptoStream.Close();

			var plainText = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);

			return plainText;
		}
	}
}
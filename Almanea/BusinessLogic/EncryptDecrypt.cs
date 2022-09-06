using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Almanea.BusinessLogic
{

	public class EncryptDecrypt
	{
		private const string initVector = "pemgail9uzpgzl88";

		public static string Encrypt_Key = "8UHjPgXZzXCGkhxV2QCnooyJexUzvJrO";

		private const int keysize = 256;

		public static string Encrypt(string plainText)
		{
			string passPhrase = Encrypt_Key;
			byte[] initVectorBytes = Encoding.UTF8.GetBytes("pemgail9uzpgzl88");
			byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
			PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
			byte[] keyBytes = password.GetBytes(32);
			RijndaelManaged symmetricKey = new RijndaelManaged();
			symmetricKey.Mode = CipherMode.CBC;
			ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
			MemoryStream memoryStream = new MemoryStream();
			CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
			cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
			cryptoStream.FlushFinalBlock();
			byte[] cipherTextBytes = memoryStream.ToArray();
			memoryStream.Close();
			cryptoStream.Close();
			return Base64Encode(Convert.ToBase64String(cipherTextBytes));
		}

		public static string Decrypt(string cipherText)
		{
			string passPhrase = Encrypt_Key;
			byte[] initVectorBytes = Encoding.UTF8.GetBytes("pemgail9uzpgzl88");
			byte[] cipherTextBytes = Convert.FromBase64String(Base64Decode(cipherText));
			PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
			byte[] keyBytes = password.GetBytes(32);
			RijndaelManaged symmetricKey = new RijndaelManaged();
			symmetricKey.Mode = CipherMode.CBC;
			ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
			MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
			CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
			byte[] plainTextBytes = new byte[cipherTextBytes.Length];
			int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
			memoryStream.Close();
			cryptoStream.Close();
			return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
		}

		public static string Base64Encode(string plainText)
		{
			try
			{
				byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
				return Convert.ToBase64String(plainTextBytes);
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		public static string Base64Decode(string base64EncodedData)
		{
			try
			{
				byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
				return Encoding.UTF8.GetString(base64EncodedBytes);
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
	}
}
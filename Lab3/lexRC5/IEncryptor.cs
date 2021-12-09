using System.IO;

namespace lexRC5
{
	internal interface IEncryptor<TKey>
	{
		TKey Key { set; }
		void Encrypt(Stream source, Stream destination);
		void Decrypt(Stream source, Stream destination);
	}
}

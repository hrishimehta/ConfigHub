using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigHub.Client.Encryption
{
    public interface IEncryptor
    {
        string Encrypt(string plaintext, string key);
        string Decrypt(string ciphertext, string key);
    }
}

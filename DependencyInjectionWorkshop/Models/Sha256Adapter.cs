using System.Text;

namespace DependencyInjectionWorkshop.Models
{
    public interface IHash
    {
        string Hash(string plainText);
    }

    public class Sha256Adapter : IHash
    {
        public string Hash(string plainText)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(plainText));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }
    }
}
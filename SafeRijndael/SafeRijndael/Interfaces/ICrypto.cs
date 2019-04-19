using System.Threading;
using System.Threading.Tasks;

namespace SafeRijndael
{
    public interface ICrypto
    {
        Task WriteStreamAsync(string password, string readPath, string outPath, CancellationToken token);

        CryptoMode CryptoMode { get; set; }
    }
}

using System.Threading.Tasks;
using PDFinch.TestClient.Shared;

namespace PDFinch.TestClient.NET50
{
    internal static class Program
    {
        private static async Task Main()
        {
            await SharedProgram.MainAsync();
        }
    }
}

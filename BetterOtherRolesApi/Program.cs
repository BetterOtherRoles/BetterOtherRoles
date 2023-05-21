namespace BetterOtherRolesApi;

internal static class Program
{
    internal static void Main(string[] args)
    {
        BorServer.Instance.Debug = true;
        BorServer.Instance.Start();
    }
}
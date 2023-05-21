using System.Collections.Generic;

namespace BetterOtherRoles.EnoFw.Libs.SocketIOClient.JsonSerializer;

public class JsonSerializeResult
{
    public string Json { get; set; }
    public IList<byte[]> Bytes { get; set; }
}
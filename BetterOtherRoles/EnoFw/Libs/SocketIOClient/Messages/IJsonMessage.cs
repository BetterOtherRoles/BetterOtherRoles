using System.Collections.Generic;
using System.Text.Json;

namespace BetterOtherRoles.EnoFw.Libs.SocketIOClient.Messages;

public interface IJsonMessage : IMessage
{
    List<JsonElement> JsonElements { get; }
}
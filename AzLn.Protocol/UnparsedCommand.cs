#nullable enable
using Newtonsoft.Json;

namespace AzLn.Protocol
{
    public class UnparsedCommand : BaseCommand
    {
        [JsonIgnore] public sealed override ushort CommandId { get; set; } = ushort.MaxValue;
        [JsonIgnore] public sealed override bool FromServer { get; set; }
        [JsonIgnore] public sealed override ushort Index { get; set; } = 0;
        [JsonIgnore] public byte[] Payload { get; set; }

        public UnparsedCommand(byte[] payload, ushort commandId, bool fromServer, ushort index = 0)
        {
            Payload = payload;
            CommandId = commandId;
            FromServer = fromServer;
            Index = index;
        }

        public override byte[] PayloadPSerialize()
        {
            return Payload;
        }

        public override byte[] FullPSerialize()
        {
            return FullPSerialize(Payload, CommandId, Index);
        }
    }
}
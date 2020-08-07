using System;
using AzLn.Contract;

namespace AzLn.GameClient.CommandsLogger
{
    public class DbCommandLogEntry
    {
        public long Id { get; set; }
        public Guid Guid { get; set; }
        public uint UserId { get; set; }
        public EGameServer GameServer { get; set; }
        public ECommandType CommandType { get; set; }
        public ushort Index { get; set; }
        public ushort CommandId { get; set; }
        public byte[] Bytes { get; set; } = new byte[0];
    }
}
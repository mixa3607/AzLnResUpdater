#nullable enable
using System;
using System.IO;
using Newtonsoft.Json;
using NLog;
using ProtoBuf;

namespace AzLn.Protocol
{
    public abstract class BaseCommand : ICommand
    {
        public const byte AdvHeaderLen = 7;
        public const byte HeaderLen = 2;
        public const byte HeaderNoIdLen = AdvHeaderLen - HeaderLen;

        [JsonIgnore] public virtual ushort CommandId { get; set; }
        [JsonIgnore] public virtual bool FromServer { get; set; }
        [JsonIgnore] public virtual ushort Index { get; set; } = 0;

        [JsonIgnore] public static ILogger Logger { get; set; }

        public virtual byte[] PayloadPSerialize()
        {
            var memStream = new MemoryStream();
            Serializer.Serialize(memStream, this);
            memStream.Position = 0;
            return memStream.ToArray();
        }

        public virtual byte[] FullPSerialize()
        {
            var payloadBytes = PayloadPSerialize();
            return FullPSerialize(payloadBytes, CommandId, Index);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static byte[] FullPSerialize(byte[] payload, ushort commandId, ushort index)
        {
            var rawCommandBytes = new byte[payload.Length + AdvHeaderLen];
            //write len (payloadLen + 5)
            rawCommandBytes[0] = (byte) ((ushort) (payload.Length + AdvHeaderLen - HeaderLen) >> 8);
            rawCommandBytes[1] = (byte) (payload.Length + AdvHeaderLen - HeaderLen);
            //write command id
            rawCommandBytes[3] = (byte) (commandId >> 8);
            rawCommandBytes[4] = (byte) commandId;
            //write idx
            rawCommandBytes[5] = (byte) (index >> 8);
            rawCommandBytes[6] = (byte) index;

            Array.Copy(payload, 0, rawCommandBytes, 7, payload.Length);
            return rawCommandBytes;
        }

        public static ICommand PDeserialize(ushort commandId, byte[] commandPayload)
        {
            if (!AllCommands.Commands.ContainsKey(commandId))
            {
                throw new CommandNotFoundException($"Command with id {commandId} not found");
            }

            return (BaseCommand) Serializer.NonGeneric.Deserialize(AllCommands.Commands[commandId],
                new ReadOnlyMemory<byte>(commandPayload));
        }

        public static ICommand PDeserialize(ushort commandId, byte[] commandPayload, ushort index)
        {
            var command = PDeserialize(commandId, commandPayload);
            command.Index = index;
            return command;
        }

        public static bool TryPDeserialize(ushort commandId, byte[] commandPayload, ref ICommand? command, bool ifFailRetUnp = false, bool fromSrvForUnp = false)
        {
            if (!AllCommands.Commands.ContainsKey(commandId))
            {
                Logger.Debug("Cant deserialize bcs command id: {CommandId} not found", commandId);
                if (ifFailRetUnp)
                {
                    command = new UnparsedCommand(commandPayload, commandId, fromSrvForUnp);
                }
                return false;
            }

            try
            {
                command = (ICommand) Serializer.NonGeneric.Deserialize(AllCommands.Commands[commandId],
                    new ReadOnlyMemory<byte>(commandPayload));
                return true;
            }
            catch (Exception e)
            {
                Logger.Debug(e, "Cant deserialize catch exception from proto deserialized");
                if (ifFailRetUnp)
                {
                    command = new UnparsedCommand(commandPayload, commandId, fromSrvForUnp);
                }
                return false;
            }
        }

        public static bool TryPDeserialize(ushort commandId, byte[] commandPayload, ushort index, ref ICommand? command, bool ifFailRetUnp = false, bool fromSrvForUnp = false)
        {
            if (TryPDeserialize(commandId, commandPayload, ref command, ifFailRetUnp, fromSrvForUnp))
            {
                command!.Index = index;
                return true;
            }
            else if (ifFailRetUnp)
            {
                command!.Index = index;
            }
            return false;
        }
    }
}
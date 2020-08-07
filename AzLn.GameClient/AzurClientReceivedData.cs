namespace AzLn.GameClient
{
    public struct AzurClientData
    {
        public enum EDataType
        {
            Command,
            UnparsedCommand,
            String,
            Bytes
        }

        public AzurClientData(EDataType type, object data)
        {
            Type = type;
            Data = data;
        }

        public EDataType Type;
        public object Data;
    }
}
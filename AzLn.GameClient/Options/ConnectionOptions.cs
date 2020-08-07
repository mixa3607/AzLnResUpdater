namespace AzLn.GameClient.Options
{
    public class ConnectionOptions : IConnectionOptions
    {
        public int InitBufferLen { get; set; }
        public int ReadBatchLen { get; set; }
        public int BufferResizeStep { get; set; }
    }
}
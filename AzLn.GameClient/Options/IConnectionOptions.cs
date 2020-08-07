namespace AzLn.GameClient.Options
{
    public interface IConnectionOptions
    {
        int InitBufferLen { get; }
        int ReadBatchLen { get; }
        int BufferResizeStep { get; }
    }
}
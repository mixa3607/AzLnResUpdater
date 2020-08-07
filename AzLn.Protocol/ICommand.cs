namespace AzLn.Protocol
{
    /// <summary>
    /// Interface for all commands
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Command identifier
        /// </summary>
        ushort CommandId { get; set; }

        /// <summary>
        /// Set if this command receiving from game/gate server
        /// </summary>
        bool FromServer { get; set; }

        ushort Index { get; set; }

        /// <summary>
        /// Serialize all proto members to json
        /// </summary>
        /// <returns>json serialized class</returns>
        string ToString();

        /// <summary>
        /// Serialize current command to proto bytes
        /// </summary>
        /// <returns></returns>
        byte[] PayloadPSerialize();

		byte[] FullPSerialize();
    }
}

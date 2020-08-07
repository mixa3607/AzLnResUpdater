using System;

namespace AzLn.GameClient
{
    public class ClientModeChangedEvent : EventArgs
    {
        public EClientMode ClientMode { get; }

        public ClientModeChangedEvent(EClientMode clientMode)
        {
            ClientMode = clientMode;
        }
    }
}
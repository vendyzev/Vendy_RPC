namespace CustomRPC
{
    public enum ConnectionState
    {
        None = -1,
        Disconnected,
        Connecting,
        UpdatingPresence,
        Connected,
        Error,
    }

    public static class ConnectionManager
    {
        static ConnectionState current = ConnectionState.Disconnected;
        static ConnectionState previous = ConnectionState.None;

        public static ConnectionState State
        {
            get
            {
                return current;
            }
            set
            {
                if (value == ConnectionState.None)
                    throw new System.ComponentModel.InvalidEnumArgumentException("Attempt to set State to ConnectionState.None.");

                if (current != ConnectionState.Connecting)
                    previous = current;
                current = value;
            }
        }

        public static bool HasChanged()
        {
            return current != previous;
        }
    }
}
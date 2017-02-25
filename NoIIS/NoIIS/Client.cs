using System;

namespace NoIIS
{
    /// <summary>
    /// This struct gets used in order to track statistics for each connection. This allows
    /// us to block connections if a client uses too many connections.
    /// </summary>
    public struct Client
    {
        /// The remote address of the client:
        public string Address;

        /// Is this client blocked?
        public bool Blocked;
        
        /// When was the last visit of this client?
        public DateTime LastVisitUTC;
        
        /// If this client was blocked, how long?
        public DateTime BlockedUntilUTC;

        /// Is this client entered i.e. was the entry successful?
        public bool Entered;
    }
}
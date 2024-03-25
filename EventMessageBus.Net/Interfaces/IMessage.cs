using System;
namespace bnoffer.EventMessageBus.Interfaces
{
    /// <summary>
    /// Base message for the message bus
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// Identifies if the callback has to be run on the main thread
        /// </summary>
        bool IsOnMainThread { get; }
    }
}

using System.Collections.Concurrent;
using bnoffer.EventMessageBus.Interfaces;

namespace bnoffer.EventMessageBus;

/// <summary>
/// Provides a message bus
/// </summary>
public class MessageBus : IMessageBus
{
    /// <summary>
    /// Subscriptions
    /// </summary>
    private readonly ConcurrentDictionary<Type, List<object>> _observers
        = new ConcurrentDictionary<Type, List<object>>();

    /// <summary>
    /// Removes all subscriptions
    /// </summary>
    public void ClearAllSubscriptions()
    {
        _observers.Clear();
    }

    /// <summary>
    /// Publish a message to the bus
    /// </summary>
    /// <typeparam name="T">Message Type</typeparam>
    /// <param name="message">Message to be published</param>
    public void Publish<T>(T? message) where T : IMessage
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        var messageType = typeof(T);
        if (_observers.TryGetValue(messageType, out var subscriptions))
        {
            Task.Factory.StartNew(() =>
            {
                if (subscriptions.Count == 0) return;
                foreach (var handler in subscriptions
                    .Select(s => s as ISubscription<T>)
                    .Select(s => s?.ActionHandler))
                {
                        handler?.Invoke(message);
                }
            });
        }
    }

    /// <summary>
    /// Subscribe a callback for a message type
    /// </summary>
    /// <typeparam name="T">Message Type</typeparam>
    /// <param name="callback">Callback</param>
    /// <returns>Subscription object needed for unsubscribing</returns>
    public ISubscription<T>? Subscribe<T>(Action<T> callback) where T : IMessage
    {
        ISubscription<T>? subscription = null;

        var messageType = typeof(T);
        var subscriptions = _observers.TryGetValue(messageType, out var observer) ?
            observer : [];

        if (subscriptions.Select(s => s as ISubscription<T>).All(s => s?.ActionHandler != callback))
        {
            subscription = new Subscription<T>(callback);
            subscriptions.Add(subscription);
        }

        _observers[messageType] = subscriptions;

        return subscription;
    }

    /// <summary>
    /// Unsubscribe the specified subscription
    /// </summary>
    /// <typeparam name="T">Message Type</typeparam>
    /// <param name="subscription">Subscription to unsubscribe</param>
    /// <returns>true on success, else false</returns>
    public bool UnSubscribe<T>(ISubscription<T>? subscription) where T : IMessage
    {
        var removed = false;

        if (subscription == null) return false;

        var messageType = typeof(T);
        if (_observers.TryGetValue(messageType, out var value))
        {
            removed = value.Remove(subscription);

            if (value.Count == 0)
                _observers.Remove(messageType, out var _);
        }
        return removed;
    }
}

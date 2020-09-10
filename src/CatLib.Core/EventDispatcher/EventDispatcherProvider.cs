namespace CatLib.EventDispatcher
{
    /// <summary>
    /// Provide the event dispatcher.
    /// </summary>
    public class EventDispatcherProvider : ServiceProvider
    {
        /// <inheritdoc />
        public override void Register()
        {
            App.Singleton<IEventDispatcher, EventDispatcher>();
        }
    }
}

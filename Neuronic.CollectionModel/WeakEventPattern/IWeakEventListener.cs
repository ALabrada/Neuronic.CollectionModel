using System;

namespace Neuronic.CollectionModel.WeakEventPattern
{
#if NETSTD
    /// <summary>
    /// Provides event listening support for classes that expect to receive events through the WeakEvent pattern.
    /// </summary>
    public interface IWeakEventListener
    {
        /// <summary>
        /// Receives events from the centralized event manager.
        /// </summary>
        /// <param name="managerType">Type of the weak event manager manager.</param>
        /// <param name="sender">Object that originated the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <returns><c>true</c> if the listener handled the event.</returns>
        bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e);
    }
#endif
}

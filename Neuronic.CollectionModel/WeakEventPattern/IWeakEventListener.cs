using System;

namespace Neuronic.CollectionModel.WeakEventPattern
{
#if NETSTD
    internal interface IWeakEventListener
    {
        bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e);
    }
#endif
}

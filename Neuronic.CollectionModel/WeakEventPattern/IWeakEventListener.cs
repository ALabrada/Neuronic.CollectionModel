using System;

namespace Neuronic.CollectionModel.WeakEventPattern
{
    internal interface IWeakEventListener
    {
        bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e);
    }
}
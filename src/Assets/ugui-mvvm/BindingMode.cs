using System;

namespace uguimvvm
{
    public enum BindingMode
    {
        OneTime,
        OneWayToTarget,
        OneWayToSource,
        TwoWay,
        [Obsolete]
        OneWayToView,
    }
}

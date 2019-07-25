using System;
using System.ComponentModel;
using UnityEngine;

namespace uguimvvm
{
    [Obsolete("INPCBinding is obsolete.  You should use PropertyBinding instead.")]
    [HideInInspector]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class INPCBinding : PropertyBinding
    {
        // INPCBinding was renamed to PropertyBinding.  This file only exists to support legacy apps to upgrade.  See INPCBindingToPropertyBindingUpgradeEditor.cs
    }
}

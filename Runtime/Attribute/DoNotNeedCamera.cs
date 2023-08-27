using System;

namespace Elly.Runtime
{
    /// <summary>
    /// When class inherit <seealso cref="CharacterBase"/> and have this attribute <br />
    /// their editor will trying to hide control fields
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [DoNotIncludeIntoDocs]
    public class DoNotNeedCamera : Attribute { }
}

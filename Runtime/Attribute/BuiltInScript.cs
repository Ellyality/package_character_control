using System;

namespace Ellyality
{
    /// <summary>
    /// Mark it as built-in script
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [DoNotIncludeIntoDocs]
    public class BuiltInScript : Attribute { }
}

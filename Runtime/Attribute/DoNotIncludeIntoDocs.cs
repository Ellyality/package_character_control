using System;

namespace Ellyality
{
    /// <summary>
    /// Mark it as built-in script
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public class DoNotIncludeIntoDocs : Attribute { }
}

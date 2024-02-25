using System;

namespace Unbound.Core.Networking
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class UnboundRPC : Attribute
    {
        public string EventID { get; set; }
    }
}

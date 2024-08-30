using System;

namespace Unbound.Networking.RPCs{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class UnboundRPC : Attribute
    {
        public string EventID { get; set; }
    }
}

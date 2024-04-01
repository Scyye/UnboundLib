using System;

namespace UnboundLib.Networking.RPCs{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class UnboundRPC : Attribute
    {
        public string EventID { get; set; }
    }
}

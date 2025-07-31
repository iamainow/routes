using System.Net;

namespace NativeMethods
{
    public class Ip4RouteEntry
    {
        public required IPAddress DestinationIP { get; set; }
        public required IPAddress SubnetMask { get; set; }
        public required IPAddress GatewayIP { get; set; }
        public int InterfaceIndex { get; set; }
        public int ForwardType { get; set; }
        public int ForwardProtocol { get; set; }
        public int ForwardAge { get; set; }
        public int Metric { get; set; }
    }
}
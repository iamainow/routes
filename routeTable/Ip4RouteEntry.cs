using System.Net;

namespace routeTable
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

        public static implicit operator Ip4RouteCreateDto(Ip4RouteEntry entry)
        {
            return new Ip4RouteCreateDto
            {
                DestinationIP = entry.DestinationIP,
                SubnetMask = entry.SubnetMask,
                GatewayIP = entry.GatewayIP,
                InterfaceIndex = entry.InterfaceIndex,
                Metric = entry.Metric,
            };
        }

        public static implicit operator Ip4RouteDeleteDto(Ip4RouteEntry entry)
        {
            return new Ip4RouteDeleteDto
            {
                DestinationIP = entry.DestinationIP,
                SubnetMask = entry.SubnetMask,
                GatewayIP = entry.GatewayIP,
                InterfaceIndex = entry.InterfaceIndex,
            };
        }
    }
}
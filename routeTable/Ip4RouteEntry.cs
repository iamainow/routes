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

        public static implicit operator Ip4RouteCreateDto(Ip4RouteEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);
            return entry.ToIp4RouteCreateDto();
        }

        public static implicit operator Ip4RouteDeleteDto(Ip4RouteEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);
            return entry.ToIp4RouteDeleteDto();
        }

        public Ip4RouteCreateDto ToIp4RouteCreateDto()
        {
            return new Ip4RouteCreateDto
            {
                DestinationIP = DestinationIP,
                SubnetMask = SubnetMask,
                GatewayIP = GatewayIP,
                InterfaceIndex = InterfaceIndex,
                Metric = Metric,
            };
        }

        public Ip4RouteDeleteDto ToIp4RouteDeleteDto()
        {
            return new Ip4RouteDeleteDto
            {
                DestinationIP = DestinationIP,
                SubnetMask = SubnetMask,
                GatewayIP = GatewayIP,
                InterfaceIndex = InterfaceIndex,
            };
        }
    }
}
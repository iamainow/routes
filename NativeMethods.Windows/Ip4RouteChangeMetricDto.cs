using System.Net;

namespace NativeMethods.Windows;

public class Ip4RouteChangeMetricDto
{
    public required IPAddress DestinationIP { get; set; }
    public required IPAddress SubnetMask { get; set; }
    public required IPAddress GatewayIP { get; set; }
    public int InterfaceIndex { get; set; }
    public int Metric { get; set; }
}
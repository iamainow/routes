using System.Net;

namespace RoutesCalculator;

public static class RoutesDifferenceCalculator
{
    public static void CalculateDifference(
        RouteWithMetricDto[] source,
        RouteWithMetricDto[] target,
        Action<RouteWithMetricDto>? toAdd = null,
        Action<RouteWithMetricDto>? toRemove = null,
        Action<ChangeMetricRouteDto>? toChangeMetric = null,
        Action<RouteWithMetricDto>? toUnchanged = null)
    {
        Dictionary<RouteWithoutMetricDto, int> sourceDictionary = source.ToDictionary(x => x.RouteWithoutMetric, x => x.Metric);
        Dictionary<RouteWithoutMetricDto, int> targetDictionary = target.ToDictionary(x => x.RouteWithoutMetric, x => x.Metric);
        RouteWithoutMetricDto[] commonRoutes = sourceDictionary.Keys.Intersect(targetDictionary.Keys).ToArray();

        if (toUnchanged is not null)
        {
            foreach (RouteWithoutMetricDto commonRoute in commonRoutes)
            {
                int oldMetric = sourceDictionary[commonRoute];
                int newMetric = targetDictionary[commonRoute];

                if (oldMetric == newMetric)
                {
                    toUnchanged(new RouteWithMetricDto(commonRoute, oldMetric));
                }
            }
        }

        if (toChangeMetric is not null)
        {
            foreach (RouteWithoutMetricDto commonRoute in commonRoutes)
            {
                int oldMetric = sourceDictionary[commonRoute];
                int newMetric = targetDictionary[commonRoute];

                if (oldMetric != newMetric)
                {
                    toChangeMetric(new ChangeMetricRouteDto(commonRoute, oldMetric, newMetric));
                }
            }
        }

        if (toRemove is not null)
        {
            IEnumerable<RouteWithoutMetricDto> routesToDelete = sourceDictionary.Keys.Except(commonRoutes);
            foreach (RouteWithoutMetricDto routeToDelete in routesToDelete)
            {
                toRemove(new RouteWithMetricDto(routeToDelete, sourceDictionary[routeToDelete]));
            }
        }

        if (toAdd is not null)
        {
            IEnumerable<RouteWithoutMetricDto> routesToAdd = targetDictionary.Keys.Except(commonRoutes);
            foreach (RouteWithoutMetricDto routeToAdd in routesToAdd)
            {
                toAdd(new RouteWithMetricDto(routeToAdd, targetDictionary[routeToAdd]));
            }
        }
    }
}

public record struct RouteWithoutMetricDto(IPAddress DestinationIP, IPAddress SubnetMask, IPAddress GatewayIP);

public record struct RouteWithMetricDto(RouteWithoutMetricDto RouteWithoutMetric, int Metric);

public record struct ChangeMetricRouteDto(RouteWithoutMetricDto RouteWithoutMetric, int OldMetric, int NewMetric);
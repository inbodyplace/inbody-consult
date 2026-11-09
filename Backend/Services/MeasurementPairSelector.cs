using Backend.Models;

namespace Backend.Services;

internal static class MeasurementPairSelector
{
    public static (InBodyMeasurement Before, InBodyMeasurement After)? FindCompletedPair(
        IEnumerable<InBodyMeasurement> source)
    {
        var measurements = source.Where(IsComplete).ToArray();
        return measurements
            .SelectMany(after => measurements
                .Where(before => before.MeasuredAt < after.MeasuredAt)
                .Select(before => new
                {
                    Before = before,
                    After = after,
                    Days = (after.MeasuredAt - before.MeasuredAt).TotalDays,
                }))
            .Where(pair => pair.Days >= 30)
            .OrderByDescending(pair => pair.After.MeasuredAt)
            .ThenBy(pair => Math.Abs(pair.Days - 30))
            .ThenByDescending(pair => pair.Before.MeasuredAt)
            .Select(pair => ((InBodyMeasurement Before, InBodyMeasurement After)?)(pair.Before, pair.After))
            .FirstOrDefault();
    }

    private static bool IsComplete(InBodyMeasurement measurement) =>
        measurement.Smm.HasValue
        && measurement.Bfm.HasValue
        && measurement.Weight.HasValue
        && measurement.Pbf.HasValue;
}

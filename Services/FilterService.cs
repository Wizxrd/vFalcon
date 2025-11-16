using vFalcon.Models;
namespace vFalcon.Services;

public class FilterService
{
    public static bool WithinFilterParam(Pilot pilot)
    {
        if (pilot.FlightPlan == null) return false;

        var fs = App.Profile.FilterSettings;
        var flightPlan = pilot.FlightPlan;

        string dep = (string?)flightPlan["departure"] ?? string.Empty;
        string arr = (string?)flightPlan["arrival"] ?? string.Empty;
        string route = (string?)flightPlan["route"] ?? string.Empty;
        string callsign = pilot.Callsign ?? string.Empty;

        bool Match(string value, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return false;
            if (string.IsNullOrWhiteSpace(value)) return false;
            return value.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        bool WithinLoHi(int low, int high, int altitudeFeet)
        {
            if (low > high) return false;
            int altitudeHundreds = altitudeFeet / 100;
            return altitudeHundreds >= low && altitudeHundreds <= high;
        }

        bool IsSet(string s) => !string.IsNullOrWhiteSpace(s);

        if (fs.RequireAll)
        {
            bool ok = true;

            if (IsSet(fs.Departure)) 
                ok &= Match(dep, fs.Departure);

            if (IsSet(fs.Arrival))
                ok &= Match(arr, fs.Arrival);

            if (IsSet(fs.Sid))
                ok &= Match(route, fs.Sid);

            if (IsSet(fs.Star))
                ok &= Match(route, fs.Star);

            if (IsSet(fs.Airline))
                ok &= Match(callsign, fs.Airline);

            ok &= WithinLoHi(fs.AltLow, fs.AltHigh, pilot.Altitude);

            return ok;
        }

        bool match = false;

        if (Match(dep, fs.Departure)) match = true;
        if (Match(arr, fs.Arrival)) match = true;
        if (Match(route, fs.Sid)) match = true;
        if (Match(route, fs.Star)) match = true;
        if (Match(callsign, fs.Airline)) match = true;

        if (WithinLoHi(fs.AltLow, fs.AltHigh, pilot.Altitude))
            match = true;

        return match;
    }

}

namespace vFalcon.Utils;
public class Frequency
{
    public static string ConvertToMhz(int frequencyHz)
    {
        double frequencyMHz = frequencyHz / 1_000_000.0;
        return frequencyMHz.ToString("F3");
    }
}

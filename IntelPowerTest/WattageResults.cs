namespace ConsoleApp5
{
    public class WattageResults
    {
        public double MaxWattage { get; set; }
        public double MinWattage { get; set; }
        public double MeanWatts { get; set; }
        public double MedianWatts { get; set; }
        public double ModeWholeWatt { get; set; }
        public double StdDev { get; set; }
        public int NumberOfSamples { get; set; }

        public string ToHumanReadableString()
        {
            return $"Mean: {MeanWatts}\nMedian: {MedianWatts}\nMode: {ModeWholeWatt}\nStdDev: {StdDev}\nSample #: {NumberOfSamples}\nMin (W): {MinWattage}\nMax (W): {MaxWattage}";
        }

        public override string ToString()
        {
            return $"{MeanWatts},{MedianWatts},{ModeWholeWatt},{StdDev},{NumberOfSamples},{MinWattage},{MaxWattage}"; 
        }
    }
}

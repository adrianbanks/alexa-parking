namespace Parking
{
    internal sealed class CarPark
    {
        public string Name { get; }
        public int NumberOfFreeSpaces { get; }
        public int PercentFull { get; }
        public SpaceUsageDirection UsageDirection { get; }

        public CarPark(string name, int numberOfFreeSpaces, int percentFull, SpaceUsageDirection usageDirection)
        {
            Name = name;
            NumberOfFreeSpaces = numberOfFreeSpaces;
            PercentFull = percentFull;
            UsageDirection = usageDirection;
        }
    }
}

namespace Parking
{
    internal sealed class CarPark
    {
        public string Name { get; set; }
        public int NumberOfFreeSpaces { get; set; }
        public int PercentFull { get; set; }
        public SpaceUsageDirection UsageDirection { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Parking
{
    internal static class CarParkParser
    {
        public static IEnumerable<CarPark> ParseFromHtml(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var carParkNames = document.DocumentNode
                .SelectNodes("//div[@class='col-md-12 main-content mt-3']/h2[@class='highlight']/text()")
                .Select(n => n.InnerText)
                .ToList();

            var carParkSpaces = document.DocumentNode
                .SelectNodes("//div[@class='col-md-12 main-content mt-3']/p/strong/text()")
                .Select(n => n.InnerText)
                .ToList();

            var carParkFillingData = document.DocumentNode
                .SelectNodes("//div[@class='col-md-12 main-content mt-3']/p/text()")
                .Select(n => n.InnerText)
                .ToList();

            var carParks = new List<CarPark>();

            foreach (var carParkData in carParkNames.Select((name, index) => new { name, index }))
            {
                var name = ParseName(carParkData.name);
                var numberOfFreeSpaces = ParseFreeSpaces(carParkSpaces[carParkData.index]);
                var (percentFull, usageDirection) = numberOfFreeSpaces == 0
                    ? (100, SpaceUsageDirection.Full)
                    : ParseFillData(carParkFillingData[carParkData.index]);

                var carPark = new CarPark(name, numberOfFreeSpaces, percentFull, usageDirection);
                carParks.Add(carPark);
            }

            return carParks.OrderBy(p => p.Name);
        }

        private static string ParseName(string name)
        {
            int end = name.IndexOf(" car park", StringComparison.Ordinal);
            return name.Substring(0, end);
        }

        private static int ParseFreeSpaces(string carParkSpaceData)
        {
            if (carParkSpaceData == "This car park is full")
            {
                return 0;
            }

            var regex = new Regex(@"(?<spaces>\d*?) spaces");
            var match = regex.Match(carParkSpaceData);
            var spacesValue = match.Groups["spaces"].Value;
            var spaces = int.Parse(spacesValue);
            return spaces;
        }

        private static (int, SpaceUsageDirection) ParseFillData(string fillingData)
        {
            var regex = new Regex(@" \((?<percent>\d*?)\% and (?<direction>.*?)\)");
            var match = regex.Match(fillingData);

            var percentFullValue = match.Groups["percent"].Value;
            var percentFull = int.Parse(percentFullValue);

            var directionValue = match.Groups["direction"].Value;
            var direction = (SpaceUsageDirection) Enum.Parse(typeof(SpaceUsageDirection), directionValue, true);

            return (percentFull, direction);
        }
    }
}

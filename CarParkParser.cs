using System;
using System.Collections.Generic;
using System.Linq;

namespace Parking
{
    internal static class CarParkParser
    {
        public static IEnumerable<CarPark> ParseFromHtml(string html)
        {
            var simplified = html.Replace(" car park", "")
                .Replace("<h2>", "")
                .Replace("</h2>", "")
                .Replace("<p>", "")
                .Replace("</p>", "")
                .Replace("<b>", "")
                .Replace("</b>", "")
                .Replace("<strong>", "")
                .Replace("</strong>", "");

            var lines = simplified.Split(new[] { "a>" }, StringSplitOptions.RemoveEmptyEntries);
            var carParks = new List<CarPark>();
            string carParkName = null;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                if (line.StartsWith("<a"))
                {
                    var start = line.IndexOf("\">") + 2;
                    var end = line.IndexOf("</");
                    carParkName = line.Substring(start, end - start);
                }
                else
                {
                    if (line.Contains("is full"))
                    {
                        var carPark = new CarPark(carParkName, 0, 100, SpaceUsageDirection.Full);
                        carParks.Add(carPark);
                    }
                    else
                    {
                        var start = line.IndexOf(" spaces");
                        var spaces = line.Substring(0, start);

                        start = line.IndexOf("(") + 1;
                        var end = line.IndexOf("%");
                        var percentFullText = line.Substring(start, end - start);

                        start = line.IndexOf(" and ") + 5;
                        end = line.IndexOf(")");
                        var direction = line.Substring(start, end - start);
                        direction = direction.First().ToString().ToUpper() + direction.Substring(1);

                        var numberOfFreeSpaces = int.Parse(spaces);
                        var percentFull = int.Parse(percentFullText);
                        var usageDirection = (SpaceUsageDirection)Enum.Parse(typeof(SpaceUsageDirection), direction);

                        var carPark = new CarPark(carParkName, numberOfFreeSpaces, percentFull, usageDirection);
                        carParks.Add(carPark);
                    }
                }
            }

            return carParks;
        }
    }
}

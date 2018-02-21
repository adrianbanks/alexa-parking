using System;
using System.Collections.Generic;
using System.Linq;

namespace Parking
{
    internal sealed class CarParkParser
    {
        public IEnumerable<CarPark> ParseFromHtml(string html)
        {
            var simplified = html.Replace(" car park", "")
                .Replace("<h2>", "")
                .Replace("</h2>", "")
                .Replace("<p>", "")
                .Replace("</p>", "")
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
                    var start = line.IndexOf(" spaces");
                    var spaces = line.Substring(0, start);

                    start = line.IndexOf("(") + 1;
                    var end = line.IndexOf("%");
                    var percentFull = line.Substring(start, end - start);

                    start = line.IndexOf(" and ") + 5;
                    end = line.IndexOf(")");
                    var direction = line.Substring(start, end - start);
                    direction = direction.First().ToString().ToUpper() + direction.Substring(1);

                    var carPark = new CarPark()
                    {
                        Name = carParkName,
                        NumberOfFreeSpaces = int.Parse(spaces),
                        PercentFull = int.Parse(percentFull),
                        UsageDirection = (SpaceUsageDirection) Enum.Parse(typeof(SpaceUsageDirection), direction)
                    };
                    carParks.Add(carPark);
                }
            }

            return carParks;
        }
    }
}

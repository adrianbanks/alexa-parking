using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Parking
{
    public class Function
    {
        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            context.Logger.LogLine("Default LaunchRequest made: 'Alexa, open parking");
            var spokenResponse = new PlainTextOutputSpeech { Text = GetLeastBusyText() };

            return new SkillResponse
            {
                Version = "1.0",
                Response = new ResponseBody
                {
                    ShouldEndSession = true,
                    OutputSpeech = spokenResponse
                }
            };
        }

        private string GetLeastBusyText()
        {
            var data = GetRawData();
            var carParks = ParseData(data);

            var leastBusy = carParks.OrderByDescending(p => p.NumberOfFreeSpaces).First();

            return $@"{leastBusy.Name} is least busy at {leastBusy.PercentFull} percent full.
It currently has {leastBusy.NumberOfFreeSpaces} spaces free and is {leastBusy.UsageDirection}";
        }

        private string GetRawData()
        {
            var client = new HttpClient();
            var responseTask = client.GetStringAsync("https://www.cambridge.gov.uk/jdi_parking_ajax/complete");
            var responseHtml = responseTask.Result;
            return responseHtml;
        }

        private IEnumerable<CarPark> ParseData(string data)
        {
            var simplified = data.Replace(" car park", "")
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
                        UsageDirection = (SpaceUsageDirection)Enum.Parse(typeof(SpaceUsageDirection), direction)
                    };
                    carParks.Add(carPark);
                }
            }

            return carParks;
        }

        private sealed class CarPark
        {
            public string Name { get; set; }
            public int NumberOfFreeSpaces { get; set; }
            public int PercentFull { get; set; }
            public SpaceUsageDirection UsageDirection { get; set; }
        }

        private enum SpaceUsageDirection
        {
            Emptying,
            Filling,
            Static
        }
    }
}

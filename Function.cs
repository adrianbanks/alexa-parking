using System.Linq;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Parking
{
    public sealed class Function
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
            var html = new RawDataFetcher().GetRawData();
            var carParks = new CarParkParser().ParseFromHtml(html);

            var leastBusy = carParks.OrderByDescending(p => p.NumberOfFreeSpaces).First();

            return $@"{leastBusy.Name} is least busy at {leastBusy.PercentFull} percent full.
It currently has {leastBusy.NumberOfFreeSpaces} spaces free and is {leastBusy.UsageDirection}";
        }
    }
}

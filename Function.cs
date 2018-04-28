using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Parking
{
    public sealed class Function
    {
        public SkillResponse FunctionHandler(SkillRequest request, ILambdaContext context)
        {
            var leastBusyText = HandleRequest(request).Result;
            var spokenResponse = new PlainTextOutputSpeech { Text = leastBusyText };

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

        private async Task<string> HandleRequest(SkillRequest request)
        {
            var progressiveResponseTask = SendProgressiveResponse(request);
            var leastBusyTask = GetLeastBusyCarPark();

            await progressiveResponseTask;
            var leastBusy = await leastBusyTask;

            return $@"{leastBusy.Name} is least busy at {leastBusy.PercentFull}% full.
It currently has {leastBusy.NumberOfFreeSpaces} spaces free and is {leastBusy.UsageDirection.ToString().ToLower()}.";
        }

        private Task<HttpResponseMessage> SendProgressiveResponse(SkillRequest request)
        {
            if (ProgressiveResponse.IsSupported(request))
            {
                var response = new ProgressiveResponse(request);
                var directive = new VoicePlayerSpeakDirective("Getting car park information");
                return response.Send(directive);
            }

            return Task.FromResult((HttpResponseMessage) null);
        }

        private Task<CarPark> GetLeastBusyCarPark()
        {
            var httpClient = new HttpClient();

            return httpClient.GetStringAsync("https://www.cambridge.gov.uk/jdi_parking_ajax/complete")
                .ContinueWith(task =>
                {
                    var carParks = CarParkParser.ParseFromHtml(task.Result);
                    return carParks.OrderByDescending(p => p.NumberOfFreeSpaces).First();
                });
        }
    }
}

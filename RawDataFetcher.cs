using System.Net.Http;

namespace Parking
{
    internal sealed class RawDataFetcher
    {
        private const string DataUrl = "https://www.cambridge.gov.uk/jdi_parking_ajax/complete";

        public string GetRawData()
        {
            using (var client = new HttpClient())
            {
                var responseTask = client.GetStringAsync(DataUrl);
                return responseTask.Result;
            }
        }
    }
}

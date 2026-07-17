using System.Net.Http.Headers;
using System.Text.Json;

public class DistanceApiService : IDistanceApiService {
    private HttpClient httpClient { get; }
    public string MatrixFootWalkingUri => "/v2/matrix/foot-walking";

    public DistanceApiService(HttpClient httpClient) {
        this.httpClient = httpClient;
    }

    public async Task<double?> GetDeliveryTimeInSecondsAsync(Location senderLocation, Location recipientLocation) {
        try {
            var content = new StringContent($"{{\"locations\":[[{senderLocation.Longitude}, {senderLocation.Latitude}],[{recipientLocation.Longitude}, {recipientLocation.Latitude}]]}}", new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await this.httpClient.PostAsync(this.MatrixFootWalkingUri, content);

            var responseData = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<MatrixResponse>(responseData);
            var senderToRecipientData = data?.Durations[0][1];
            return senderToRecipientData;
        } catch (Exception ex) {
            Console.Error.WriteLine($"An error occurred while fetching delivery time: {ex.Message}");
            return null;
        }

    }
}

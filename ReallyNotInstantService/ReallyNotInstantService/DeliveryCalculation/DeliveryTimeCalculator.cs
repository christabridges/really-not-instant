public class DeliveryTimeCalculator : IDeliveryTimeCalculator {
    public IDistanceApiService distanceApiService { get; }

    public DeliveryTimeCalculator(IDistanceApiService apiService) {
        this.distanceApiService = apiService;
    }

    public async Task<DateTime?> Calculate(Location senderLocation, Location recipientLocation, DateTime sentAtUtc) {
        var deliveryTimeInSeconds = await this.distanceApiService.GetDeliveryTimeInSecondsAsync(senderLocation, recipientLocation);

        if (deliveryTimeInSeconds == null) {
            return null;
        }

        return sentAtUtc.AddSeconds(deliveryTimeInSeconds.Value);
    }
}

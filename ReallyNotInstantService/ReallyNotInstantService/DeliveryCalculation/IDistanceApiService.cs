public interface IDistanceApiService {
    Task<double?> GetDeliveryTimeInSecondsAsync(Location senderLocation, Location recipientLocation);
}
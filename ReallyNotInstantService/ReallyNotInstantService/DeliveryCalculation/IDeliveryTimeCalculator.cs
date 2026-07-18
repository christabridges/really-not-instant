public interface IDeliveryTimeCalculator {
    Task<DateTime?> Calculate(Location senderLocation, Location recipientLocation, DateTime sentAtUtc);
}
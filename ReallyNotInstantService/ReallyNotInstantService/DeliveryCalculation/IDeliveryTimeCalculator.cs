public interface IDeliveryTimeCalculator {
    Task<DateTime?> Calculate(string senderLocation, string recipientLocation, DateTime sentAtUtc);
}
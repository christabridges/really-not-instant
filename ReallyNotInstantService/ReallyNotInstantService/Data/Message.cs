namespace ReallyNotInstantService.Data {
    public record Message {
        public required Guid Id { get; init; }
        public required Guid SenderId { get; init; }
        public required Guid RecipientId { get; init; }
        public required DateTime SentAtUtc { get; init; }
        public required string MessageContent { get; init; }
        public required string Status { get; init; }
        public DateTime? PlannedDeliveryTimeUtc { get; init; }
    }

    public static class MessageStatusStrings { 
        public const string Sent = "Sent";
        public const string InTransit = "InTransit";
        public const string WaitingForDelivery = "WaitingForDelivery";
        public const string Delivered = "Delivered";
        public const string Read = "Read";
        public const string Failed = "Failed";
    }
}

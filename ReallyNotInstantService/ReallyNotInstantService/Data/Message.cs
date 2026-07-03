namespace ReallyNotInstantService.Data {
    public record Message {
        public required Guid Id;
        public required Guid SenderId;
        public required Guid RecipientId;
        public required DateTime SentAtUtc;
        public required string MessageContent;
        public required string Status;
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

namespace ReallyNotInstantService.Models {
    public class Message {
        public virtual Guid Id { get; set; }
        public virtual Guid SenderId { get; set; }
        public virtual Guid RecipientId { get; set; }
        public virtual DateTime SentAtUtc { get; set; }
        public virtual string MessageContent { get; set; }
        public virtual string Status { get; set; }
        public virtual DateTime? PlannedDeliveryTimeUtc { get; set; }
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

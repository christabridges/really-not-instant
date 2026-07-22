using FluentMigrator;

namespace ReallyNotInstantService.Migrations {
    [Migration(202607191447)]
    public class AddMessagesTable : Migration {
        public override void Down() {
            Delete.Table("Messages");
        }

        public override void Up() {
            Create.Table("Messages")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("SenderId").AsGuid().NotNullable().Indexed()
                .WithColumn("RecipientId").AsGuid().NotNullable().Indexed()
                .WithColumn("SentAtUtc").AsDateTime().NotNullable()
                .WithColumn("MessageContent").AsString().NotNullable()
                .WithColumn("Status").AsString().NotNullable().Indexed()
                .WithColumn("PlannedDeliveryTimeUtc").AsDateTime().Nullable();
        }
    }
}

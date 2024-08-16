using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class adminapikeyseed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Api",
                columns: new[] { "CustomerId", "Apikey", "Calls", "DateTime", "SubscriptionTier" },
                values: new object[] { new Guid("8233a0ab-78ac-4ee7-916f-0cbb93e85a63"), new Guid("33dcddeb-8f60-41d7-ad33-7bf7b34aaf20"), 0, null, 2 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Api",
                keyColumn: "CustomerId",
                keyValue: new Guid("8233a0ab-78ac-4ee7-916f-0cbb93e85a63"));
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class OnetoOneApi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Api",
                columns: table => new
                {
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Apikey = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionTier = table.Column<int>(type: "integer", nullable: false),
                    Calls = table.Column<int>(type: "integer", nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("UserID", x => x.CustomerId);
                    table.ForeignKey(
                        name: "FK_Api_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "Address", "Email", "FirstName", "LastName", "Password", "Phone", "RolesModelId", "Salt" },
                values: new object[] { new Guid("8233a0ab-78ac-4ee7-916f-0cbb93e85a63"), "here", "admin@example.com", "Admin", "Admin", "7fb1cf92faf20c657c1fee16d6e975eb5c8b61a82cbaaf66a2c9a2c2c19addf1", "yes331", 1, "e1ed2b31" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Api");

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("8233a0ab-78ac-4ee7-916f-0cbb93e85a63"));
        }
    }
}

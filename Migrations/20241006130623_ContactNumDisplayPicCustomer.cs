using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace it15_palit.Migrations
{
    /// <inheritdoc />
    public partial class ContactNumDisplayPicCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Customers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Contact_Number",
                table: "Customers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Display_Picture",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created_at", "Updated_at" },
                values: new object[] { new DateTime(2024, 10, 6, 13, 6, 17, 173, DateTimeKind.Utc).AddTicks(1765), new DateTime(2024, 10, 6, 13, 6, 17, 173, DateTimeKind.Utc).AddTicks(1766) });

            migrationBuilder.UpdateData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created_at", "Updated_at" },
                values: new object[] { new DateTime(2024, 10, 6, 13, 6, 17, 173, DateTimeKind.Utc).AddTicks(1768), new DateTime(2024, 10, 6, 13, 6, 17, 173, DateTimeKind.Utc).AddTicks(1768) });

            migrationBuilder.UpdateData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Created_at", "Updated_at" },
                values: new object[] { new DateTime(2024, 10, 6, 13, 6, 17, 173, DateTimeKind.Utc).AddTicks(1770), new DateTime(2024, 10, 6, 13, 6, 17, 173, DateTimeKind.Utc).AddTicks(1771) });

            migrationBuilder.UpdateData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created_at", "Updated_at" },
                values: new object[] { new DateTime(2024, 10, 6, 13, 6, 17, 173, DateTimeKind.Utc).AddTicks(1772), new DateTime(2024, 10, 6, 13, 6, 17, 173, DateTimeKind.Utc).AddTicks(1773) });

            migrationBuilder.UpdateData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Created_at", "Updated_at" },
                values: new object[] { new DateTime(2024, 10, 6, 13, 6, 17, 173, DateTimeKind.Utc).AddTicks(1774), new DateTime(2024, 10, 6, 13, 6, 17, 173, DateTimeKind.Utc).AddTicks(1775) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created_At", "Updated_At" },
                values: new object[] { new DateTime(2024, 10, 6, 13, 6, 17, 173, DateTimeKind.Utc).AddTicks(1216), new DateTime(2024, 10, 6, 13, 6, 17, 173, DateTimeKind.Utc).AddTicks(1221) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created_At", "Updated_At" },
                values: new object[] { new DateTime(2024, 10, 6, 13, 6, 17, 173, DateTimeKind.Utc).AddTicks(1223), new DateTime(2024, 10, 6, 13, 6, 17, 173, DateTimeKind.Utc).AddTicks(1224) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contact_Number",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Display_Picture",
                table: "Customers");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Customers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.UpdateData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created_at", "Updated_at" },
                values: new object[] { new DateTime(2024, 10, 6, 8, 20, 17, 439, DateTimeKind.Utc).AddTicks(8652), new DateTime(2024, 10, 6, 8, 20, 17, 439, DateTimeKind.Utc).AddTicks(8653) });

            migrationBuilder.UpdateData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created_at", "Updated_at" },
                values: new object[] { new DateTime(2024, 10, 6, 8, 20, 17, 439, DateTimeKind.Utc).AddTicks(8655), new DateTime(2024, 10, 6, 8, 20, 17, 439, DateTimeKind.Utc).AddTicks(8655) });

            migrationBuilder.UpdateData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Created_at", "Updated_at" },
                values: new object[] { new DateTime(2024, 10, 6, 8, 20, 17, 439, DateTimeKind.Utc).AddTicks(8657), new DateTime(2024, 10, 6, 8, 20, 17, 439, DateTimeKind.Utc).AddTicks(8658) });

            migrationBuilder.UpdateData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created_at", "Updated_at" },
                values: new object[] { new DateTime(2024, 10, 6, 8, 20, 17, 439, DateTimeKind.Utc).AddTicks(8659), new DateTime(2024, 10, 6, 8, 20, 17, 439, DateTimeKind.Utc).AddTicks(8659) });

            migrationBuilder.UpdateData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Created_at", "Updated_at" },
                values: new object[] { new DateTime(2024, 10, 6, 8, 20, 17, 439, DateTimeKind.Utc).AddTicks(8661), new DateTime(2024, 10, 6, 8, 20, 17, 439, DateTimeKind.Utc).AddTicks(8661) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created_At", "Updated_At" },
                values: new object[] { new DateTime(2024, 10, 6, 8, 20, 17, 439, DateTimeKind.Utc).AddTicks(8126), new DateTime(2024, 10, 6, 8, 20, 17, 439, DateTimeKind.Utc).AddTicks(8130) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created_At", "Updated_At" },
                values: new object[] { new DateTime(2024, 10, 6, 8, 20, 17, 439, DateTimeKind.Utc).AddTicks(8132), new DateTime(2024, 10, 6, 8, 20, 17, 439, DateTimeKind.Utc).AddTicks(8133) });
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace it15_palit.Migrations
{
    /// <inheritdoc />
    public partial class Updates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created_at", "Updated_at" },
                values: new object[] { new DateTime(2024, 10, 9, 1, 55, 5, 433, DateTimeKind.Utc).AddTicks(8688), new DateTime(2024, 10, 9, 1, 55, 5, 433, DateTimeKind.Utc).AddTicks(8688) });

            migrationBuilder.UpdateData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created_at", "Updated_at" },
                values: new object[] { new DateTime(2024, 10, 9, 1, 55, 5, 433, DateTimeKind.Utc).AddTicks(8690), new DateTime(2024, 10, 9, 1, 55, 5, 433, DateTimeKind.Utc).AddTicks(8690) });

            migrationBuilder.UpdateData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Created_at", "Updated_at" },
                values: new object[] { new DateTime(2024, 10, 9, 1, 55, 5, 433, DateTimeKind.Utc).AddTicks(8692), new DateTime(2024, 10, 9, 1, 55, 5, 433, DateTimeKind.Utc).AddTicks(8692) });

            migrationBuilder.UpdateData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created_at", "Updated_at" },
                values: new object[] { new DateTime(2024, 10, 9, 1, 55, 5, 433, DateTimeKind.Utc).AddTicks(8693), new DateTime(2024, 10, 9, 1, 55, 5, 433, DateTimeKind.Utc).AddTicks(8694) });

            migrationBuilder.UpdateData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Created_at", "Updated_at" },
                values: new object[] { new DateTime(2024, 10, 9, 1, 55, 5, 433, DateTimeKind.Utc).AddTicks(8695), new DateTime(2024, 10, 9, 1, 55, 5, 433, DateTimeKind.Utc).AddTicks(8695) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created_At", "Updated_At" },
                values: new object[] { new DateTime(2024, 10, 9, 1, 55, 5, 433, DateTimeKind.Utc).AddTicks(8382), new DateTime(2024, 10, 9, 1, 55, 5, 433, DateTimeKind.Utc).AddTicks(8386) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created_At", "Updated_At" },
                values: new object[] { new DateTime(2024, 10, 9, 1, 55, 5, 433, DateTimeKind.Utc).AddTicks(8388), new DateTime(2024, 10, 9, 1, 55, 5, 433, DateTimeKind.Utc).AddTicks(8388) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}

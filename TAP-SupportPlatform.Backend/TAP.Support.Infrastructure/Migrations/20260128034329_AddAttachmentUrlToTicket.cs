using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAP.Support.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttachmentUrlToTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "Tickets",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "Tickets");
        }
    }
}

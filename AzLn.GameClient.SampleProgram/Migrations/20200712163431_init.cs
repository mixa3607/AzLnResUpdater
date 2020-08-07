using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AzLn.GameClient.SampleProgram.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Commands",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Guid = table.Column<Guid>(nullable: false),
                    UserId = table.Column<uint>(nullable: false),
                    GameServer = table.Column<short>(nullable: false),
                    CommandType = table.Column<int>(nullable: false),
                    Index = table.Column<ushort>(nullable: false),
                    CommandId = table.Column<ushort>(nullable: false),
                    Bytes = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commands", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Commands");
        }
    }
}

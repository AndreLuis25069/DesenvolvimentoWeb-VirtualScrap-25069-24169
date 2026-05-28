using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualScrap_25069_24169.Migrations
{
    /// <inheritdoc />
    public partial class ForeignKeyParaPostNoComentarioMaisDonoNoPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Posts_PostId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_MyUsers_MyUserId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_MyUserId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Comments_PostId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "MyUserId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PostId",
                table: "Comments");

            migrationBuilder.CreateTable(
                name: "PostComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CommentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AutorFK = table.Column<int>(type: "int", nullable: false),
                    PostFK = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostComments_MyUsers_AutorFK",
                        column: x => x.AutorFK,
                        principalTable: "MyUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostComments_Posts_PostFK",
                        column: x => x.PostFK,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_OwnerFK",
                table: "Posts",
                column: "OwnerFK");

            migrationBuilder.CreateIndex(
                name: "IX_PostComments_AutorFK",
                table: "PostComments",
                column: "AutorFK");

            migrationBuilder.CreateIndex(
                name: "IX_PostComments_PostFK",
                table: "PostComments",
                column: "PostFK");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_MyUsers_OwnerFK",
                table: "Posts",
                column: "OwnerFK",
                principalTable: "MyUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_MyUsers_OwnerFK",
                table: "Posts");

            migrationBuilder.DropTable(
                name: "PostComments");

            migrationBuilder.DropIndex(
                name: "IX_Posts_OwnerFK",
                table: "Posts");

            migrationBuilder.AddColumn<int>(
                name: "MyUserId",
                table: "Posts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PostId",
                table: "Comments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_MyUserId",
                table: "Posts",
                column: "MyUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PostId",
                table: "Comments",
                column: "PostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Posts_PostId",
                table: "Comments",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_MyUsers_MyUserId",
                table: "Posts",
                column: "MyUserId",
                principalTable: "MyUsers",
                principalColumn: "Id");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualScrap_25069_24169.Migrations
{
    /// <inheritdoc />
    public partial class TentativaDeAtivarAlgunsCascades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Posts_PostFK",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_Posts_PostFK",
                table: "PostComments");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Posts_PostFK",
                table: "Likes",
                column: "PostFK",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostComments_Posts_PostFK",
                table: "PostComments",
                column: "PostFK",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Posts_PostFK",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_Posts_PostFK",
                table: "PostComments");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Posts_PostFK",
                table: "Likes",
                column: "PostFK",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PostComments_Posts_PostFK",
                table: "PostComments",
                column: "PostFK",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

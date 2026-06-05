using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualScrap_25069_24169.Migrations
{
    /// <inheritdoc />
    public partial class SetDeleteBehaviorNoAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_MyUsers_AutorFK",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_MyUsers_RecipientFK",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_MyUsers_LikeAutorFK",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Posts_PostFK",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_MyUsers_AutorFK",
                table: "PostComments");

            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_Posts_PostFK",
                table: "PostComments");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_MyUsers_OwnerFK",
                table: "Posts");

            migrationBuilder.AlterColumn<int>(
                name: "AutorFK",
                table: "PostComments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "AutorFK",
                table: "Comments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_MyUsers_AutorFK",
                table: "Comments",
                column: "AutorFK",
                principalTable: "MyUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_MyUsers_RecipientFK",
                table: "Comments",
                column: "RecipientFK",
                principalTable: "MyUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_MyUsers_LikeAutorFK",
                table: "Likes",
                column: "LikeAutorFK",
                principalTable: "MyUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Posts_PostFK",
                table: "Likes",
                column: "PostFK",
                principalTable: "Posts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PostComments_MyUsers_AutorFK",
                table: "PostComments",
                column: "AutorFK",
                principalTable: "MyUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PostComments_Posts_PostFK",
                table: "PostComments",
                column: "PostFK",
                principalTable: "Posts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_MyUsers_OwnerFK",
                table: "Posts",
                column: "OwnerFK",
                principalTable: "MyUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_MyUsers_AutorFK",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_MyUsers_RecipientFK",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_MyUsers_LikeAutorFK",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Posts_PostFK",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_MyUsers_AutorFK",
                table: "PostComments");

            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_Posts_PostFK",
                table: "PostComments");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_MyUsers_OwnerFK",
                table: "Posts");

            migrationBuilder.AlterColumn<int>(
                name: "AutorFK",
                table: "PostComments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AutorFK",
                table: "Comments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_MyUsers_AutorFK",
                table: "Comments",
                column: "AutorFK",
                principalTable: "MyUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_MyUsers_RecipientFK",
                table: "Comments",
                column: "RecipientFK",
                principalTable: "MyUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_MyUsers_LikeAutorFK",
                table: "Likes",
                column: "LikeAutorFK",
                principalTable: "MyUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Posts_PostFK",
                table: "Likes",
                column: "PostFK",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostComments_MyUsers_AutorFK",
                table: "PostComments",
                column: "AutorFK",
                principalTable: "MyUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PostComments_Posts_PostFK",
                table: "PostComments",
                column: "PostFK",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_MyUsers_OwnerFK",
                table: "Posts",
                column: "OwnerFK",
                principalTable: "MyUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

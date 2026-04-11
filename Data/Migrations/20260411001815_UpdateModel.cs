using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalMusicPlayer.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Songs_Album",
                table: "Songs",
                column: "Album");

            migrationBuilder.CreateIndex(
                name: "IX_Songs_AlbumArtPath",
                table: "Songs",
                column: "AlbumArtPath");

            migrationBuilder.CreateIndex(
                name: "IX_Songs_Artist",
                table: "Songs",
                column: "Artist");

            migrationBuilder.CreateIndex(
                name: "IX_PlayHistory_FilePath",
                table: "PlayHistory",
                column: "FilePath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Songs_Album",
                table: "Songs");

            migrationBuilder.DropIndex(
                name: "IX_Songs_AlbumArtPath",
                table: "Songs");

            migrationBuilder.DropIndex(
                name: "IX_Songs_Artist",
                table: "Songs");

            migrationBuilder.DropIndex(
                name: "IX_PlayHistory_FilePath",
                table: "PlayHistory");
        }
    }
}

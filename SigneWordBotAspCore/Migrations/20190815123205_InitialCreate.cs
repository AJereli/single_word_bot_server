using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace SigneWordBotAspCore.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "basket_access_enum",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    title = table.Column<string>(maxLength: 100, nullable: false),
                    description = table.Column<string>(maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_basket_access_enum", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "passwords_basket",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    name = table.Column<string>(maxLength: 512, nullable: false),
                    basket_pass = table.Column<string>(maxLength: 1024, nullable: true),
                    description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_passwords_basket", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    password = table.Column<string>(maxLength: 512, nullable: false),
                    first_name = table.Column<string>(maxLength: 256, nullable: true),
                    second_name = table.Column<string>(maxLength: 256, nullable: true),
                    tg_id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Comment", "Uniq telegram ID of the user"),
                    tg_username = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "credentials",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    unit_password = table.Column<string>(nullable: false),
                    name = table.Column<string>(maxLength: 1024, nullable: false),
                    login = table.Column<string>(maxLength: 1024, nullable: false),
                    basket_pass_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credentials", x => x.id);
                    table.ForeignKey(
                        name: "fk_credentials_basket_pass",
                        column: x => x.basket_pass_id,
                        principalTable: "passwords_basket",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "note",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    title = table.Column<string>(nullable: false),
                    details = table.Column<string>(nullable: true),
                    user_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_note", x => x.id);
                    table.ForeignKey(
                        name: "fk_note_basket_access_enum",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_basket",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<int>(nullable: false),
                    basket_id = table.Column<int>(nullable: false),
                    access_type_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_basket", x => x.id);
                    table.ForeignKey(
                        name: "fk_basket_access_enum",
                        column: x => x.access_type_id,
                        principalTable: "basket_access_enum",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_basket_credentials",
                        column: x => x.basket_id,
                        principalTable: "passwords_basket",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_basket_user",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_credentials_basket_pass_id",
                table: "credentials",
                column: "basket_pass_id");

            migrationBuilder.CreateIndex(
                name: "idx_credentials_login",
                table: "credentials",
                column: "login");

            migrationBuilder.CreateIndex(
                name: "idx_credentials_name",
                table: "credentials",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_note_user_id",
                table: "note",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_passwords_basket_name",
                table: "passwords_basket",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "idx_table",
                table: "user",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_tg_id",
                table: "user",
                column: "tg_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_tg_username",
                table: "user",
                column: "tg_username");

            migrationBuilder.CreateIndex(
                name: "IX_user_basket_access_type_id",
                table: "user_basket",
                column: "access_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_basket_basket_id",
                table: "user_basket",
                column: "basket_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_basket_user_id",
                table: "user_basket",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "credentials");

            migrationBuilder.DropTable(
                name: "note");

            migrationBuilder.DropTable(
                name: "user_basket");

            migrationBuilder.DropTable(
                name: "basket_access_enum");

            migrationBuilder.DropTable(
                name: "passwords_basket");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetVerse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "system_configs",
                columns: table => new
                {
                    key = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    value = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_configs", x => x.key);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_tags",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    tag_name = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    color = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icon_url = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    extra_info = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_tags", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    username = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nickname = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    avatar_url = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bio = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    status = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValue: (sbyte)1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pets",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    breed = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gender = table.Column<sbyte>(type: "tinyint", nullable: true),
                    birthday = table.Column<DateOnly>(type: "date", nullable: true),
                    weight_kg = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    health_status = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    avatar_url = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pettag_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pets", x => x.id);
                    table.ForeignKey(
                        name: "FK_pets_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    reporter_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    target_type = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    target_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    reason_type = table.Column<sbyte>(type: "tinyint", nullable: false),
                    reason_detail = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValue: (sbyte)0),
                    handled_by = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    handled_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reports", x => x.id);
                    table.ForeignKey(
                        name: "FK_reports_users_handled_by",
                        column: x => x.handled_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_reports_users_reporter_id",
                        column: x => x.reporter_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_tag_relations",
                columns: table => new
                {
                    user_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    tag_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    assigned_by = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_tag_relations", x => new { x.user_id, x.tag_id });
                    table.ForeignKey(
                        name: "FK_user_tag_relations_user_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "user_tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_tag_relations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pet_vaccines",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    pet_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    vaccine_name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    vaccinate_date = table.Column<DateOnly>(type: "date", nullable: false),
                    next_date = table.Column<DateOnly>(type: "date", nullable: true),
                    hospital = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    remark = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pet_vaccines", x => x.id);
                    table.ForeignKey(
                        name: "FK_pet_vaccines_pets_pet_id",
                        column: x => x.pet_id,
                        principalTable: "pets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pettags",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    serial_number = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_id = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    pet_id = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    status = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValue: (sbyte)0),
                    battery_level = table.Column<sbyte>(type: "tinyint", nullable: true),
                    last_seen = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    firmware_version = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pettags", x => x.id);
                    table.ForeignKey(
                        name: "FK_pettags_pets_pet_id",
                        column: x => x.pet_id,
                        principalTable: "pets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_pettags_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "posts",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    pet_id = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    media_urls = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    location = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    likes_count = table.Column<uint>(type: "int unsigned", nullable: false, defaultValue: 0u),
                    comments_count = table.Column<uint>(type: "int unsigned", nullable: false, defaultValue: 0u),
                    visibility = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValue: (sbyte)1),
                    status = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValue: (sbyte)1),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_posts", x => x.id);
                    table.ForeignKey(
                        name: "FK_posts_pets_pet_id",
                        column: x => x.pet_id,
                        principalTable: "pets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_posts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "comments",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    post_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    user_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    parent_id = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    likes_count = table.Column<uint>(type: "int unsigned", nullable: false, defaultValue: 0u),
                    status = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValue: (sbyte)1),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comments", x => x.id);
                    table.ForeignKey(
                        name: "FK_comments_comments_parent_id",
                        column: x => x.parent_id,
                        principalTable: "comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_comments_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_comments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "likes",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    target_type = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    target_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    user_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CommentId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    PostId = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_likes", x => x.id);
                    table.ForeignKey(
                        name: "FK_likes_comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "comments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_likes_posts_PostId",
                        column: x => x.PostId,
                        principalTable: "posts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_likes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_comments_parent_id",
                table: "comments",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_comments_post_id",
                table: "comments",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_comments_user_id",
                table: "comments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_likes_CommentId",
                table: "likes",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_likes_PostId",
                table: "likes",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_likes_target_type_target_id",
                table: "likes",
                columns: new[] { "target_type", "target_id" });

            migrationBuilder.CreateIndex(
                name: "IX_likes_target_type_target_id_user_id",
                table: "likes",
                columns: new[] { "target_type", "target_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_likes_user_id",
                table: "likes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_pet_vaccines_pet_id",
                table: "pet_vaccines",
                column: "pet_id");

            migrationBuilder.CreateIndex(
                name: "IX_pets_user_id",
                table: "pets",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_pettags_pet_id",
                table: "pettags",
                column: "pet_id");

            migrationBuilder.CreateIndex(
                name: "IX_pettags_serial_number",
                table: "pettags",
                column: "serial_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pettags_user_id_pet_id",
                table: "pettags",
                columns: new[] { "user_id", "pet_id" });

            migrationBuilder.CreateIndex(
                name: "IX_posts_created_at",
                table: "posts",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_posts_pet_id",
                table: "posts",
                column: "pet_id");

            migrationBuilder.CreateIndex(
                name: "IX_posts_user_id",
                table: "posts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_reports_handled_by",
                table: "reports",
                column: "handled_by");

            migrationBuilder.CreateIndex(
                name: "IX_reports_reporter_id",
                table: "reports",
                column: "reporter_id");

            migrationBuilder.CreateIndex(
                name: "IX_reports_status",
                table: "reports",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_reports_target_type_target_id",
                table: "reports",
                columns: new[] { "target_type", "target_id" });

            migrationBuilder.CreateIndex(
                name: "IX_user_tag_relations_tag_id",
                table: "user_tag_relations",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_tags_tag_name",
                table: "user_tags",
                column: "tag_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_phone",
                table: "users",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_status",
                table: "users",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "likes");

            migrationBuilder.DropTable(
                name: "pet_vaccines");

            migrationBuilder.DropTable(
                name: "pettags");

            migrationBuilder.DropTable(
                name: "reports");

            migrationBuilder.DropTable(
                name: "system_configs");

            migrationBuilder.DropTable(
                name: "user_tag_relations");

            migrationBuilder.DropTable(
                name: "comments");

            migrationBuilder.DropTable(
                name: "user_tags");

            migrationBuilder.DropTable(
                name: "posts");

            migrationBuilder.DropTable(
                name: "pets");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}

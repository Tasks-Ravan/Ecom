using ECommerce.Api.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ECommerce.Api.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261020000000_GoogleOAuthCodes")]
public sealed class GoogleOAuthCodes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        CREATE TABLE "GoogleOAuthCodes" (
            "Id" uuid NOT NULL CONSTRAINT "PK_GoogleOAuthCodes" PRIMARY KEY,
            "UserId" uuid NOT NULL,
            "CodeHash" character varying(64) NOT NULL,
            "ExpiresAt" timestamp with time zone NOT NULL,
            CONSTRAINT "FK_GoogleOAuthCodes_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE);
        CREATE UNIQUE INDEX "IX_GoogleOAuthCodes_CodeHash" ON "GoogleOAuthCodes" ("CodeHash");
        CREATE INDEX "IX_GoogleOAuthCodes_ExpiresAt" ON "GoogleOAuthCodes" ("ExpiresAt");
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DROP TABLE IF EXISTS "GoogleOAuthCodes";
        """);
}

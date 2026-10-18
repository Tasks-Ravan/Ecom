using ECommerce.Api.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ECommerce.Api.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261018010000_Catalog")]
public sealed class Catalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        CREATE TABLE "Categories" (
            "Id" uuid NOT NULL CONSTRAINT "PK_Categories" PRIMARY KEY,
            "Name" character varying(150) NOT NULL,
            "Slug" character varying(180) NOT NULL,
            "ParentCategoryId" uuid,
            "IsActive" boolean NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL,
            "UpdatedAt" timestamp with time zone NOT NULL,
            CONSTRAINT "FK_Categories_Categories_ParentCategoryId" FOREIGN KEY ("ParentCategoryId") REFERENCES "Categories" ("Id") ON DELETE RESTRICT);
        CREATE UNIQUE INDEX "IX_Categories_Slug" ON "Categories" ("Slug");
        CREATE INDEX "IX_Categories_ParentCategoryId" ON "Categories" ("ParentCategoryId");
        CREATE TABLE "Brands" (
            "Id" uuid NOT NULL CONSTRAINT "PK_Brands" PRIMARY KEY,
            "Name" character varying(150) NOT NULL,
            "Slug" character varying(180) NOT NULL,
            "LogoUrl" character varying(2048),
            "IsActive" boolean NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL,
            "UpdatedAt" timestamp with time zone NOT NULL);
        CREATE UNIQUE INDEX "IX_Brands_Name" ON "Brands" ("Name");
        CREATE UNIQUE INDEX "IX_Brands_Slug" ON "Brands" ("Slug");
        CREATE TABLE "Products" (
            "Id" uuid NOT NULL CONSTRAINT "PK_Products" PRIMARY KEY,
            "Sku" character varying(64) NOT NULL,
            "Price" numeric(18,2) NOT NULL,
            "StockQuantity" integer NOT NULL,
            "CategoryId" uuid NOT NULL,
            "BrandId" uuid NOT NULL,
            "IsActive" boolean NOT NULL,
            translations jsonb NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL,
            "UpdatedAt" timestamp with time zone NOT NULL,
            CONSTRAINT "FK_Products_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_Products_Brands_BrandId" FOREIGN KEY ("BrandId") REFERENCES "Brands" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "CK_Products_Price_NonNegative" CHECK ("Price" >= 0),
            CONSTRAINT "CK_Products_StockQuantity_NonNegative" CHECK ("StockQuantity" >= 0));
        CREATE UNIQUE INDEX "IX_Products_Sku" ON "Products" ("Sku");
        CREATE INDEX "IX_Products_BrandId" ON "Products" ("BrandId");
        CREATE INDEX "IX_Products_CategoryId" ON "Products" ("CategoryId");
        CREATE INDEX "IX_Products_CategoryId_BrandId_Price_IsActive" ON "Products" ("CategoryId", "BrandId", "Price", "IsActive");
        CREATE INDEX "IX_Products_Translations" ON "Products" USING gin (translations);
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DROP TABLE IF EXISTS "Products";
        DROP TABLE IF EXISTS "Brands";
        DROP TABLE IF EXISTS "Categories";
        """);
}

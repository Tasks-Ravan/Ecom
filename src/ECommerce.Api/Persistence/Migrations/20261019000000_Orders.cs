using ECommerce.Api.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ECommerce.Api.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261019000000_Orders")]
public sealed class Orders : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        CREATE TABLE "Orders" (
            "Id" uuid NOT NULL CONSTRAINT "PK_Orders" PRIMARY KEY,
            "OrderNumber" character varying(40) NOT NULL,
            "UserId" uuid NOT NULL,
            "Status" character varying(32) NOT NULL,
            "PaymentProvider" character varying(24) NOT NULL,
            "PaymentReference" character varying(64) NOT NULL,
            "Currency" character varying(3) NOT NULL,
            "TotalAmount" numeric(18,2) NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL,
            "PaidAt" timestamp with time zone,
            CONSTRAINT "FK_Orders_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT);
        CREATE UNIQUE INDEX "IX_Orders_OrderNumber" ON "Orders" ("OrderNumber");
        CREATE INDEX "IX_Orders_UserId_CreatedAt" ON "Orders" ("UserId", "CreatedAt");
        CREATE TABLE "OrderItems" (
            "Id" uuid NOT NULL CONSTRAINT "PK_OrderItems" PRIMARY KEY,
            "OrderId" uuid NOT NULL,
            "ProductId" uuid NOT NULL,
            "Sku" character varying(64) NOT NULL,
            "ProductName" character varying(200) NOT NULL,
            "UnitPrice" numeric(18,2) NOT NULL,
            "Quantity" integer NOT NULL,
            "LineTotal" numeric(18,2) NOT NULL,
            CONSTRAINT "CK_OrderItems_Quantity_Positive" CHECK ("Quantity" > 0),
            CONSTRAINT "CK_OrderItems_UnitPrice_NonNegative" CHECK ("UnitPrice" >= 0),
            CONSTRAINT "FK_OrderItems_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE CASCADE);
        CREATE UNIQUE INDEX "IX_OrderItems_OrderId_ProductId" ON "OrderItems" ("OrderId", "ProductId");
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DROP TABLE IF EXISTS "OrderItems";
        DROP TABLE IF EXISTS "Orders";
        """);
}

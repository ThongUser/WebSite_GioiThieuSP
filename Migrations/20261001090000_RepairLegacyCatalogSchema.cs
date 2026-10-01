using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebSite_GioiThieuSP.Data;

#nullable disable

namespace WebSite_GioiThieuSP.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261001090000_RepairLegacyCatalogSchema")]
    public partial class RepairLegacyCatalogSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Categories",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "CategoryName",
                table: "Categories",
                newName: "Name");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql("UPDATE [Categories] SET [IsActive] = CASE WHEN LTRIM(RTRIM([Status])) = N'Active' THEN 1 ELSE 0 END");
            migrationBuilder.DropColumn(name: "Status", table: "Categories");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Products",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ProductName",
                table: "Products",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Image",
                table: "Products",
                newName: "ImageUrl");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsActive", table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Products",
                type: "nvarchar(250)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Products",
                newName: "Image");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Products",
                newName: "ProductName");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Products",
                newName: "ProductId");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Categories",
                type: "nvarchar(100)",
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.Sql("UPDATE [Categories] SET [Status] = CASE WHEN [IsActive] = 1 THEN N'Active' ELSE N'Inactive' END");

            migrationBuilder.DropColumn(name: "IsActive", table: "Categories");
            migrationBuilder.DropColumn(name: "DisplayOrder", table: "Categories");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Categories",
                newName: "CategoryName");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Categories",
                newName: "CategoryId");
        }
    }
}
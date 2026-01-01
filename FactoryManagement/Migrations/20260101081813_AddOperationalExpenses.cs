using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FactoryManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalExpenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    SettingId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompanyName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CurrencySymbol = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsMenuPinned = table.Column<bool>(type: "INTEGER", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.SettingId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseCategories",
                columns: table => new
                {
                    ExpenseCategoryId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategoryName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IconName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ColorCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    IsSystemCategory = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseCategories", x => x.ExpenseCategoryId);
                    table.ForeignKey(
                        name: "FK_ExpenseCategories_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ItemName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CurrentStock = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    ModifiedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_Items_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Items_Users_ModifiedByUserId",
                        column: x => x.ModifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Parties",
                columns: table => new
                {
                    PartyId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    MobileNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Place = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PartyType = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    ModifiedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parties", x => x.PartyId);
                    table.ForeignKey(
                        name: "FK_Parties_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Parties_Users_ModifiedByUserId",
                        column: x => x.ModifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Workers",
                columns: table => new
                {
                    WorkerId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    MobileNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DailyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAdvance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalWagesPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    ModifiedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    JoiningDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LeavingDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workers", x => x.WorkerId);
                    table.ForeignKey(
                        name: "FK_Workers_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Workers_Users_ModifiedByUserId",
                        column: x => x.ModifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OperationalExpenses",
                columns: table => new
                {
                    OperationalExpenseId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExpenseCategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VendorName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SpentBy = table.Column<int>(type: "INTEGER", nullable: true),
                    PaymentMode = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemId = table.Column<int>(type: "INTEGER", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsApproved = table.Column<bool>(type: "INTEGER", nullable: false),
                    ApprovedBy = table.Column<int>(type: "INTEGER", nullable: true),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TotalAmountWithTax = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    EnteredBy = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AttachmentPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationalExpenses", x => x.OperationalExpenseId);
                    table.ForeignKey(
                        name: "FK_OperationalExpenses_ExpenseCategories_ExpenseCategoryId",
                        column: x => x.ExpenseCategoryId,
                        principalTable: "ExpenseCategories",
                        principalColumn: "ExpenseCategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperationalExpenses_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OperationalExpenses_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OperationalExpenses_Users_EnteredBy",
                        column: x => x.EnteredBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperationalExpenses_Users_SpentBy",
                        column: x => x.SpentBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LoanAccounts",
                columns: table => new
                {
                    LoanAccountId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartyId = table.Column<int>(type: "INTEGER", nullable: true),
                    PartyName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    LoanType = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OutstandingPrincipal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OutstandingInterest = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalOutstanding = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanAccounts", x => x.LoanAccountId);
                    table.ForeignKey(
                        name: "FK_LoanAccounts_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "PartyId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LoanAccounts_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ItemId = table.Column<int>(type: "INTEGER", nullable: true),
                    ItemName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PartyId = table.Column<int>(type: "INTEGER", nullable: true),
                    PartyName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TransactionType = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    PricePerUnit = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    PaymentMode = table.Column<int>(type: "INTEGER", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EnteredBy = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    InputItemId = table.Column<int>(type: "INTEGER", nullable: true),
                    InputQuantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    ConversionRate = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_Transactions_Items_InputItemId",
                        column: x => x.InputItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_Transactions_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Transactions_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "PartyId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Transactions_Users_EnteredBy",
                        column: x => x.EnteredBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WageTransactions",
                columns: table => new
                {
                    WageTransactionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkerId = table.Column<int>(type: "INTEGER", nullable: false),
                    TransactionType = table.Column<int>(type: "INTEGER", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DaysWorked = table.Column<decimal>(type: "TEXT", nullable: true),
                    HoursWorked = table.Column<decimal>(type: "TEXT", nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OvertimeHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    OvertimeRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OvertimeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AdvanceAdjusted = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Deductions = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMode = table.Column<int>(type: "INTEGER", nullable: false),
                    EnteredBy = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WageTransactions", x => x.WageTransactionId);
                    table.ForeignKey(
                        name: "FK_WageTransactions_Users_EnteredBy",
                        column: x => x.EnteredBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WageTransactions_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "WorkerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialTransactions",
                columns: table => new
                {
                    FinancialTransactionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartyId = table.Column<int>(type: "INTEGER", nullable: true),
                    PartyName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TransactionType = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMode = table.Column<int>(type: "INTEGER", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    InterestAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LinkedLoanAccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    EnteredBy = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactions", x => x.FinancialTransactionId);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_LoanAccounts_LinkedLoanAccountId",
                        column: x => x.LinkedLoanAccountId,
                        principalTable: "LoanAccounts",
                        principalColumn: "LoanAccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "PartyId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_Users_EnteredBy",
                        column: x => x.EnteredBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "SettingId", "Address", "CompanyName", "CurrencySymbol", "IsMenuPinned", "ModifiedDate" },
                values: new object[] { 1, "123 Industrial Area", "Factory Management System", "₹", true, null });

            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "ItemId", "CreatedByUserId", "CreatedDate", "CurrentStock", "ItemName", "ModifiedByUserId", "ModifiedDate", "Unit" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7794), 1000m, "Rice", null, null, "Kg" },
                    { 2, null, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7799), 500m, "Husk", null, null, "Kg" },
                    { 3, null, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7800), 2000m, "Paddy", null, null, "Kg" },
                    { 4, null, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7802), 300m, "Broken Rice", null, null, "Kg" },
                    { 5, null, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7803), 150m, "Bran", null, null, "Kg" }
                });

            migrationBuilder.InsertData(
                table: "Parties",
                columns: new[] { "PartyId", "CreatedByUserId", "CreatedDate", "MobileNumber", "ModifiedByUserId", "ModifiedDate", "Name", "PartyType", "Place" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7827), "9876543210", null, null, "ABC Traders", 2, "Mumbai" },
                    { 2, null, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7830), "9876543211", null, null, "XYZ Suppliers", 1, "Delhi" },
                    { 3, null, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7831), "9876543212", null, null, "PQR Distributors", 0, "Bangalore" },
                    { 4, null, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7833), "9876543213", null, null, "LMN Enterprises", 2, "Chennai" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedDate", "IsActive", "ModifiedDate", "Role", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7642), true, null, "Administrator", "Admin" },
                    { 2, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7661), true, null, "Manager", "Manager" },
                    { 3, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7663), true, null, "Operator", "Operator" }
                });

            migrationBuilder.InsertData(
                table: "ExpenseCategories",
                columns: new[] { "ExpenseCategoryId", "CategoryName", "ColorCode", "CreatedBy", "CreatedDate", "Description", "DisplayOrder", "IconName", "IsActive", "IsSystemCategory", "ModifiedDate" },
                values: new object[,]
                {
                    { 1, "Cab Charges", null, 1, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7874), "Transportation and cab expenses", 1, "CarSide", true, true, null },
                    { 2, "Transportation Fees", null, 1, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7878), "General transportation costs", 2, "Truck", true, true, null },
                    { 3, "Freight Charges", null, 1, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7880), "Shipping and freight costs", 3, "ShippingBox", true, true, null },
                    { 4, "Electricity", null, 1, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7882), "Electricity bills and charges", 4, "Lightbulb", true, true, null },
                    { 5, "Water", null, 1, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7885), "Water bills and charges", 5, "Water", true, true, null },
                    { 6, "Internet & Phone", null, 1, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7887), "Communication expenses", 6, "Wifi", true, true, null },
                    { 7, "Machinery Purchase", null, 1, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7889), "New machinery and equipment purchases", 7, "Cog", true, true, null },
                    { 8, "Machinery Repair", null, 1, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7891), "Machine repair and servicing costs", 8, "Wrench", true, true, null },
                    { 9, "Machinery Maintenance", null, 1, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7893), "Regular maintenance costs", 9, "Tools", true, true, null },
                    { 10, "Fuel", null, 1, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7895), "Fuel and petroleum expenses", 10, "Gas", true, true, null },
                    { 11, "Generator Diesel", null, 1, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7897), "Diesel for generators", 11, "Power", true, true, null },
                    { 12, "Rent", null, 1, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7898), "Facility and equipment rent", 12, "Home", true, true, null },
                    { 13, "Insurance", null, 1, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7900), "Insurance premiums", 13, "Shield", true, true, null },
                    { 14, "Stationery", null, 1, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7902), "Office supplies and stationery", 14, "Paperclip", true, true, null },
                    { 15, "Printing", null, 1, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7904), "Printing and documentation costs", 15, "Printer", true, true, null },
                    { 16, "Legal Fees", null, 1, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7906), "Legal and compliance costs", 16, "Gavel", true, true, null },
                    { 17, "Accounting Fees", null, 1, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7908), "Accounting and auditing fees", 17, "Calculator", true, true, null },
                    { 18, "Building Maintenance", null, 1, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7910), "Building and facility maintenance", 18, "Factory", true, true, null },
                    { 19, "Repairs", null, 1, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7912), "General repair expenses", 19, "Hammer", true, true, null },
                    { 20, "Miscellaneous", null, 1, new DateTime(2026, 1, 1, 13, 48, 11, 771, DateTimeKind.Local).AddTicks(7914), "Other miscellaneous expenses", 20, "DotsHorizontal", true, true, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_CategoryName",
                table: "ExpenseCategories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_CreatedBy",
                table: "ExpenseCategories",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_DisplayOrder",
                table: "ExpenseCategories",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_IsActive",
                table: "ExpenseCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_EnteredBy",
                table: "FinancialTransactions",
                column: "EnteredBy");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_LinkedLoanAccountId",
                table: "FinancialTransactions",
                column: "LinkedLoanAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_PartyId",
                table: "FinancialTransactions",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_TransactionDate",
                table: "FinancialTransactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_TransactionType",
                table: "FinancialTransactions",
                column: "TransactionType");

            migrationBuilder.CreateIndex(
                name: "IX_Items_CreatedByUserId",
                table: "Items",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemName",
                table: "Items",
                column: "ItemName");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ModifiedByUserId",
                table: "Items",
                column: "ModifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanAccounts_CreatedBy",
                table: "LoanAccounts",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LoanAccounts_DueDate",
                table: "LoanAccounts",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_LoanAccounts_LoanType",
                table: "LoanAccounts",
                column: "LoanType");

            migrationBuilder.CreateIndex(
                name: "IX_LoanAccounts_PartyId",
                table: "LoanAccounts",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanAccounts_Status",
                table: "LoanAccounts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalExpenses_ApprovedBy",
                table: "OperationalExpenses",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalExpenses_EnteredBy",
                table: "OperationalExpenses",
                column: "EnteredBy");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalExpenses_ExpenseCategoryId",
                table: "OperationalExpenses",
                column: "ExpenseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalExpenses_ExpenseDate",
                table: "OperationalExpenses",
                column: "ExpenseDate");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalExpenses_IsApproved",
                table: "OperationalExpenses",
                column: "IsApproved");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalExpenses_ItemId",
                table: "OperationalExpenses",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalExpenses_SpentBy",
                table: "OperationalExpenses",
                column: "SpentBy");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_CreatedByUserId",
                table: "Parties",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_ModifiedByUserId",
                table: "Parties",
                column: "ModifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_Name",
                table: "Parties",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_PartyType",
                table: "Parties",
                column: "PartyType");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_EnteredBy",
                table: "Transactions",
                column: "EnteredBy");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_InputItemId",
                table: "Transactions",
                column: "InputItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ItemId",
                table: "Transactions",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PartyId",
                table: "Transactions",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionDate",
                table: "Transactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionType",
                table: "Transactions",
                column: "TransactionType");

            migrationBuilder.CreateIndex(
                name: "IX_WageTransactions_EnteredBy",
                table: "WageTransactions",
                column: "EnteredBy");

            migrationBuilder.CreateIndex(
                name: "IX_WageTransactions_TransactionDate",
                table: "WageTransactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_WageTransactions_TransactionType",
                table: "WageTransactions",
                column: "TransactionType");

            migrationBuilder.CreateIndex(
                name: "IX_WageTransactions_WorkerId",
                table: "WageTransactions",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_Workers_CreatedByUserId",
                table: "Workers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Workers_ModifiedByUserId",
                table: "Workers",
                column: "ModifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Workers_Name",
                table: "Workers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Workers_Status",
                table: "Workers",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "FinancialTransactions");

            migrationBuilder.DropTable(
                name: "OperationalExpenses");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "WageTransactions");

            migrationBuilder.DropTable(
                name: "LoanAccounts");

            migrationBuilder.DropTable(
                name: "ExpenseCategories");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Workers");

            migrationBuilder.DropTable(
                name: "Parties");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

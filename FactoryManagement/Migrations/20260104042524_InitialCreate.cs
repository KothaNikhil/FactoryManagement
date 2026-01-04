using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FactoryManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "CashBalances",
                columns: table => new
                {
                    CashBalanceId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpectedClosingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualCashCounted = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Discrepancy = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsReconciled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReconciledBy = table.Column<int>(type: "INTEGER", nullable: true),
                    ReconciledDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    DiscrepancyReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TotalCashIn = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCashOut = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashBalances", x => x.CashBalanceId);
                    table.ForeignKey(
                        name: "FK_CashBalances_Users_ReconciledBy",
                        column: x => x.ReconciledBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ExpenseCategories",
                columns: table => new
                {
                    ExpenseCategoryId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategoryName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
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
                    SpentBy = table.Column<int>(type: "INTEGER", nullable: true),
                    PaymentMode = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsApproved = table.Column<bool>(type: "INTEGER", nullable: false),
                    ApprovedBy = table.Column<int>(type: "INTEGER", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "ProcessingOutputItems",
                columns: table => new
                {
                    ProcessingOutputId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TransactionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessingOutputItems", x => x.ProcessingOutputId);
                    table.ForeignKey(
                        name: "FK_ProcessingOutputItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcessingOutputItems_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Cascade);
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
                    { 1, null, new DateTime(2026, 1, 4, 9, 55, 24, 61, DateTimeKind.Local).AddTicks(2052), 1000m, "Rice", null, null, "Kg" },
                    { 2, null, new DateTime(2026, 1, 4, 9, 55, 24, 61, DateTimeKind.Local).AddTicks(2058), 500m, "Husk", null, null, "Kg" },
                    { 3, null, new DateTime(2026, 1, 4, 9, 55, 24, 61, DateTimeKind.Local).AddTicks(2060), 2000m, "Paddy", null, null, "Kg" },
                    { 4, null, new DateTime(2026, 1, 4, 9, 55, 24, 61, DateTimeKind.Local).AddTicks(2061), 300m, "Broken Rice", null, null, "Kg" },
                    { 5, null, new DateTime(2026, 1, 4, 9, 55, 24, 61, DateTimeKind.Local).AddTicks(2062), 150m, "Bran", null, null, "Kg" }
                });

            migrationBuilder.InsertData(
                table: "Parties",
                columns: new[] { "PartyId", "CreatedByUserId", "CreatedDate", "MobileNumber", "ModifiedByUserId", "ModifiedDate", "Name", "PartyType", "Place" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2026, 1, 4, 9, 55, 24, 61, DateTimeKind.Local).AddTicks(2083), "9876543210", null, null, "ABC Traders", 2, "Mumbai" },
                    { 2, null, new DateTime(2026, 1, 4, 9, 55, 24, 61, DateTimeKind.Local).AddTicks(2087), "9876543211", null, null, "XYZ Suppliers", 1, "Delhi" },
                    { 3, null, new DateTime(2026, 1, 4, 9, 55, 24, 61, DateTimeKind.Local).AddTicks(2089), "9876543212", null, null, "PQR Distributors", 0, "Bangalore" },
                    { 4, null, new DateTime(2026, 1, 4, 9, 55, 24, 61, DateTimeKind.Local).AddTicks(2090), "9876543213", null, null, "LMN Enterprises", 2, "Chennai" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedDate", "IsActive", "ModifiedDate", "PasswordHash", "Role", "Username" },
                values: new object[] { 1, new DateTime(2026, 1, 4, 9, 55, 24, 61, DateTimeKind.Local).AddTicks(1899), true, null, null, "Administrator", "Admin" });

            migrationBuilder.InsertData(
                table: "ExpenseCategories",
                columns: new[] { "ExpenseCategoryId", "CategoryName", "CreatedBy", "CreatedDate", "Description", "IsDeleted", "ModifiedDate" },
                values: new object[,]
                {
                    { 1, "Cab Charges", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Transportation and cab expenses", false, null },
                    { 2, "Transportation Fees", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General transportation costs", false, null },
                    { 3, "Freight Charges", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shipping and freight costs", false, null },
                    { 4, "Electricity", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Electricity bills and charges", false, null },
                    { 5, "Water", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Water bills and charges", false, null },
                    { 6, "Internet & Phone", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Communication expenses", false, null },
                    { 7, "Machinery Purchase", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "New machinery and equipment purchases", false, null },
                    { 8, "Machinery Repair", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Machine repair and servicing costs", false, null },
                    { 9, "Machinery Maintenance", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Regular maintenance costs", false, null },
                    { 10, "Fuel", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Fuel and petroleum expenses", false, null },
                    { 11, "Generator Diesel", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Diesel for generators", false, null },
                    { 12, "Rent", 1, new DateTime(2026, 1, 4, 9, 55, 24, 61, DateTimeKind.Local).AddTicks(2156), "Facility and equipment rent", false, null },
                    { 13, "Insurance", 1, new DateTime(2026, 1, 4, 9, 55, 24, 61, DateTimeKind.Local).AddTicks(2158), "Insurance premiums", false, null },
                    { 14, "Stationery", 1, new DateTime(2026, 1, 4, 9, 55, 24, 61, DateTimeKind.Local).AddTicks(2159), "Office supplies and stationery", false, null },
                    { 15, "Printing", 1, new DateTime(2026, 1, 4, 9, 55, 24, 61, DateTimeKind.Local).AddTicks(2161), "Printing and documentation costs", false, null },
                    { 16, "Legal Fees", 1, new DateTime(2026, 1, 4, 9, 55, 24, 61, DateTimeKind.Local).AddTicks(2162), "Legal and compliance costs", false, null },
                    { 17, "Accounting Fees", 1, new DateTime(2026, 1, 4, 9, 55, 24, 61, DateTimeKind.Local).AddTicks(2164), "Accounting and auditing fees", false, null },
                    { 18, "Building Maintenance", 1, new DateTime(2026, 1, 4, 9, 55, 24, 61, DateTimeKind.Local).AddTicks(2165), "Building and facility maintenance", false, null },
                    { 19, "Repairs", 1, new DateTime(2026, 1, 4, 9, 55, 24, 61, DateTimeKind.Local).AddTicks(2167), "General repair expenses", false, null },
                    { 20, "Miscellaneous", 1, new DateTime(2026, 1, 4, 9, 55, 24, 61, DateTimeKind.Local).AddTicks(2168), "Other miscellaneous expenses", false, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CashBalances_Date",
                table: "CashBalances",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashBalances_IsReconciled",
                table: "CashBalances",
                column: "IsReconciled");

            migrationBuilder.CreateIndex(
                name: "IX_CashBalances_ReconciledBy",
                table: "CashBalances",
                column: "ReconciledBy");

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
                name: "IX_ExpenseCategories_IsDeleted",
                table: "ExpenseCategories",
                column: "IsDeleted");

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
                name: "IX_ProcessingOutputItems_ItemId",
                table: "ProcessingOutputItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingOutputItems_TransactionId",
                table: "ProcessingOutputItems",
                column: "TransactionId");

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
                name: "CashBalances");

            migrationBuilder.DropTable(
                name: "FinancialTransactions");

            migrationBuilder.DropTable(
                name: "OperationalExpenses");

            migrationBuilder.DropTable(
                name: "ProcessingOutputItems");

            migrationBuilder.DropTable(
                name: "WageTransactions");

            migrationBuilder.DropTable(
                name: "LoanAccounts");

            migrationBuilder.DropTable(
                name: "ExpenseCategories");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Workers");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Parties");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

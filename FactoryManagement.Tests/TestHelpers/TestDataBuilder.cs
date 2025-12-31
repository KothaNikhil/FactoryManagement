using FactoryManagement.Models;
using System;
using System.Collections.Generic;

namespace FactoryManagement.Tests.TestHelpers
{
    /// <summary>
    /// Builder class for creating test data with fluent interface
    /// </summary>
    public class TestDataBuilder
    {
        public static ItemBuilder AnItem() => new ItemBuilder();
        public static PartyBuilder AParty() => new PartyBuilder();
        public static TransactionBuilder ATransaction() => new TransactionBuilder();
        public static WorkerBuilder AWorker() => new WorkerBuilder();
        public static LoanAccountBuilder ALoanAccount() => new LoanAccountBuilder();
        public static UserBuilder AUser() => new UserBuilder();
    }

    public class ItemBuilder
    {
        private readonly Item _item = new Item
        {
            ItemName = "Default Item",
            CurrentStock = 100,
            Unit = "kg"
        };

        public ItemBuilder WithId(int id)
        {
            _item.ItemId = id;
            return this;
        }

        public ItemBuilder WithName(string name)
        {
            _item.ItemName = name;
            return this;
        }

        public ItemBuilder WithStock(decimal stock)
        {
            _item.CurrentStock = stock;
            return this;
        }

        public ItemBuilder WithUnit(string unit)
        {
            _item.Unit = unit;
            return this;
        }

        public Item Build() => _item;
    }

    public class PartyBuilder
    {
        private readonly Party _party = new Party
        {
            Name = "Default Party",
            MobileNumber = "1234567890",
            PartyType = PartyType.Both,
            Place = "Default Place"
        };

        public PartyBuilder WithId(int id)
        {
            _party.PartyId = id;
            return this;
        }

        public PartyBuilder WithName(string name)
        {
            _party.Name = name;
            return this;
        }

        public PartyBuilder WithMobileNumber(string mobileNumber)
        {
            _party.MobileNumber = mobileNumber;
            return this;
        }

        public PartyBuilder WithPartyType(PartyType partyType)
        {
            _party.PartyType = partyType;
            return this;
        }

        public PartyBuilder WithPlace(string place)
        {
            _party.Place = place;
            return this;
        }

        public Party Build() => _party;
    }

    public class TransactionBuilder
    {
        private readonly Transaction _transaction = new Transaction
        {
            ItemId = 1,
            TransactionType = TransactionType.Buy,
            Quantity = 10,
            PricePerUnit = 100,
            TotalAmount = 1000,
            TransactionDate = DateTime.Now,
            EnteredBy = 1
        };

        public TransactionBuilder WithId(int id)
        {
            _transaction.TransactionId = id;
            return this;
        }

        public TransactionBuilder WithItemId(int itemId)
        {
            _transaction.ItemId = itemId;
            return this;
        }

        public TransactionBuilder WithPartyId(int? partyId)
        {
            _transaction.PartyId = partyId;
            return this;
        }

        public TransactionBuilder WithPartyName(string partyName)
        {
            _transaction.PartyName = partyName;
            return this;
        }

        public TransactionBuilder WithType(TransactionType type)
        {
            _transaction.TransactionType = type;
            return this;
        }

        public TransactionBuilder WithQuantity(decimal quantity)
        {
            _transaction.Quantity = quantity;
            _transaction.TotalAmount = quantity * _transaction.PricePerUnit;
            return this;
        }

        public TransactionBuilder WithPricePerUnit(decimal price)
        {
            _transaction.PricePerUnit = price;
            _transaction.TotalAmount = _transaction.Quantity * price;
            return this;
        }

        public TransactionBuilder WithDate(DateTime date)
        {
            _transaction.TransactionDate = date;
            return this;
        }

        public TransactionBuilder WithNotes(string notes)
        {
            _transaction.Notes = notes;
            return this;
        }

        public TransactionBuilder WithInputItem(int inputItemId, decimal inputQuantity)
        {
            _transaction.InputItemId = inputItemId;
            _transaction.InputQuantity = inputQuantity;
            return this;
        }

        public TransactionBuilder WithConversionRate(decimal conversionRate)
        {
            _transaction.ConversionRate = conversionRate;
            return this;
        }

        public Transaction Build() => _transaction;
    }

    public class WorkerBuilder
    {
        private readonly Worker _worker = new Worker
        {
            Name = "Default Worker",
            MobileNumber = "9876543210",
            Address = "Worker Address",
            Status = WorkerStatus.Active,
            Rate = 500,
            DailyRate = 500
        };

        public WorkerBuilder WithId(int id)
        {
            _worker.WorkerId = id;
            return this;
        }

        public WorkerBuilder WithName(string name)
        {
            _worker.Name = name;
            return this;
        }

        public WorkerBuilder WithMobileNumber(string mobileNumber)
        {
            _worker.MobileNumber = mobileNumber;
            return this;
        }

        public WorkerBuilder WithRate(decimal rate)
        {
            _worker.Rate = rate;
            return this;
        }

        public WorkerBuilder WithDailyRate(decimal rate)
        {
            _worker.DailyRate = rate;
            return this;
        }

        public WorkerBuilder WithStatus(WorkerStatus status)
        {
            _worker.Status = status;
            return this;
        }

        public WorkerBuilder WithJoiningDate(DateTime date)
        {
            _worker.JoiningDate = date;
            return this;
        }

        public Worker Build() => _worker;
    }

    public class LoanAccountBuilder
    {
        private readonly LoanAccount _loanAccount = new LoanAccount
        {
            PartyId = 1,
            LoanType = LoanType.Given,
            OriginalAmount = 10000,
            InterestRate = 5.0m,
            StartDate = DateTime.Now,
            OutstandingPrincipal = 10000,
            OutstandingInterest = 0,
            TotalOutstanding = 10000,
            Status = LoanStatus.Active,
            CreatedBy = 1
        };

        public LoanAccountBuilder WithId(int id)
        {
            _loanAccount.LoanAccountId = id;
            return this;
        }

        public LoanAccountBuilder WithPartyId(int partyId)
        {
            _loanAccount.PartyId = partyId;
            return this;
        }

        public LoanAccountBuilder WithType(LoanType type)
        {
            _loanAccount.LoanType = type;
            return this;
        }

        public LoanAccountBuilder WithOriginalAmount(decimal amount)
        {
            _loanAccount.OriginalAmount = amount;
            _loanAccount.OutstandingPrincipal = amount;
            _loanAccount.TotalOutstanding = amount;
            return this;
        }

        public LoanAccountBuilder WithInterestRate(decimal rate)
        {
            _loanAccount.InterestRate = rate;
            return this;
        }

        public LoanAccountBuilder WithOutstandingPrincipal(decimal amount)
        {
            _loanAccount.OutstandingPrincipal = amount;
            _loanAccount.TotalOutstanding = amount + _loanAccount.OutstandingInterest;
            return this;
        }

        public LoanAccountBuilder WithStatus(LoanStatus status)
        {
            _loanAccount.Status = status;
            return this;
        }

        public LoanAccount Build() => _loanAccount;
    }

    public class UserBuilder
    {
        private readonly User _user = new User
        {
            Username = "testuser",
            Role = "Admin",
            IsActive = true
        };

        public UserBuilder WithId(int id)
        {
            _user.UserId = id;
            return this;
        }

        public UserBuilder WithUsername(string username)
        {
            _user.Username = username;
            return this;
        }

        public UserBuilder WithRole(string role)
        {
            _user.Role = role;
            return this;
        }

        public UserBuilder WithIsActive(bool isActive)
        {
            _user.IsActive = isActive;
            return this;
        }

        public User Build() => _user;
    }

    /// <summary>
    /// Static class for creating common test data collections
    /// </summary>
    public static class TestDataSets
    {
        public static List<Item> GetSampleItems() => new()
        {
            TestDataBuilder.AnItem().WithId(1).WithName("Item A").WithStock(100).Build(),
            TestDataBuilder.AnItem().WithId(2).WithName("Item B").WithStock(50).Build(),
            TestDataBuilder.AnItem().WithId(3).WithName("Item C").WithStock(75).Build()
        };

        public static List<Party> GetSampleParties() => new()
        {
            TestDataBuilder.AParty().WithId(1).WithName("Party A").WithPartyType(PartyType.Buyer).Build(),
            TestDataBuilder.AParty().WithId(2).WithName("Party B").WithPartyType(PartyType.Seller).Build(),
            TestDataBuilder.AParty().WithId(3).WithName("Party C").WithPartyType(PartyType.Both).Build()
        };

        public static List<Worker> GetSampleWorkers() => new()
        {
            TestDataBuilder.AWorker().WithId(1).WithName("Worker 1").WithRate(500).Build(),
            TestDataBuilder.AWorker().WithId(2).WithName("Worker 2").WithRate(600).Build(),
            TestDataBuilder.AWorker().WithId(3).WithName("Worker 3").WithRate(450).Build()
        };
    }
}

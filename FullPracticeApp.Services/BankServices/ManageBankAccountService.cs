using FullPracticeApp.Contracts.Dtos;
using FullPracticeApp.Contracts.Interfaces;
using FullPracticeApp.Domain.Entities;
using FullPracticeApp.Domain.Enum;
using FullPracticeApp.Infrastructure;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullPracticeApp.Services.BankServices
{
    public class ManageBankAccountService : IManageBankAccount
    {
        private readonly FullPracticeDbContext dbContext;
        private readonly IJwtService jwt;
        public ManageBankAccountService(FullPracticeDbContext dbContext, IJwtService jwt)
        {
            this.dbContext = dbContext;
            this.jwt = jwt;
        }
        public async Task CreateAccount(int userId)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(a => a.Id == userId);
            if(user is null)
            {
                throw new Exception("User doesn't exist");
            }
            var account = new BankAccount
            {
                Balance = 0,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };
            dbContext.BankAccounts.Add(account);
            await dbContext.SaveChangesAsync();
        }
        public async Task DeleteAccount(int userId, int accountId)
        {
            var account = await dbContext.BankAccounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);
            var user = await dbContext.Users.FirstOrDefaultAsync(a => a.Id == userId);
            if (user is null)
            {
                throw new Exception("User doesn't exist");
            }
            if (account is null)
            {
                throw new Exception("Account doesn't exist");
            }
            dbContext.BankAccounts.Remove(account);
            await dbContext.SaveChangesAsync();
        }
        public async Task<double> Deposit(int userId, int accountId, double amount)
        {
            if(amount <= 0)
            {
                throw new Exception("Deposit amount must be greater than zero");
            }
            var user = await dbContext.Users.FirstOrDefaultAsync(a => a.Id == userId);
            var amountDeposited = user?.AmountDeposited;
            var limit = 10000;
            if ((amountDeposited + amount) > limit)
            {
                throw new Exception("Deposit limit exceeded");
            }
            var account = await dbContext.BankAccounts.FirstOrDefaultAsync(a => a.Id == accountId &&a.UserId == userId);
            account.Balance += amount;
            user.AmountDeposited += amount;
            var transaction = new Transaction
            {
                Amount = amount,
                Type = TransactionTypeEnum.Deposit,
                TransactionDate = DateTime.UtcNow,
                BankAccountId = accountId
            };
            await dbContext.Transactions.AddAsync(transaction);
            await dbContext.SaveChangesAsync();
            BackgroundJob.Schedule<ManageBankAccountService>(svc => svc.Withdraw(userId, accountId, 100), TimeSpan.FromMinutes(2));
            return account.Balance;
        }
        public async Task<double> Withdraw(int userId, int accountId, double amount)
        {
            var account = await dbContext.BankAccounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);
            var user = await dbContext.Users.FirstOrDefaultAsync(a => a.Id == userId);
            var amountWithdrawn = user?.AmountWithdrawn;
            var limit = 10000;
            if ((amountWithdrawn + amount) > limit)
            {
                throw new Exception("Withdrawal limit exceeded");
            }
            if (amount > account.Balance)
            {
                throw new Exception("Insufficient balance");
            }
            account.Balance -= amount;
            user.AmountWithdrawn += amount;
            var transaction = new Transaction
            {
                Amount = amount,
                Type = TransactionTypeEnum.Withdraw,
                TransactionDate = DateTime.UtcNow,
                BankAccountId = accountId
            };
            await dbContext.Transactions.AddAsync(transaction);
            await dbContext.SaveChangesAsync();
            return account.Balance;
        }
        public async Task<object> GetBankAccountList()
        {
            var list = await dbContext.BankAccounts.Select(a => new BankAccountDetailsDto
            {
                Balance = a.Balance,
                CreatedAt = a.CreatedAt,
                UserId = a.UserId
            }).ToListAsync();
            return list;
        }
        public async Task<object> GetBankAccountById(int accountId)
        {
            var account = await dbContext.BankAccounts.FirstOrDefaultAsync(a => a.Id == accountId);
            var accountDetails = new BankAccountDetailsDto
            {
                Balance = account.Balance,
                CreatedAt = account.CreatedAt,
                UserId = account.UserId
            };
            return accountDetails;
        }
        public async Task Transfer(int senderAccountId, int receiverAccountId, double amount)
        {
            var senderAccountNumber = await dbContext.BankAccounts.FirstOrDefaultAsync(b => b.Id == senderAccountId);
            var receiverAccountNumber = await dbContext.BankAccounts.FirstOrDefaultAsync(b => b.Id == receiverAccountId);
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == jwt.GetUserId());
            var limit = 10000;
            var amountTransferred = user?.AmountTransferred;
            if((amountTransferred + amount) > limit)
            {
                throw new Exception("Transfer limit exceeded");
            }
            if (senderAccountNumber is null || receiverAccountNumber is null)
            {
                throw new Exception("Account not found");
            }
            if(senderAccountNumber.Balance < amount)
            {
                throw new Exception("Insufficient balance");
            }
            senderAccountNumber.Balance -= amount;
            receiverAccountNumber.Balance += amount;
            user.AmountTransferred += amount;
            var senderTransaction = new Transaction
            {
                Amount = amount,
                Type = TransactionTypeEnum.Transfer,
                TransactionDate = DateTime.UtcNow,
                BankAccountId = senderAccountId
            };
            var receiverTransaction = new Transaction
            {
                Amount = amount,
                Type = TransactionTypeEnum.Transfer,
                TransactionDate = DateTime.UtcNow,
                BankAccountId = receiverAccountId
            };
            await dbContext.Transactions.AddAsync(senderTransaction);
            await dbContext.Transactions.AddAsync(receiverTransaction);
            await dbContext.SaveChangesAsync();
        }
        public async Task<object> GetTransactions()
        {
            var transactions = await dbContext.Transactions.Select(t => new
            {
                Amount = t.Amount,
                Type = t.Type,
                TransactionDate = t.TransactionDate,
                BankAccountId = t.BankAccountId
            }).ToListAsync();
            return transactions;
        }
        public async Task<object> GetTransactionById(int account)
        {
            var accountId = await dbContext.Transactions.FirstOrDefaultAsync(t => t.Id == account);
            var transaction = new
            {
                Amount = accountId.Amount,
                Type = accountId.Type,
                TransactionDate = accountId.TransactionDate,
                BankAccountId = accountId.BankAccountId
            };
            return transaction;
        }

    }
}

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
        public async Task CreateAccount()
        {
            var userId = jwt.GetUserId();

            // used AnyAsync instead of FirstOrDefaultAsync for better performance
            var user = await dbContext.Users.AnyAsync(a => a.Id == userId);

            if(!user)
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
            // also here used AnyAsync instead of FirstOrDefaultAsync for better performance
            var user = await dbContext.Users.AnyAsync(a => a.Id == userId);

            if (!user)
            {
                throw new Exception("User doesn't exist");
            }

            var account = await dbContext.BankAccounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

            if (account is null)
            {
                throw new Exception("Account doesn't exist");
            }

            account.IsDeleted = true;

            account.DeletedAt = DateTime.UtcNow;

            account.DeletedById = jwt.GetUserId();

            await dbContext.SaveChangesAsync();
        }
        public async Task<double> Deposit(int accountId, double amount)
        {
            var userId = jwt.GetUserId();

            var user = await dbContext.Users.FirstOrDefaultAsync(a => a.Id == userId);

            if(user is null)
            {
                throw new Exception("User doesn't exist");
            }

            var account = await dbContext.BankAccounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

            if (account is null) {
                throw new Exception("Account doesn't exist");
            }

            if (amount <= 0)
            {
                throw new Exception("Deposit amount must be greater than zero");
            }

            // removed the ? from user?.AmountDeposited since we already check if user is null
            var amountDeposited = user.AmountDeposited;

            var limit = 10000;

            if ((amountDeposited + amount) > limit)
            {
                throw new Exception("Deposit limit exceeded");
            }
            
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

            BackgroundJob.Schedule<ManageBankAccountService>(svc => svc.Withdraw(accountId, 100), TimeSpan.FromMinutes(2));

            return account.Balance;
        }
        public async Task<double> Withdraw(int accountId, double amount)
        {
            var userId = jwt.GetUserId();

            var user = await dbContext.Users.FirstOrDefaultAsync(a => a.Id == userId);

            if (user is null)
            {
                throw new Exception("User doesn't exist");
            }

            var account = await dbContext.BankAccounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);
           
            if (account is null)
            {
                throw new Exception("Account doesn't exist");
            }
            var amountWithdrawn = user.AmountWithdrawn;

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
            })
                .AsNoTracking()
                .ToListAsync();

            return list;
        }
        public async Task<object> GetBankAccountById(int accountId)
        {
            var account = await dbContext.BankAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if(account == null)
            {
                throw new Exception("Account not found");
            }

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
            var userId = jwt.GetUserId();

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if(user is null)
            {
                throw new Exception("User not found");
            }

            var senderAccountNumber = await dbContext.BankAccounts.FirstOrDefaultAsync(b => b.Id == senderAccountId);

            var receiverAccountNumber = await dbContext.BankAccounts.FirstOrDefaultAsync(b => b.Id == receiverAccountId);

            if(senderAccountNumber is null || receiverAccountNumber is null)
            {
                throw new Exception("Account not found");
            }

            if(senderAccountNumber.UserId != userId)
            {
                throw new Exception("Unauthorized access to sender account");
            }

            if (senderAccountId == receiverAccountId)
            {
                throw new Exception("Cannot transfer to the same account");
            }

            if (amount <= 0)
            {
                throw new Exception("Transfer amount must be greater than zero");
            }

            var limit = 10000;

            var amountTransferred = user.AmountTransferred;

            if((amountTransferred + amount) > limit)
            {
                throw new Exception("Transfer limit exceeded");
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

            await dbContext.Transactions.AddRangeAsync(senderTransaction, receiverTransaction);

            await dbContext.SaveChangesAsync();
        }
        public async Task<object> GetTransactions()
        {
            var transactions = await dbContext.Transactions.Select(t => new
            {
               t.Amount,
               t.Type,
               t.TransactionDate,
               t.BankAccountId
            })
                .AsNoTracking()
                .ToListAsync();

            return transactions;
        }
        public async Task<object> GetTransactionById(int account)
        {
            var accountId = await dbContext.Transactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == account);

            if (accountId == null) {
                throw new Exception("Transaction not found");
            }

            var transaction = new
            {
                accountId.Amount,
                accountId.Type,
                accountId.TransactionDate,
                accountId.BankAccountId
            };

            return transaction;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullPracticeApp.Contracts.Interfaces
{
    public interface IManageBankAccount
    {                                                                                                                                           
        Task CreateAccount(); 
        Task DeleteAccount(int userId, int accountId);
        Task<double> Deposit(int accountId, double amount);
        Task<double> Withdraw(int accountId, double amount);
        Task Transfer(int senderAccountId, int receiverAccountId, double amount);
        Task<object> GetBankAccountList();
        Task<object> GetBankAccountById(int accountId);
        Task<object> GetTransactions();
        Task<object> GetTransactionById(int accountId);
    }
}

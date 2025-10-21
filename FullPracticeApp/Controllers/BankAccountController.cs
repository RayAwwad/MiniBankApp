using FullPracticeApp.Contracts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FullPracticeApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankAccountController : Controller
    {
        private readonly IManageBankAccount manageBankAccount;
        public BankAccountController(IManageBankAccount manageBankAccount)
        {
            this.manageBankAccount = manageBankAccount;
        }

        [HttpGet("get-accounts")]
        [Authorize(Policy = "AdminEmail")]
        public async Task<IActionResult> GetBankAccountList()
        {
            var accounts = await manageBankAccount.GetBankAccountList();
            return Ok(accounts);
        }

        [HttpGet("get-account-by-id")]
        public async Task<IActionResult> GetBankAccountById(int accountId)
        {
            var account = await manageBankAccount.GetBankAccountById(accountId);
            return Ok(account);
        }

        [HttpPost("create-account")]
        public async Task<IActionResult> CreateAccount(int userId)
        {
            await manageBankAccount.CreateAccount();
            return Ok("Account created successfully");
        }

        [HttpPut("deposit")]
        public async Task<IActionResult> Deposit(int accountId, double amount)
        {
            var balance = await manageBankAccount.Deposit(accountId, amount);
            return Ok(balance);
        }

        [HttpPut("withdraw")]
        public async Task<IActionResult> Withdraw(int accountId, double amount)
        {
            var balance = await manageBankAccount.Withdraw(accountId, amount);
            return Ok(balance);
        }

        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount(int userId, int accountId)
        {
            await manageBankAccount.DeleteAccount(userId, accountId);
            return Ok("Account deleted successfully");
        }
        [HttpPut("transfer")]
        public async Task<IActionResult> Transfer(int senderAccountId, int receiverAccountId, double amount)
        {
            await manageBankAccount.Transfer(senderAccountId, receiverAccountId, amount);
            return Ok("Transfer successful");
        }
        [HttpGet("get-transactions")]
        public async Task<IActionResult> GetTransactions()
        {
            var transactions = await manageBankAccount.GetTransactions();
            return Ok(transactions);
        }
        [HttpGet("get-transaction-by-id")]
        public async Task<IActionResult> GetTransactionById(int accountId)
        {
            var transaction = await manageBankAccount.GetTransactionById(accountId);
            return Ok(transaction);
        }

    }
}

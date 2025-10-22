using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FullPracticeApp.Domain.Entities
{
    public class Users
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string HashedPassword { get; set; }
        public string? RefreshToken { get; set; } = null;
        public bool IsDeleted { get; set; } = false;
        public int DeletedById { get; set; }  
        public DateTime DeletedAt { get; set; }
        public double AmountWithdrawn { get; set; } = 0;
        public double AmountDeposited { get; set; } = 0;
        public double AmountTransferred { get; set; } = 0;

        public List<BankAccount> BankAccounts { get; set; }
    }
}

using FullPracticeApp.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullPracticeApp.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public double Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public TransactionTypeEnum Type { get; set; }

        public int BankAccountId { get; set; }  
        public BankAccount BankAccount { get; set; }
    }
}

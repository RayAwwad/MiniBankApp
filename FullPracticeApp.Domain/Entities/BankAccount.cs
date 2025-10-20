using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FullPracticeApp.Domain.Entities
{
    public class BankAccount
    {
        public int Id { get; set; }
        public double Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public Users User { get; set; }

        public List<Transaction> Transactions { get; set; }
    }
}

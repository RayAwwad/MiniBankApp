using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullPracticeApp.Contracts.Interfaces
{
    public interface IJwtService
    {
        public int GetUserId();
        public string GetUserEmail();
    }
}

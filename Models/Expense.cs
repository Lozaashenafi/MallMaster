using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public int MallId { get; set; }
        public int ExpenseTypeId { get; set; }
        public string Description { get; set; }
        public double ExpenseAmount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public bool? IsAcrive { get; set; } = true;
        public virtual ExpenseType ExpenseType { get; set; }

    }
}
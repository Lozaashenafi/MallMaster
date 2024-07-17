using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Models.ViewModels
{
    public class ExpenseVM
    {
        public int? ExpenseTypeId { get; set; }
        public double ExpenseAmount { get; set; }
        public string? Description { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string? Other { get; set; }
    }
}
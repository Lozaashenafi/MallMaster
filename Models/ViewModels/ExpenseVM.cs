using System;
using System.ComponentModel.DataAnnotations;

namespace MallMinder.Models.ViewModels
{
    public class ExpenseVM
    {
        [Required(ErrorMessage = "Expense Type is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Expense Type.")]
        public int? ExpenseTypeId { get; set; }

        [Required(ErrorMessage = "Expense Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Expense Amount must be greater than zero.")]
        public double ExpenseAmount { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Expense Date is required.")]
        public DateTime ExpenseDate { get; set; }

        public string? Other { get; set; }
    }
}

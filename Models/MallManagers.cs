using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Models
{
    public class MallManagers
    {
        public int Id { get; set; }
        public int MallId { get; set; }
        public String? OwnerId { get; set; }

    }
}
using AvaBPMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaBPMS.Application.Models
{
    public class PoolDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
       
    }
    public class PoolCreateDto
    {
       
        public string Title { get; set; }

    }
    public class PoolEditDto
    {
        public int Id { get; set; }
        public string Title { get; set; }

    }
}

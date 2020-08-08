using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DTO
{
  public  class Hieararchy
    {
        public Contract parent;
        public IEnumerable<Contract> children;
    }
}

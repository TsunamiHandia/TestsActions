using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepeaterModule.API.DB
{
    public abstract class TypeDB : IRepeaterType
    {
        public RepeaterSqlModel model;
        public abstract RepeaterResponse DoAction();
    }
}

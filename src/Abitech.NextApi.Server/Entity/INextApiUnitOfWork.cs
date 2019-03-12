using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abitech.NextApi.Server.Entity
{
    public interface INextApiUnitOfWork
    {
        Task Commit();
    }
}
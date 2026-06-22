using Microsoft.EntityFrameworkCore;
using UnitofWork.DbContexts;

namespace UnitofWork.InUnitOfWork;

public interface IContextCenter
{
    TestContext TestContext { get; }
    QuartzContext QuartzContext { get; }
    IEnumerable<DbContext> GetAllContexts();
}

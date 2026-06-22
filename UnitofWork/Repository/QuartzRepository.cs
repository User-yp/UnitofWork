using UnitofWork.DbContexts;
using UnitofWork.Entity;
using UnitofWork.InUnitOfWork;
using UnitofWork.IRepository;

namespace UnitofWork.Repository;

public class QuartzRepository : IQuartzRepository
{
    private readonly QuartzContext _quartzContext;

    public QuartzRepository(IContextCenter contextCenter)
    {
        _quartzContext = contextCenter.QuartzContext;
    }

    public async Task AddJobConfig(JobConfig jobConfig)
    {
        await _quartzContext.JobConfigs.AddAsync(jobConfig);
    }
}

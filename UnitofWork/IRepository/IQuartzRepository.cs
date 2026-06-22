using UnitofWork.Entity;

namespace UnitofWork.IRepository;

public interface IQuartzRepository
{
    Task AddJobConfig(JobConfig jobConfig);
}

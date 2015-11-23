using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using CostEffectiveCode.Common;
using CostEffectiveCode.Domain.Cqrs.Commands;
using CostEffectiveCode.Domain.Cqrs.Queries;
using CostEffectiveCode.Domain.Ddd.Entities;
using CostEffectiveCode.Domain.Ddd.UnitOfWork;

namespace CostEffectiveCode.BackOffice.WebApi.Controller
{
    public class EntityApiController<TEntity, TPrimaryKey> : EntityApiController<TEntity, TPrimaryKey, TEntity> 
        where TEntity : class, IEntityBase<TPrimaryKey>
    {
        public EntityApiController(IQueryFactory queryFactory, ICommandFactory commandFactory, IMapper mapper, ILogger logger)
            : base(queryFactory, commandFactory, mapper, logger)
        {
        }

    }

}
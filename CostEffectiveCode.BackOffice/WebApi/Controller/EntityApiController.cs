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
    public class EntityApiController<TEntity> : EntityApiControllerBase<TEntity> 
        where TEntity : class, IEntityBase<long>
    {
        public EntityApiController(IQueryFactory queryFactory, 
            ICommandFactory commandFactory)
            :base(queryFactory, commandFactory)
        {
        }

        // PUT: api/WearBrands/5
        [ResponseType(typeof(void))]
        public virtual IHttpActionResult Put(long id, TEntity entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != entity.Id)
            {
                return BadRequest();
            }

            //_db.Entry(entity).State = EntityState.Modified;

            CommandFactory
                .GetUpdateCommand<TEntity>()
                .Execute(entity);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/WearBrands
        //[ResponseType(typeof(WearBrand))]
        public virtual IHttpActionResult Post(TEntity entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            CommandFactory
                .GetCreateCommand<TEntity>()
                .Execute(entity);

            return CreatedAtRoute(DefaultApiRouteName, new { id = entity.Id }, entity);
        }

    }

}
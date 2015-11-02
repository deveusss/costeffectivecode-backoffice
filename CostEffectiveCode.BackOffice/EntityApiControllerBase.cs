using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Http;
using CostEffectiveCode.Common;
using CostEffectiveCode.Domain;
using CostEffectiveCode.Domain.Cqrs.Commands;
using CostEffectiveCode.Domain.Cqrs.Queries;
using CostEffectiveCode.Domain.Ddd.Entities;
using CostEffectiveCode.Domain.Ddd.UnitOfWork;

namespace CostEffectiveCode.BackOffice
{
    //[System.Web.Mvc.Authorize]
    public class EntityApiControllerBase<TEntity> : ApiController
        where TEntity : class, IEntityBase<long>
    {
        protected const string DefaultApiRouteName = "DefaultApi";
        protected readonly IQueryFactory QueryFactory;
        protected readonly ICommandFactory CommandFactory;
        protected readonly IScope<IUnitOfWork> UowScope;

        public EntityApiControllerBase(IQueryFactory queryFactory, ICommandFactory commandFactory, IScope<IUnitOfWork> uowScope)
        {
            QueryFactory = queryFactory;
            CommandFactory = commandFactory;
            UowScope = uowScope;
        }

        // GET: api/product
        public virtual IHttpActionResult Get()
        {
            var entities = LoadEntities();
            return Ok(entities);
        }

        // GET: api/WearBrands/5
        //[ResponseType(typeof(T))]
        public virtual IHttpActionResult Get(long id)
        {
            var entity = GetById(id);
            if (entity == null)
            {
                return NotFound();
            }

            return Ok(entity);
        }

        protected virtual Expression<Func<TEntity, bool>> Where
        {
            get { return x => true; }
        }

        // DELETE: api/WearBrands/5
        //[ResponseType(typeof(WearBrand))]
        public virtual IHttpActionResult Delete(long id)
        {
            TEntity entity = GetById(id);
            if (entity == null)
            {
                return NotFound();
            }

            CommandFactory.GetDeleteCommand<TEntity>().Execute(entity);

            return Ok(entity);
        }

        protected TEntity GetById(long id)
        {
            return QueryFactory
                .GetQuery<TEntity>()
                .Where(Where)
                .ById(id);
        }

        protected IEnumerable<TEntity> LoadEntities()
        {
            return QueryFactory
                .GetQuery<TEntity>()
                .Where(Where)
                .All();
        }
    }

}
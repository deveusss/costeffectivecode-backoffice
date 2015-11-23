using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using CostEffectiveCode.Common;
using CostEffectiveCode.Domain;
using CostEffectiveCode.Domain.Cqrs.Commands;
using CostEffectiveCode.Domain.Cqrs.Queries;
using CostEffectiveCode.Domain.Ddd.Entities;
using CostEffectiveCode.Domain.Ddd.Specifications;

namespace CostEffectiveCode.BackOffice.WebApi.Controller
{
    public class EntityApiController<TEntity, TPrimaryKey, TViewModel> : ApiController
        where TEntity : class, IEntityBase<TPrimaryKey>
    {
        protected readonly IQueryFactory QueryFactory;
        protected readonly ICommandFactory CommandFactory;
        protected readonly IMapper Mapper;
        protected readonly ILogger Logger;

        protected const string DefaultApiRouteName = "DefaultApi";

        public EntityApiController(IQueryFactory queryFactory, ICommandFactory commandFactory, IMapper mapper, ILogger logger)
        {
            QueryFactory = queryFactory;
            CommandFactory = commandFactory;
            Mapper = mapper;
            Logger = logger;
        }

        //[ResponseType(typeof(IEnumerable<TViewModel>))]
        public virtual IHttpActionResult Get()
        {
            try
            {
                return Ok(LoadEntities()
                    .Select(x =>
                    {
                        var viewModel = Mapper.Map<TViewModel>(x);

                        PostProcessViewModel(viewModel, x);
                        return viewModel;
                    }));
            }
            catch (Exception e)
            {
                return ProcessBadRequest(e);
            }
        }

        //[ResponseType(typeof(TViewModel))]
        public virtual IHttpActionResult Get(TPrimaryKey id)
        {
            try
            {
                var entity = LoadById(id);

                var viewModel = Mapper.Map<TViewModel>(entity);
                PostProcessViewModel(viewModel, entity);

                return Ok(viewModel);
            }
            catch (Exception e)
            {
                return ProcessBadRequest(e);
            }
        }

        [ResponseType(typeof(void))]
        public virtual IHttpActionResult Put(TPrimaryKey id, TViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var entity = Mapper.Map<TEntity>(model);
                if (!entity.Id.Equals(id))
                {
                    return BadRequest();
                }

                CommandFactory
                    .GetUpdateCommand<TEntity>()
                    .Execute(entity);

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception e)
            {
                return ProcessBadRequest(e);
            }
        }

        //[ResponseType(typeof(TViewModel))]
        public virtual IHttpActionResult Post(TViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var entity = Mapper.Map<TEntity>(model);

                CommandFactory
                .GetCreateCommand<TEntity>()
                .Execute(entity);

                return CreatedAtRoute(DefaultApiRouteName, new { id = entity.Id }, entity);
            }
            catch (Exception e)
            {
                return ProcessBadRequest(e);
            }
        }

        //[ResponseType(typeof(TViewModel))]
        public virtual IHttpActionResult Delete(TPrimaryKey id)
        {
            try
            {
                var entity = LoadById(id);
                if (entity == null)
                {
                    return NotFound();
                }

                CommandFactory
                    .GetDeleteCommand<TEntity>()
                    .Execute(entity);

                return Ok(entity);
            }
            catch (Exception e)
            {
                return ProcessBadRequest(e);
            }
        }

        private IHttpActionResult ProcessBadRequest(Exception e)
        {
            Logger.Error(e);
            return BadRequest(e.Message);
        }

        protected virtual Expression<Func<TEntity, bool>> Where => x => true;

        protected virtual Expression<Func<TEntity, object>> Include => null;
        
        protected TEntity LoadById(TPrimaryKey id)
        {
            return GetBaseQuery().Where(q => q.Id.Equals(id)).Single();
        }

        protected IEnumerable<TEntity> LoadEntities()
        {
            return GetBaseQuery().All();
        }

        private IQuery<TEntity, IExpressionSpecification<TEntity>> GetBaseQuery()
        {
            var query = QueryFactory
                .GetQuery<TEntity>()
                .Where(Where);

            if (Include != null)
                query = query.Include(Include);

            return query;
        }

        protected virtual void PostProcessViewModel(TViewModel viewModel, TEntity entity)
        {
        }
    }

}

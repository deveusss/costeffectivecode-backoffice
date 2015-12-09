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
    public class EntityApiController<TEntity, TPrimaryKey, TReadViewModel, TModifyViewModel> : ApiController
        where TEntity : class, IEntityBase<TPrimaryKey>
        where TPrimaryKey : struct, IComparable<TPrimaryKey>
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

        //[ResponseType(typeof(IEnumerable<TReadViewModel>))]
        public virtual IHttpActionResult Get(int? paginationPageNumber = null, int? paginationTakeCount = null)
        {
            try
            {
                return Ok(LoadEntities(paginationPageNumber, paginationTakeCount)
                    .Select(x =>
                    {
                        var viewModel = Mapper.Map<TReadViewModel>(x);

                        PostProcessViewModel(viewModel, x);
                        return viewModel;
                    }));
            }
            catch (Exception e)
            {
                return ProcessBadRequest(e);
            }
        }

        //[ResponseType(typeof(TReadViewModel))]
        public virtual IHttpActionResult Get(TPrimaryKey id)
        {
            try
            {
                var entity = LoadById(id);

                var viewModel = Mapper.Map<TReadViewModel>(entity);
                PostProcessViewModel(viewModel, entity);

                return Ok(viewModel);
            }
            catch (Exception e)
            {
                return ProcessBadRequest(e);
            }
        }

        [ResponseType(typeof(void))]
        public virtual IHttpActionResult Put(TPrimaryKey id, TModifyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var entity = LoadById(id);
                Mapper.Map(model, entity);

                if (entity.Id.CompareTo(id) != 0)
                {
                    return BadRequest("Wrong id specified");
                }

                CommandFactory
                    .GetCommitCommand()
                    .Execute();

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception e)
            {
                return ProcessBadRequest(e);
            }
        }

        //[ResponseType(typeof(TPrimaryKey))]
        public virtual IHttpActionResult Post(TModifyViewModel model)
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

                return CreatedAtRoute(DefaultApiRouteName, new { id = entity.Id }, entity.Id);
            }
            catch (Exception e)
            {
                return ProcessBadRequest(e);
            }
        }

        //[ResponseType(typeof(TPrimaryKey))]
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

                return Ok(entity.Id);
            }
            catch (Exception e)
            {
                return ProcessBadRequest(e);
            }
        }

        protected IHttpActionResult ProcessBadRequest(Exception e)
        {
            Logger.Error(e);
            return BadRequest(e.Message);
        }

        // Base check for both Get(id) and Get()
        protected virtual Expression<Func<TEntity, bool>> BaseWhere => x => true;

        // Condition for list action: for Get()
        protected virtual Expression<Func<TEntity, bool>> Where => x => true;

        protected virtual Expression<Func<TEntity, object>> Include => null;

        protected TEntity LoadById(TPrimaryKey id)
        {
            return GetBaseQuery()
                .Where(x => x.Id.CompareTo(id) == 0)
                .Single();
        }

        protected IEnumerable<TEntity> LoadEntities(int? pageNumber = null, int? takeCount = null)
        {
            var query = GetBaseQuery()
                .Where(Where);

            if (pageNumber.HasValue && takeCount.HasValue)
                return query.Paged(pageNumber.Value, takeCount.Value);

            return query.All();
        }

        private IQuery<TEntity, IExpressionSpecification<TEntity>> GetBaseQuery()
        {
            var query = QueryFactory
                .GetQuery<TEntity>()
                .Where(BaseWhere);

            if (Include != null)
                query = query.Include(Include);

            return query;
        }

        protected virtual void PostProcessViewModel(TReadViewModel viewModel, TEntity entity)
        {
        }
    }

}

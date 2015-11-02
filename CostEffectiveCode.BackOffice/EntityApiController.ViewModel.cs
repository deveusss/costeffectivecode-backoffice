using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using CostEffectiveCode.Common;
using CostEffectiveCode.Domain;
using CostEffectiveCode.Domain.Cqrs.Commands;
using CostEffectiveCode.Domain.Cqrs.Queries;
using CostEffectiveCode.Domain.Ddd.Entities;
using CostEffectiveCode.Domain.Ddd.UnitOfWork;

namespace CostEffectiveCode.BackOffice
{
    public class EntityApiController<TEntity, TViewModel> : EntityApiControllerBase<TEntity>
        where TEntity : class, IEntityBase<long>
        where TViewModel : class
    {
        protected readonly IMapper Mapper;

        public EntityApiController(IQueryFactory queryFactory, ICommandFactory commandFactory, IScope<IUnitOfWork> uowScope, IMapper mapper)
            : base(queryFactory, commandFactory, uowScope)
        {
            Mapper = mapper;
        }

        //[ResponseType(typeof(IEnumerable<ShopWearViewModel>))]
        public override IHttpActionResult Get()
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
                return BadRequest(e.Message);
            }
        }

        //[ResponseType(typeof(ShopWearViewModel))]
        public override IHttpActionResult Get(long id)
        {
            try
            {
                var entity = QueryFactory
                    .GetQuery<TEntity>()
                    .Where(Where)
                    .Where(x => x.Id == id)
                    .Single();

                var viewModel = Mapper.Map<TViewModel>(entity);
                PostProcessViewModel(viewModel, entity);

                return Ok(viewModel);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [ResponseType(typeof(void))]
        public virtual IHttpActionResult Put(long id, TViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var entity = Mapper.Map<TEntity>(model);
                if (id != entity.Id)
                {
                    return BadRequest();
                }

#warning please use UpdateCommand instead (will be avaialable in CostEffectiveCode 2.0.0)
                UowScope.GetScoped().Save(entity);
                UowScope.GetScoped().Commit();

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // POST: api/WearBrands
        //[ResponseType(typeof(WearBrand))]
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
                return BadRequest(e.Message);
            }
        }

        protected virtual void PostProcessViewModel(TViewModel viewModel, TEntity entity)
        {
        }
    }

}

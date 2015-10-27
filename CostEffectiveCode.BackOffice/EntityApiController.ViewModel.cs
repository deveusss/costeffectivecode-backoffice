using System;
using System.Linq;
using System.Web.Http;
using CostEffectiveCode.Common;
using CostEffectiveCode.Domain;
using CostEffectiveCode.Domain.Cqrs.Commands;
using CostEffectiveCode.Domain.Cqrs.Queries;
using CostEffectiveCode.Domain.Ddd.Entities;
using CostEffectiveCode.Domain.Ddd.UnitOfWork;

namespace CostEffectiveCode.BackOffice
{
    public class EntityApiController<TEntity, TViewModel> : EntityApiController<TEntity>
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


        protected virtual void PostProcessViewModel(TViewModel viewModel, TEntity entity)
        {
        }
    }

}

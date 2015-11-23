using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using CostEffectiveCode.BackOffice.WebApi.Controller;
using CostEffectiveCode.Domain.Ddd.Entities;
using CostEffectiveCode.Web;

namespace CostEffectiveCode.BackOffice.WebApi.Infrastructure
{
    public class RuntimeScaffoldingHttpControllerSelector : DefaultHttpControllerSelector
    {
        // Unfortunately there is no way to reuse _configuration from base class, nevertheless it's all the same
        private readonly HttpConfiguration _configuration;

        // { products => Store.Domain.Product }
        private readonly Dictionary<string, Type> _typesDictionary;

        public RuntimeScaffoldingHttpControllerSelector(
            HttpConfiguration configuration,
            RuntimeScaffoldingApproach runtimeScaffoldingApproach = RuntimeScaffoldingApproach.Allow,
            params Assembly[] assemblies)
            : base(configuration)
        {
            _configuration = configuration;

            if (!assemblies.Any())
            {
                throw new ArgumentException("Empty collection of assemblies given", nameof(assemblies));
            }

            var types = new List<Type>();
            foreach (var a in assemblies)
            {
                types.AddRange(
                    a.DefinedTypes
                    .Where(x => x.IsClass && !x.IsAbstract && typeof(IEntityBase<>).IsAssignableFrom(x) &&
                        ((runtimeScaffoldingApproach == RuntimeScaffoldingApproach.Allow && x.IsDefined(typeof(AllowScaffoldAttribute)))
                        || (runtimeScaffoldingApproach == RuntimeScaffoldingApproach.Deny && !x.IsDefined(typeof(DenyScaffoldAttribute))))
                    ));
            }

            _typesDictionary = new Dictionary<string, Type>();
            foreach (var x in types)
            {
                _typesDictionary[x.Name.ToLowerInvariant()] = x;
            }
        }

        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            try
            {
                return base.SelectController(request);
            }
            catch (HttpResponseException e)
            {
                if (e.Response.StatusCode != HttpStatusCode.NotFound)
                {
                    // rethrow exception
                    throw;
                }

                // if the controller was not found via a standard way -- we perform a fall-back to EntityApiController<T>
                return TryFallbackToDefaultController(request);
            }
        }

        private HttpControllerDescriptor TryFallbackToDefaultController(HttpRequestMessage request)
        {
            var controllerName = GetControllerName(request);
            var name = controllerName.ToLowerInvariant();

            var entityType = _typesDictionary[name];

            var pkType = entityType.GetGenericArguments().First();

            var entityApiControllerType = typeof(EntityApiController<,>).MakeGenericType(entityType, pkType);

            return new HttpControllerDescriptor(_configuration, "EntityApi", entityApiControllerType);
        }
    }
}

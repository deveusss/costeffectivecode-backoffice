using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using CostEffectiveCode.Domain.Ddd.Entities;
using CostEffectiveCode.Web;

namespace CostEffectiveCode.BackOffice
{
    public class RuntimeScaffoldingHttpControllerSelector : DefaultHttpControllerSelector
    {
        // Unfortunately there is no way to reuse _configuration from base class, nevertheless it's all the same
        private readonly HttpConfiguration _configuration;

        // { dummies => Boommy.Domain.Dummy }
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
                throw new ArgumentException("Empty collection of assemblies given", "assemblies");
            }

            _typesDictionary = new Dictionary<string, Type>();

            var types = new List<Type>();
            foreach (var a in assemblies)
            {
                types.AddRange(a.GetTypes());
            }

            var entityTypes = types.Where(x => x.IsClass && !x.IsAbstract && typeof(IEntity).IsAssignableFrom(x));

            if (runtimeScaffoldingApproach == RuntimeScaffoldingApproach.Allow)
            {
                entityTypes = entityTypes
                    .Where(x => x.IsDefined(typeof(AllowScaffoldAttribute)));
            }
            else
            {
                entityTypes = entityTypes
                    .Where(x => !x.IsDefined(typeof(DenyScaffoldAttribute)));
            }

            foreach (var x in entityTypes)
            {
                _typesDictionary.Add(x.Name.ToLowerInvariant(), x);
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
            var entityApiControllerType = typeof(EntityApiController<>).MakeGenericType(entityType);

            return new HttpControllerDescriptor(_configuration, "EntityApi", entityApiControllerType);
        }
    }
}

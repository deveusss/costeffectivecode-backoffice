using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;

namespace CostEffectiveCode.BackOffice.WebApi.Infrastructure
{
    [Obsolete("Try to change DependencyResolver of HttpConfiguration class first!")]
    public class WebApiControllerActivator : IHttpControllerActivator
    {
        private static WebApiControllerActivator _instance;

        [Obsolete("Register with castle", true)]
        public static WebApiControllerActivator Instance => _instance ?? (_instance = new WebApiControllerActivator());

        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            return (IHttpController)DependencyResolver.Current.GetService(controllerType);
        }
    }

}
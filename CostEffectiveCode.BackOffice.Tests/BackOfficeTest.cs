using Microsoft.VisualStudio.TestTools.UnitTesting;
using CostEffectiveCode.BackOffice.WebApi.Controller;
using CostEffectiveCode.Common;
using CostEffectiveCode.Domain.Cqrs.Commands;
using CostEffectiveCode.Domain.Cqrs.Queries;
using NUnit.Framework;

namespace CostEffectiveCode.BackOffice.Tests
{
    [TestFixture]
    public class BackOfficeTest
    {
        [Test]
        public void TestMethod1()
        {
            var controller = new EntityApiController<Product, long>(QueryFactory, CommandFactory, Mapper, Logger);
        }

        [SetUp]
        public void Setup()
        {

        }

        protected IQueryFactory QueryFactory { get; set; }

        protected ICommandFactory CommandFactory { get; set; }

        protected IMapper Mapper { get; set; }

        protected ILogger Logger { get; set; }
    }
}

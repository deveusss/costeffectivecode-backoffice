using System;
using JetBrains.Annotations;

namespace CostEffectiveCode.BackOffice.Attributes
{
    /// <summary>
    /// An attribute to deny <b>runtime scaffolding</b> of REST Web API2 controller for marked entity class.
    /// This technique helps you to avoid writing boilerplate code as the controller is built in runtime.
    /// You can choose between Allow or Deny approach -- by specifying it in the controller selector class.
    /// </summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Class)]
    public class DenyScaffoldAttribute : Attribute
    {

    }
}
using System;
using JetBrains.Annotations;

namespace CostEffectiveCode.BackOffice.Files
{
    [AttributeUsage(AttributeTargets.Property)]
    [PublicAPI]
    public class FileUploadAttribute : Attribute
    {
    }
}

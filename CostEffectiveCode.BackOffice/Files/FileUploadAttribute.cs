using System;
using JetBrains.Annotations;

// It looks like this stuff could be excluded from the project

namespace CostEffectiveCode.BackOffice.Files
{
    [AttributeUsage(AttributeTargets.Property)]
    [PublicAPI]
    public class FileUploadAttribute : Attribute
    {
    }
}

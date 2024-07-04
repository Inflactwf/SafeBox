using System;

namespace SafeBox.Extensions
{
    internal static class ICloneableExtensions
    {
        public static T Clone<T>(this T obj) where T : ICloneable =>
            (T)obj.Clone();
    }
}

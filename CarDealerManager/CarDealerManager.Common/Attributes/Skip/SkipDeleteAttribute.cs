using System.Reflection;

namespace CarDealerManager.Common.Attributes.Skip
{
    public class SkipDeleteAttribute : Attribute
    {
        public static bool IsDeclared(PropertyInfo propertyInfo)
            => propertyInfo.GetCustomAttribute(typeof(SkipDeleteAttribute)) != null;
    }
}

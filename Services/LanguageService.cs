using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Reflection;

namespace FastFingerTest.Services
{

    public class SharedResource { 

    }
    public class LanguageService
    {
        private readonly IStringLocalizer _localizer;
        public LanguageService(IStringLocalizerFactory factory)
        {
            var type = typeof(SharedResource);
            var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName);
            _localizer = factory.Create("SharedResource", assemblyName.Name);
        }
        public LocalizedString Getkey(string key)
        {
            return _localizer[key];
        }
        public string GetCurentLenguage()
        {
            return CultureInfo.CurrentCulture.Name;
        }
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Reflection;

namespace LiveTelemetrySensor.SensorAlerts.Services
{
    public class IgnorePropertiesContractResolver: DefaultContractResolver
    {
        private string[] _propertiesToIgnore;

        public IgnorePropertiesContractResolver(string[] propertyToIgnore)
        {
            _propertiesToIgnore = propertyToIgnore;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if(_propertiesToIgnore.Contains(property.PropertyName)){
                property.ShouldSerialize = (instance) => false;
            }
            return property;
        }
    }
}

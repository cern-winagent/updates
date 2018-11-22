using System;

namespace plugin
{
    public class PluginDefinition
    {
        public Attribute Attribute { get; set; }
        public Type ImplementationType { get; set; }

        public PluginDefinition(Attribute Attribute, Type implementationType)
        {
            this.Attribute = Attribute;
            this.ImplementationType = implementationType;
        }
    }
}

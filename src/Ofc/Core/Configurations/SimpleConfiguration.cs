namespace Ofc.Core.Configurations
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    // todo full implement of readonly
    public class SimpleConfiguration : IConfiguaration
    {
        private Dictionary<string, object> _configuration = new Dictionary<string, object>();


        internal bool HasLimitation { get; }

        internal int MaxEntries { get; }

        internal bool ReachedLimit => HasLimitation && _configuration.Count >= MaxEntries;

        internal bool Readonly { get; }


        public SimpleConfiguration() : this(-1)
        {
        }

        public SimpleConfiguration(int max)
        {
            HasLimitation = max > 0;
            MaxEntries = max > 0 ? max : -1;
            Readonly = false;
        }


        public object Get(string name)
        {
            object obj;
            return _configuration.TryGetValue(name.ToLower(), out obj) ? obj : null;
        }

        public bool Has(string name) => _configuration.ContainsKey(name.ToLower());

        public bool Remove(string name) => _configuration.Remove(name.ToLower());

        public void Set(string name, [CanBeNull] object value)
        {
            if (Readonly) throw new NotSupportedException("Configuration is readonly.");
            _configuration[name.ToLower()] = value;
            if (ReachedLimit) throw new InvalidOperationException("Configuration is full.");
        }

        public object this[string name]
        {
            get { return Get(name); }
            set { Set(name, value); }
        }
    }
}
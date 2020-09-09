using System;

namespace Datadog.Trace.ClrProfiler.CallTarget.DuckTyping
{
    /// <summary>
    /// Duck Type
    /// </summary>
    public partial class DuckType
    {
        /// <summary>
        /// Create duck type proxy using a base type
        /// </summary>
        /// <param name="instance">Instance object</param>
        /// <typeparam name="T">Duck type</typeparam>
        /// <returns>Duck type proxy</returns>
        public static T Create<T>(object instance)
        {
            return (T)Create(typeof(T), instance);
        }

        /// <summary>
        /// Create duck type proxy using a base type
        /// </summary>
        /// <param name="duckType">Duck type</param>
        /// <param name="instance">Instance object</param>
        /// <returns>Duck Type proxy</returns>
        public static IDuckType Create(Type duckType, object instance)
        {
            // Validate arguments
            EnsureArguments(duckType, instance);

            // Create Type
            CreateTypeResult result = GetOrCreateProxyType(duckType, instance.GetType());
            result.ExceptionInfo?.Throw();

            // Create instance
            var objInstance = (IDuckType)Activator.CreateInstance(result.Type);
            objInstance.SetInstance(instance);
            return objInstance;
        }
    }
}

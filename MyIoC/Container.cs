using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyIoC
{
    public class Container
    {
        private static readonly IDictionary<Type, Type> _types = new Dictionary<Type, Type>();

        public void AddAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes()
                .Where(type => type.CustomAttributes
                .Any(atr => atr.AttributeType == typeof(ImportAttribute) 
                || atr.AttributeType == typeof(ExportAttribute) 
                || atr.AttributeType == typeof(ImportConstructorAttribute))))
            {
                AddType(type);
            }
        }

        public void AddType(Type type)
        {
            _types[type] = type;
        }

        public void AddType(Type type, Type baseType)
        {
            _types[type] = baseType;
        }

        public object CreateInstance(Type type)
        {
            if (_types.ContainsKey(type))
            {
                Type implementation = _types[type];
                ConstructorInfo constructor = implementation.GetConstructors()[0];
                ParameterInfo[] constructorParameters = constructor.GetParameters();
                if (constructorParameters.Length == 0)
                {
                    return Activator.CreateInstance(implementation);
                }
                List<object> parameters = new List<object>(constructorParameters.Length);
                foreach (ParameterInfo parameterInfo in constructorParameters)
                {
                    parameters.Add(CreateInstance(parameterInfo.ParameterType));
                }
                return constructor.Invoke(parameters.ToArray());
            }
            else return default(Type);
        }

        public T CreateInstance<T>()
        {
            if (_types.ContainsKey(typeof(T)))
            {
                Type implementation = _types[typeof(T)];
                ConstructorInfo constructor = implementation.GetConstructors()[0];
                ParameterInfo[] constructorParameters = constructor.GetParameters();
                if (constructorParameters.Length == 0)
                {
                    return Activator.CreateInstance<T>();
                }
                List<object> parameters = new List<object>(constructorParameters.Length);
                foreach (ParameterInfo parameterInfo in constructorParameters)
                {
                    parameters.Add(CreateInstance(parameterInfo.ParameterType));
                }
                return (dynamic)constructor.Invoke(parameters.ToArray());
            }
            else return default(T);
        }

        public void Sample()
        {
            var container = new Container();
            container.AddAssembly(Assembly.GetExecutingAssembly());

            var customerBLL = (CustomerBLL)container.CreateInstance(typeof(CustomerBLL));
            var customerBLL2 = container.CreateInstance<CustomerBLL>();

            container.AddType(typeof(CustomerBLL));
            container.AddType(typeof(Logger));
            container.AddType(typeof(CustomerDAL), typeof(ICustomerDAL));
        }
    }
}

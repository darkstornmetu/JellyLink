using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

public static class ServiceContainer
{
    private static readonly Dictionary<Type, object> sr_services = new();

    /// <summary>
    /// Registers a service implementation for a specific type
    /// </summary>
    /// <param name="service"></param>
    /// <typeparam name="T"></typeparam>
    public static void Register<T>(T service)
    {
        sr_services[typeof(T)] = service;
    }
    
    /// <summary>
    /// Resolves a service instance for a specific type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Resolve<T>()
    {
        return (T)sr_services[typeof(T)];
    }

    /// <summary>
    /// Performs dependency injection on the target object
    /// </summary>
    /// <param name="target"></param>
    public static void InjectDependencies(object target)
    {
        var targetType = target.GetType();

        // Inject fields and properties
        foreach (var member in targetType.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        {
            if (member.GetCustomAttribute<InjectAttribute>() != null)
            {
                if (member is FieldInfo field)
                {
                    Type fieldType = field.FieldType;
                    if (sr_services.TryGetValue(fieldType, out var service))
                    {
                        field.SetValue(target, service);
                    }
                }
                else if (member is PropertyInfo property)
                {
                    Type propertyType = property.PropertyType;
                    if (sr_services.TryGetValue(propertyType, out var service) && property.CanWrite)
                    {
                        property.SetValue(target, service);
                    }
                }
            }
        }
        
        // Inject methods with parameters
        foreach (var method in targetType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        {
            if (method.GetCustomAttribute<InjectAttribute>() != null)
            {
                var parameters = method.GetParameters();
                var args = new object[parameters.Length];
                bool canInvoke = true;

                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameterType = parameters[i].ParameterType;
                    if (sr_services.TryGetValue(parameterType, out var service))
                    {
                        args[i] = service;
                    }
                    else
                    {
                        Debug.LogError($"No registered service for type {parameterType} required by {method.Name} in {targetType.Name}");
                        canInvoke = false;
                        break;
                    }
                }

                if (canInvoke)
                {
                    method.Invoke(target, args);
                }
            }
        }
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class InjectableAttribute : Attribute
{
    
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
[MeansImplicitUse]
public class InjectAttribute : Attribute
{
}
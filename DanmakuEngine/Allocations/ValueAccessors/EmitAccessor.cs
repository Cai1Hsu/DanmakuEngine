using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace DanmakuEngine.Allocations.ValueAccessors;

/// <summary>
/// A value accessor using emit or reflection.
/// </summary>
/// <typeparam name="TValue">The type of the field or property</typeparam>
/// <typeparam name="TTransformable">The type that contains the value or property</typeparam>
public class EmitAccessor<TValue, TTransformable> : IAccessor<TValue>
{
    public delegate TValue GetterDelegate(TTransformable transformable);
    public delegate void SetterDelegate(TTransformable transformable, TValue value);

    private static GetterDelegate createFieldGetter(FieldInfo field)
    {
        if (!RuntimeFeature.IsDynamicCodeCompiled)
            return transformable => (TValue)field.GetValue(transformable)!;

        string methodName = $"{typeof(TValue)}.get_{field.Name}_{Guid.NewGuid():N}";

        DynamicMethod setterMethod = new DynamicMethod(methodName, typeof(TValue), [typeof(TTransformable)], true);
        ILGenerator gen = setterMethod.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldfld, field);
        gen.Emit(OpCodes.Ret);

        return setterMethod.CreateDelegate<GetterDelegate>();
    }

    private static SetterDelegate createFieldSetter(FieldInfo field)
    {
        if (!RuntimeFeature.IsDynamicCodeCompiled)
            return (transformable, value) => field.SetValue(transformable, value);

        string methodName = $"{typeof(TValue)}.set_{field.Name}_{Guid.NewGuid():N}";

        DynamicMethod setterMethod = new DynamicMethod(methodName, null, [typeof(TTransformable), typeof(TValue)], true);
        ILGenerator gen = setterMethod.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Stfld, field);
        gen.Emit(OpCodes.Ret);

        return setterMethod.CreateDelegate<SetterDelegate>();
    }

    private static GetterDelegate createPropertyGetter(MethodInfo getter)
    {
        if (!RuntimeFeature.IsDynamicCodeCompiled)
            return transformable => (TValue)getter.Invoke(transformable, [])!;

        return getter.CreateDelegate<GetterDelegate>();
    }

    private static SetterDelegate createPropertySetter(MethodInfo setter)
    {
        if (!RuntimeFeature.IsDynamicCodeCompiled)
            return (transformable, value) => setter.Invoke(transformable, [value]);

        return setter.CreateDelegate<SetterDelegate>();
    }

    private bool tryInitializeFieldAccessor(TTransformable transformable, string memberName)
    {
        FieldInfo? field = typeof(TTransformable).GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (field is null)
            return false;

        if (field.FieldType != typeof(TValue))
        {
            throw new InvalidOperationException(
                $"Cannot create accessor for field {typeof(TTransformable)}.{memberName} " +
                $"since its type should be {typeof(TValue)}, but is {field.FieldType}.");
        }

        if (field.IsStatic)
        {
            throw new NotSupportedException(
                $"Cannot create accessor for static field {typeof(TTransformable)}.{memberName}.");
        }

        var getter = createFieldGetter(field);
        var setter = createFieldSetter(field);

        _getterCache.TryAdd((typeof(TTransformable), memberName), getter);
        _setterCache.TryAdd((typeof(TTransformable), memberName), setter);

        _getter ??= getter;
        _setter ??= setter;

        return true;
    }

    private bool tryInitializePropertyAccessor(TTransformable transformable, string memberName)
    {
        PropertyInfo? property = typeof(TTransformable).GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (property is null)
            return false;

        if (property.PropertyType != typeof(TValue))
        {
            throw new InvalidOperationException(
                $"Cannot create accessor for property {typeof(TTransformable)}.{memberName} " +
                $"since its type should be {typeof(TValue)}, but is {property.PropertyType}.");
        }

        if (property.GetMethod is null)
        {
            throw new NotSupportedException(
                $"Cannot create accessor for property {typeof(TTransformable)}.{memberName} " +
                "since it does not have a getter.");
        }

        if (property.SetMethod is null)
        {
            throw new NotSupportedException(
                $"Cannot create accessor for property {typeof(TTransformable)}.{memberName} " +
                "since it does not have a setter.");
        }

        var getter = createPropertyGetter(property.GetMethod);
        var setter = createPropertySetter(property.SetMethod);

        _getterCache.TryAdd((typeof(TTransformable), memberName), getter);
        _setterCache.TryAdd((typeof(TTransformable), memberName), setter);

        _getter ??= getter;
        _setter ??= setter;

        return true;
    }

    private static ConcurrentDictionary<(Type, string), GetterDelegate> _getterCache = new();
    private static ConcurrentDictionary<(Type, string), SetterDelegate> _setterCache = new();

    private GetterDelegate _getter = null!;
    private SetterDelegate _setter = null!;

    private readonly TTransformable _transformable;

    public EmitAccessor(TTransformable transformable, string memberName, bool? isField = null)
    {
        _transformable = transformable;

        if (_getterCache.TryGetValue((typeof(TTransformable), memberName), out GetterDelegate? getter))
        {
            Debug.Assert(getter is not null);

            _getter = getter;

            if (_setterCache.TryGetValue((typeof(TTransformable), memberName), out SetterDelegate? setter))
            {
                Debug.Assert(setter is not null);

                _setter = setter;

                return;
            }
        }

        if (isField.HasValue)
        {
            if (isField.Value)
            {
                if (!tryInitializeFieldAccessor(transformable, memberName))
                    throw new ArgumentException("The field does not exist or is not accessible.");
            }
            else
            {
                if (!tryInitializePropertyAccessor(transformable, memberName))
                    throw new ArgumentException("The property does not exist or is not accessible.");
            }
        }
        else
        {
            // Try with property first as we assume that most developers have good practices which is to use properties for public access.
            if (!tryInitializePropertyAccessor(transformable, memberName)
                && !tryInitializeFieldAccessor(transformable, memberName))
                throw new ArgumentException("The field or property does not exist or is not accessible.");
        }

        Debug.Assert(_getter is not null);
        Debug.Assert(_setter is not null);
    }

    public TValue Value
    {
        get => _getter(_transformable);
        set => _setter(_transformable, value);
    }
}

using System.Reflection;

namespace DanmakuEngine.Dependency;

public interface IInjectable
{
    /// <summary>
    /// Auto inject dependency with <see cref="InjectAttribute"/>
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="Exception">Raises when unable to inject a dependency</exception>
    public void AutoInject(bool inherit = false)
    {
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        foreach (var fieldInfo in this.GetType().GetFields(flags))
        {
            var attributes = fieldInfo.GetCustomAttributes(inherit)
                .Where(a => a is InjectAttribute).ToArray();

            if (!attributes.Any())
                continue;

            if (attributes.First() is not InjectAttribute inject)
                continue;

            var type = fieldInfo.FieldType;

            var value = DependencyContainer.Instance.Get(type);

            if (value == null)
                throw new Exception($"Unable to inject {type} for {fieldInfo.Name}, value is null");

            fieldInfo.SetValue(this, value);
        }

        foreach (var propInfo in this.GetType().GetProperties(flags))
        {
            var attributes = propInfo.GetCustomAttributes(inherit)
                .Where(a => a is InjectAttribute).ToArray();

            if (!attributes.Any())
                continue;

            if (attributes.First() is not InjectAttribute inject)
                continue;

            var type = propInfo.PropertyType;

            var value = DependencyContainer.Instance.Get(type);

            if (value == null)
                throw new Exception($"Unable to inject {type} for {propInfo.Name}, value is null");

            propInfo.SetValue(this, value);
        }

        if (this is IAutoloadable autoloadable)
            autoloadable.OnInjected();
    }
}

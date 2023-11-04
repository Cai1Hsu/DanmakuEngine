namespace DanmakuEngine.Dependency;

public interface IInjectable
{
    public void Inject(DependencyContainer container);

    /// <summary>
    /// Auto inject dependency with <see cref="InjectAttribute"/>
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="Exception">Raises when unable to inject a dependency</exception>
    public void AutoInject()
    {
        foreach (var fieldInfo in this.GetType().GetFields())
        {
            var attributes = fieldInfo.GetCustomAttributes(false)
                .Where(a => a is InjectAttribute).ToArray();

            if (!attributes.Any())
                continue;

            if (attributes.First() is not InjectAttribute inject)
                continue;

            var type = fieldInfo.FieldType;

            if (!typeof(IInjectable).IsAssignableFrom(type))
                throw new InvalidOperationException("Can not inject a non IInjectable object");

            var value = DependencyContainer.Instance.Get(type);

            if (value == null)
                throw new Exception($"Unable to inject {type}");

            fieldInfo.SetValue(this, value);
        }

        foreach (var propInfo in this.GetType().GetProperties())
        {
            var attributes = propInfo.GetCustomAttributes(false)
                .Where(a => a is InjectAttribute).ToArray();

            if (!attributes.Any())
                continue;

            if (attributes.First() is not InjectAttribute inject)
                continue;

            var type = propInfo.PropertyType;

            if (!typeof(IInjectable).IsAssignableFrom(type))
                throw new InvalidOperationException("Can not inject a non IInjectable object");

            var value = DependencyContainer.Instance.Get(type);

            if (value == null)
                throw new Exception($"Unable to inject {type}");

            propInfo.SetValue(this, value);
        }

        if (this is IAutoloadable autoloadable)
            autoloadable.OnLoad();
    }
}

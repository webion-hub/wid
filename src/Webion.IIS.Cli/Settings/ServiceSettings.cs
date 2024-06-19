namespace Webion.IIS.Cli.Settings;

public sealed class ServiceSettings
{
    public EnvironmentSettings[] Envs { get; init; } = [];
    public string BundleDir { get; init; } = null!;
    public string IgnoreFile { get; init; } = ".widignore";
    public BuildSettings[] Build { get; init; } = [];


    public EnvironmentSettings GetEnvironment(string? env)
    {
        return env is null 
            ? Envs.First()
            : Envs.First(x => x.Name == env);
    }
}
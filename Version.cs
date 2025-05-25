using System.Reflection;

namespace MassoToolEditor;

/// <summary>
/// Provides centralized version information for the application.
/// Version numbers are managed in Directory.Build.props.
/// </summary>
public static class Version
{
    /// <summary>
    /// Gets the application version in the format "Major.Minor.Patch"
    /// </summary>
    public static string ApplicationVersion =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

    /// <summary>
    /// Gets the full assembly version in the format "Major.Minor.Patch.Revision"
    /// </summary>
    public static string AssemblyVersion =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";

    /// <summary>
    /// Gets the version display string used in the UI
    /// </summary>
    public static string DisplayVersion => $"v{ApplicationVersion}";
}

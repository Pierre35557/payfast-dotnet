namespace Payfast.Common.Helpers
{
    /// <summary>
    /// Utility class for loading environment variables from a file.
    /// </summary>
    public class EnvironmentConfigLoader
    {
        /// <summary>
        /// Loads environment variables from a file and sets them in the current process.
        /// </summary>
        /// <param name="filePath">The path to the environment variable file.</param>
        public static void Load(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            foreach (var line in File.ReadAllLines(filePath))
            {
                var parts = line.Split(
                    '=',
                    StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 2)
                {
                    Environment.SetEnvironmentVariable(parts[0], string.Empty);
                    continue;
                }

                Environment.SetEnvironmentVariable(parts[0], parts[1]);
            }
        }
    }
}

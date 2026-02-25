using System.Diagnostics;

namespace Common.Helpers
{
    public static class BrowserHelper
    {
        public static void OpenUrlInBrowser(string url)
        {
            Console.WriteLine($"Attempting to open URL: '{url}'");
            try
            {
                // Use ProcessStartInfo to specify a non-interactive window if preferred
                // or to ensure the shell executes the command.
                var psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true // Crucial: Tells the OS to use the default program for the file/URI
                };

                // Process.Start returns a Process object, or null if no process was started.
                Process? p = Process.Start(psi);

                if (p != null)
                {
                    Console.WriteLine($"  Successfully launched process for URL. Process ID: {p.Id}");
                }
                else
                {
                    Console.WriteLine("  Failed to launch browser process (Process.Start returned null).");
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                // This exception often occurs if the URL is malformed, or if
                // there's no default browser configured, or permission issues.
                Console.Error.WriteLine($"  Error opening URL (Win32Exception - likely no default browser or malformed URL): {ex.Message}");
            }
            catch (PlatformNotSupportedException ex)
            {
                // This can occur if UseShellExecute is true on a platform that doesn't support it,
                // or if the URL format is problematic for the specific OS.
                Console.Error.WriteLine($"  Error opening URL (PlatformNotSupportedException): {ex.Message}");
                // Fallback for older .NET Core on non-Windows/macOS might involve specific commands:
                // if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                // {
                //     Process.Start("xdg-open", url);
                // }
                // else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                // {
                //     Process.Start("open", url);
                // }
            }
            catch (Exception ex)
            {
                // Catch any other unexpected exceptions
                Console.Error.WriteLine($"  An unexpected error occurred while opening URL: {ex.Message}");
            }
        }
    }
}

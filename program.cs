using System;
using System.IO;
using System.Drawing;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace WallpaperProtector
{
    class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
        
        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;
        
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        
        private static readonly string schoolWallpaper = @"C:\Windows\Web\Wallpaper\School\default.jpg";
        private static Thread monitorThread;
        private static bool monitorRunning = false;
        private static IntPtr consoleWindow;
        
        static void Main(string[] args)
        {
            consoleWindow = GetConsoleWindow();
            
            if (args.Length > 0 && args[0] == "/silent")
            {
                ShowWindow(consoleWindow, SW_HIDE);
                StartBackgroundMonitor();
                Thread.Sleep(Timeout.Infinite);
                return;
            }
            
            if (!IsRunningAsAdministrator())
            {
                RestartAsAdministrator();
                return;
            }
            
            Console.Title = "Wallpaper Protector";
            
            if (!monitorRunning)
            {
                StartBackgroundMonitor();
            }
            
            while (true)
            {
                Console.Clear();
                ShowHeader();
                ShowMenu();
                
                var key = Console.ReadKey(true).Key;
                
                switch (key)
                {
                    case ConsoleKey.D1:
                        LockWallpaper();
                        break;
                    case ConsoleKey.D2:
                        UnlockWallpaper();
                        break;
                    case ConsoleKey.D3:
                        SetCustomWallpaper();
                        break;
                    case ConsoleKey.D4:
                        CheckStatus();
                        break;
                    case ConsoleKey.D5:
                        CreateAutoStart();
                        break;
                    case ConsoleKey.X:
                    case ConsoleKey.Escape:
                        StopBackgroundMonitor();
                        return;
                }
            }
        }
        
        static bool IsRunningAsAdministrator()
        {
            using (var identity = System.Security.Principal.WindowsIdentity.GetCurrent())
            {
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
        }
        
        static void RestartAsAdministrator()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Environment.ProcessPath,
                UseShellExecute = true,
                Verb = "runas"
            };
            
            try
            {
                Process.Start(startInfo);
            }
            catch
            {
                Console.WriteLine("Admin access required");
                Console.ReadKey();
            }
            
            Environment.Exit(0);
        }
        
        static void StartBackgroundMonitor()
        {
            if (monitorRunning) return;
            
            monitorRunning = true;
            monitorThread = new Thread(MonitorLoop);
            monitorThread.IsBackground = true;
            monitorThread.Start();
        }
        
        static void StopBackgroundMonitor()
        {
            monitorRunning = false;
            if (monitorThread != null && monitorThread.IsAlive)
            {
                monitorThread.Join(1000);
            }
        }
        
        static void MonitorLoop()
        {
            while (monitorRunning)
            {
                try
                {
                    string currentWallpaper = GetCurrentWallpaper();
                    
                    if (currentWallpaper != schoolWallpaper && File.Exists(schoolWallpaper))
                    {
                        SetWallpaper(schoolWallpaper);
                    }
                }
                catch
                {
                }
                
                Thread.Sleep(2000);
            }
        }
        
        static void ShowHeader()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("     WALLPAPER PROTECTOR");
            Console.WriteLine("========================================");
            Console.WriteLine();
            
            Console.Write("Protection: ");
            Console.ForegroundColor = monitorRunning ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(monitorRunning ? "ACTIVE" : "INACTIVE");
            Console.ResetColor();
            
            Console.Write("Wallpaper: ");
            if (File.Exists(schoolWallpaper))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("SET");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("NOT SET");
            }
            Console.ResetColor();
            Console.WriteLine();
        }
        
        static void ShowMenu()
        {
            Console.WriteLine("MAIN MENU:");
            Console.WriteLine();
            
            Console.WriteLine("  1. Lock & Protect");
            Console.WriteLine("  2. Unlock & Remove");
            Console.WriteLine("  3. Set Custom Wallpaper");
            Console.WriteLine("  4. Check Status");
            Console.WriteLine("  5. Install Auto-Start");
            Console.WriteLine("  X. Exit");
            Console.WriteLine();
            
            Console.Write("Select: ");
        }
        
        static void LockWallpaper()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("ACTIVATING PROTECTION...");
                Console.WriteLine(new string('=', 50));
                Console.WriteLine();
                
                Directory.CreateDirectory(Path.GetDirectoryName(schoolWallpaper));
                
                if (!File.Exists(schoolWallpaper))
                {
                    CreateDefaultWallpaper();
                    Console.WriteLine("Created default wallpaper");
                }
                
                SetWallpaper(schoolWallpaper);
                Console.WriteLine("Wallpaper applied");
                
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System"))
                {
                    key.SetValue("NoDispBackgroundPage", 1, RegistryValueKind.DWord);
                }
                
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\ActiveDesktop"))
                {
                    key.SetValue("NoChangingWallPaper", 1, RegistryValueKind.DWord);
                }
                Console.WriteLine("Registry locked");
                
                if (!monitorRunning)
                {
                    StartBackgroundMonitor();
                    Console.WriteLine("Background monitor started");
                }
                
                CreateAutoStartEntry();
                Console.WriteLine("Auto-start configured");
                
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("PROTECTION ACTIVATED");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.ResetColor();
            }
            
            WaitForKey();
        }
        
        static void UnlockWallpaper()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("REMOVING PROTECTION...");
                Console.WriteLine(new string('=', 50));
                Console.WriteLine();
                
                StopBackgroundMonitor();
                Console.WriteLine("Background monitor stopped");
                
                // Διαγραφή κακών registry keys
                Console.WriteLine("Deleting registry policies...");
                
                // Διαγραφή συγκεκριμένων registry values
                try
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true))
                    {
                        if (key != null)
                        {
                            key.DeleteValue("NoDispBackgroundPage", false);
                            key.DeleteValue("NoDisplayBackground", false);
                        }
                    }
                }
                catch { }
                
                try
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\ActiveDesktop", true))
                    {
                        key?.DeleteValue("NoChangingWallPaper", false);
                    }
                }
                catch { }
                
                // Διαγραφή όλου του Policies φακέλου για το wallpaper
                try
                {
                    Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", false);
                }
                catch { }
                
                try
                {
                    Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Policies\ActiveDesktop", false);
                }
                catch { }
                
                Console.WriteLine("Registry unlocked");
                
                // Επαναφορά default ρυθμίσεων
                Console.WriteLine("Restoring default wallpaper settings...");
                
                using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
                {
                    if (key != null)
                    {
                        key.SetValue("Wallpaper", "", RegistryValueKind.String);
                        key.SetValue("WallpaperStyle", "10", RegistryValueKind.String);  // 10 = Fill
                        key.SetValue("TileWallpaper", "0", RegistryValueKind.String);
                    }
                }
                
                // Force update wallpaper
                Console.WriteLine("Updating wallpaper...");
                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, "", SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, null, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
                
                // Update user parameters
                try
                {
                    Process.Start("RUNDLL32.EXE", "USER32.DLL,UpdatePerUserSystemParameters 1, True")?.WaitForExit(1000);
                }
                catch { }
                
                // Restart Explorer
                Console.WriteLine("Restarting Explorer...");
                try
                {
                    Process.Start("taskkill", "/f /im explorer.exe")?.WaitForExit(2000);
                    Thread.Sleep(2000);
                    Process.Start("explorer.exe");
                    Console.WriteLine("Explorer restarted");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not restart Explorer: {ex.Message}");
                }
                
                // Άνοιγμα ρυθμίσεων
                try
                {
                    Process.Start("ms-settings:personalization-background");
                    Console.WriteLine("Opened wallpaper settings");
                }
                catch { }
                
                RemoveAutoStartEntry();
                Console.WriteLine("Auto-start removed");
                
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("PROTECTION REMOVED");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.ResetColor();
            }
            
            WaitForKey();
        }
        
        static void SetCustomWallpaper()
        {
            Console.Clear();
            Console.WriteLine("SET CUSTOM WALLPAPER");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine();
            
            Console.WriteLine("Enter image path:");
            Console.Write("> ");
            string imagePath = Console.ReadLine();
            imagePath = imagePath.Trim('"');
            
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No image selected");
                Console.ResetColor();
                WaitForKey();
                return;
            }
            
            if (!File.Exists(imagePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"File not found: {imagePath}");
                Console.ResetColor();
                WaitForKey();
                return;
            }
            
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(schoolWallpaper));
                File.Copy(imagePath, schoolWallpaper, true);
                Console.WriteLine($"Copied: {Path.GetFileName(imagePath)}");
                SetWallpaper(schoolWallpaper);
                Console.WriteLine("Wallpaper applied");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("CUSTOM WALLPAPER SET");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.ResetColor();
            }
            
            WaitForKey();
        }
        
        static void CheckStatus()
        {
            Console.Clear();
            Console.WriteLine("PROTECTION STATUS");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine();
            
            Console.Write("Background Monitor: ");
            Console.ForegroundColor = monitorRunning ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(monitorRunning ? "RUNNING" : "STOPPED");
            Console.ResetColor();
            
            Console.Write("Registry Protection: ");
            bool registryLocked = IsRegistryLocked();
            Console.ForegroundColor = registryLocked ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(registryLocked ? "ACTIVE" : "INACTIVE");
            Console.ResetColor();
            
            Console.Write("School Wallpaper: ");
            if (File.Exists(schoolWallpaper))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("EXISTS");
                Console.ResetColor();
                var info = new FileInfo(schoolWallpaper);
                Console.WriteLine($"  Size: {info.Length / 1024} KB");
                Console.WriteLine($"  Modified: {info.LastWriteTime:dd/MM/yyyy HH:mm}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("NOT FOUND");
                Console.ResetColor();
            }
            
            Console.Write("Auto-Start: ");
            if (AutoStartExists())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("CONFIGURED");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("NOT CONFIGURED");
            }
            Console.ResetColor();
            
            Console.WriteLine();
            Console.WriteLine(new string('=', 50));
            
            WaitForKey();
        }
        
        static void CreateAutoStart()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("CONFIGURING AUTO-START");
                Console.WriteLine(new string('=', 50));
                Console.WriteLine();
                
                CreateAutoStartEntry();
                Console.WriteLine("Added to Windows startup");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("AUTO-START CONFIGURED");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.ResetColor();
            }
            
            WaitForKey();
        }
        
        static string GetCurrentWallpaper()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop"))
                {
                    return key?.GetValue("WallPaper") as string ?? "";
                }
            }
            catch
            {
                return "";
            }
        }
        
        static bool IsRegistryLocked()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Policies\System"))
                {
                    var value = key?.GetValue("NoDispBackgroundPage");
                    return value != null && (int)value == 1;
                }
            }
            catch
            {
                return false;
            }
        }
        
        static void CreateDefaultWallpaper()
        {
            using (Bitmap bitmap = new Bitmap(1920, 1080))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.DarkBlue);
                using (Font font = new Font("Arial", 48, FontStyle.Bold))
                using (Brush brush = new SolidBrush(Color.White))
                {
                    string text = "SCHOOL COMPUTER";
                    SizeF textSize = graphics.MeasureString(text, font);
                    float x = (bitmap.Width - textSize.Width) / 2;
                    float y = (bitmap.Height - textSize.Height) / 2;
                    graphics.DrawString(text, font, brush, x, y);
                }
                bitmap.Save(schoolWallpaper, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }
        
        static void RestoreDefaultWallpaper()
        {
            string[] defaults = {
                @"C:\Windows\Web\Wallpaper\Windows\img0.jpg",
                @"C:\Windows\Web\Wallpaper\Windows\img1.jpg",
                @"C:\Windows\Web\4K\Wallpaper\Windows\img0.jpg"
            };
            
            foreach (string wallpaper in defaults)
            {
                if (File.Exists(wallpaper))
                {
                    SetWallpaper(wallpaper);
                    Console.WriteLine($"Restored: {Path.GetFileName(wallpaper)}");
                    return;
                }
            }
            
            SetWallpaper("");
            Console.WriteLine("Set to solid color");
        }
        
        static void CreateAutoStartEntry()
        {
            string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupPath, "WallpaperProtector.lnk");
            
            string vbsPath = Path.Combine(Path.GetTempPath(), "create_shortcut.vbs");
            string vbsContent = 
                "Set oWS = WScript.CreateObject(\"WScript.Shell\")\r\n" +
                "sLinkFile = \"" + shortcutPath.Replace("\\", "\\\\") + "\"\r\n" +
                "Set oLink = oWS.CreateShortcut(sLinkFile)\r\n" +
                "oLink.TargetPath = \"" + Environment.ProcessPath.Replace("\\", "\\\\") + "\"\r\n" +
                "oLink.Arguments = \"/silent\"\r\n" +
                "oLink.WindowStyle = 7\r\n" +
                "oLink.Save\r\n";
            
            File.WriteAllText(vbsPath, vbsContent);
            Process.Start("wscript.exe", "\"" + vbsPath + "\"").WaitForExit();
            File.Delete(vbsPath);
        }
        
        static void RemoveAutoStartEntry()
        {
            string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupPath, "WallpaperProtector.lnk");
            
            if (File.Exists(shortcutPath))
            {
                File.Delete(shortcutPath);
            }
        }
        
        static bool AutoStartExists()
        {
            string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupPath, "WallpaperProtector.lnk");
            return File.Exists(shortcutPath);
        }
        
        static void SetWallpaper(string path)
        {
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
        
        static void WaitForKey()
        {
            Console.WriteLine();
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }
    }
}
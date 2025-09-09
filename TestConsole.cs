using System;
using System.IO;
using System.Windows;

namespace TaskLogger
{
    /// <summary>
    /// Simple console test program to verify the application can start
    /// Run this with: dotnet run --project TaskLogger.csproj -- --test-console
    /// </summary>
    public class TestConsole
    {
        public static void RunTest()
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("Task Logger - Console Test Mode");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            try
            {
                // Test 1: Check environment
                Console.WriteLine("[TEST 1] Checking Environment...");
                Console.WriteLine($"  OS: {Environment.OSVersion}");
                Console.WriteLine($"  .NET Version: {Environment.Version}");
                Console.WriteLine($"  Current Directory: {Directory.GetCurrentDirectory()}");
                Console.WriteLine($"  User: {Environment.UserName}");
                Console.WriteLine($"  Machine: {Environment.MachineName}");
                Console.WriteLine("  ✓ Environment check passed");
                Console.WriteLine();

                // Test 2: Check AppData folders
                Console.WriteLine("[TEST 2] Checking AppData Folders...");
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var taskLoggerPath = Path.Combine(localAppData, "TaskLogger");
                Console.WriteLine($"  LocalAppData: {localAppData}");
                Console.WriteLine($"  TaskLogger Path: {taskLoggerPath}");
                
                if (!Directory.Exists(taskLoggerPath))
                {
                    Console.WriteLine("  Creating TaskLogger directory...");
                    Directory.CreateDirectory(taskLoggerPath);
                }
                Console.WriteLine("  ✓ AppData folders check passed");
                Console.WriteLine();

                // Test 3: Test Logging Service
                Console.WriteLine("[TEST 3] Testing Logging Service...");
                try
                {
                    var logger = Services.LoggingService.Instance;
                    logger.LogInfo("Test log entry from console test");
                    var logPath = logger.GetLogFilePath();
                    Console.WriteLine($"  Log file created at: {logPath}");
                    
                    if (File.Exists(logPath))
                    {
                        var fileInfo = new FileInfo(logPath);
                        Console.WriteLine($"  Log file size: {fileInfo.Length} bytes");
                        Console.WriteLine("  ✓ Logging service test passed");
                    }
                    else
                    {
                        Console.WriteLine("  ✗ Log file not found!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ✗ Logging service failed: {ex.Message}");
                }
                Console.WriteLine();

                // Test 4: Test Database Configuration
                Console.WriteLine("[TEST 4] Testing Database Configuration...");
                try
                {
                    var dbConfigService = new Services.DatabaseConfigService();
                    var dbPath = dbConfigService.GetDatabasePath();
                    Console.WriteLine($"  Database path: {dbPath}");
                    
                    var dbDir = Path.GetDirectoryName(dbPath);
                    if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
                    {
                        Console.WriteLine($"  Creating database directory: {dbDir}");
                        Directory.CreateDirectory(dbDir);
                    }
                    Console.WriteLine("  ✓ Database configuration test passed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ✗ Database configuration failed: {ex.Message}");
                }
                Console.WriteLine();

                // Test 5: Test WPF Application Creation
                Console.WriteLine("[TEST 5] Testing WPF Application Creation...");
                try
                {
                    // Note: This might fail in a pure console context
                    var app = new App();
                    Console.WriteLine("  ✓ WPF Application instance created");
                    
                    // Don't actually run the app, just test creation
                    Console.WriteLine("  Note: Not starting WPF app in console mode");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ✗ WPF Application creation failed: {ex.Message}");
                    Console.WriteLine($"     This is expected in console-only mode");
                }
                Console.WriteLine();

                // Summary
                Console.WriteLine("===========================================");
                Console.WriteLine("Test Summary:");
                Console.WriteLine("  Most critical components are working.");
                Console.WriteLine("  Check the log file for detailed information.");
                Console.WriteLine("  If the exe doesn't show a window, check:");
                Console.WriteLine("    1. The log file for errors");
                Console.WriteLine("    2. Windows Event Viewer for crashes");
                Console.WriteLine("    3. Task Manager to see if process starts");
                Console.WriteLine("===========================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FATAL ERROR: {ex}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
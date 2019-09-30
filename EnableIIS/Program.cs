using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EnableIIS
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                SetupIIS();
                Console.WriteLine("Done. Press any key to close.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred:" + ex.Message);
            }

            Console.ReadLine();
        }

        static string SetupIIS()
        {
            /*
Feature Name : IIS-WebServerRole
Feature Name : IIS-WebServer
Feature Name : IIS-CommonHttpFeatures
Feature Name : IIS-HttpErrors
Feature Name : IIS-HttpRedirect
Feature Name : IIS-ApplicationDevelopment
Feature Name : IIS-NetFxExtensibility
Feature Name : IIS-NetFxExtensibility45
Feature Name : IIS-HealthAndDiagnostics
Feature Name : IIS-HttpLogging
Feature Name : IIS-LoggingLibraries
Feature Name : IIS-RequestMonitor
Feature Name : IIS-HttpTracing
Feature Name : IIS-Security
Feature Name : IIS-URLAuthorization
Feature Name : IIS-RequestFiltering
Feature Name : IIS-IPSecurity
Feature Name : IIS-Performance
Feature Name : IIS-HttpCompressionDynamic
Feature Name : IIS-WebServerManagementTools
Feature Name : IIS-ManagementScriptingTools
Feature Name : IIS-IIS6ManagementCompatibility
Feature Name : IIS-Metabase
Feature Name : IIS-HostableWebCore
Feature Name : IIS-CertProvider
Feature Name : IIS-WindowsAuthentication
Feature Name : IIS-DigestAuthentication
Feature Name : IIS-ClientCertificateMappingAuthentication
Feature Name : IIS-IISCertificateMappingAuthentication
Feature Name : IIS-ODBCLogging
Feature Name : IIS-StaticContent
Feature Name : IIS-DefaultDocument
Feature Name : IIS-DirectoryBrowsing
Feature Name : IIS-WebDAV
Feature Name : IIS-WebSockets
Feature Name : IIS-ApplicationInit
Feature Name : IIS-ASPNET
Feature Name : IIS-ASPNET45
Feature Name : IIS-ASP
Feature Name : IIS-CGI
Feature Name : IIS-ISAPIExtensions
Feature Name : IIS-ISAPIFilter
Feature Name : IIS-ServerSideIncludes
Feature Name : IIS-CustomLogging
Feature Name : IIS-BasicAuthentication
Feature Name : IIS-HttpCompressionStatic
Feature Name : IIS-ManagementConsole
Feature Name : IIS-ManagementService
Feature Name : IIS-WMICompatibility
Feature Name : IIS-LegacyScripts
Feature Name : IIS-LegacySnapIn
Feature Name : IIS-FTPServer
Feature Name : IIS-FTPSvc
Feature Name : IIS-FTPExtensibility
             */
            // In command prompt run this command to see all the features names which are equivalent to UI features.
            // c:\>dism /online /get-features /format:table 
            var featureNames = new List<string>
            {
                //Application Development
                "IIS-ApplicationDevelopment",
                "IIS-ISAPIExtensions",
                "IIS-ISAPIFilter",
                //"IIS-ServerSideIncludes",
                "IIS-ApplicationInit",
                "IIS-WebSockets",
                //Common HTTP Features*
                "IIS-CommonHttpFeatures",
                "IIS-DefaultDocument",
                "IIS-DirectoryBrowsing",
                "IIS-HttpErrors",
                "IIS-StaticContent",
                "IIS-HttpRedirect",
                //Health and Diagnostics*
                "IIS-HealthAndDiagnostics",
                "IIS-HttpLogging",
                "IIS-CustomLogging",
                "IIS-HttpTracing",
                //Web Server*	
                "IIS-WebServer",
                //Web Server Role
                "IIS-WebServerRole",
                //Management Tools*
                "IIS-ManagementConsole",
                //Performance*
                "IIS-HttpCompressionDynamic",
                "IIS-HttpCompressionStatic",
                //Security*
                "IIS-Security",
                "IIS-RequestFiltering",
                "IIS-CertProvider",
                "IIS-IPSecurity",
                "IIS-BasicAuthentication",
                "IIS-URLAuthorization",
                "IIS-WindowsAuthentication"
            };

            Console.WriteLine("Checking the Operating System...\n");

            ManagementObjectSearcher obj = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
            try
            {
                foreach (ManagementObject wmi in obj.Get())
                {
                    string Name = wmi.GetPropertyValue("Caption").ToString();

                    // Remove all non-alphanumeric characters so that only letters, numbers, and spaces are left.
                    // Imp. for 32 bit window server 2008
                    Name = Regex.Replace(Name.ToString(), "[^A-Za-z0-9 ]", "");

                    if (Name.Contains("Server 2019") ||
                        Name.Contains("Server 2016") ||
                        Name.Contains("Server 2012 R2") ||
                        Name.Contains("Server 2012") ||
                        Name.Contains("Windows 8") ||
                        Name.Contains("Windows 81") ||
                        Name.Contains("Windows 10"))
                    {
                        featureNames.Add("IIS-ASPNET45");
                        featureNames.Add("IIS-NetFxExtensibility45");
                        featureNames.Add("NetFx4Extended-ASPNET45");
                    }
                    else if (Name.Contains("Server 2008 R2") || Name.Contains("Windows 7"))
                    {
                        featureNames.Add("IIS-ASPNET");
                        featureNames.Add("IIS-NetFxExtensibility");
                        featureNames.Add("NetFx4Extended-ASPNET45");
                    }
                    else
                    {
                        featureNames.Clear();
                    }

                    string Version = (string) wmi["Version"];
                    string Architecture = (string) wmi["OSArchitecture"];

                    Console.WriteLine("Operating System details:");
                    Console.WriteLine("OS Name: " + Name);
                    Console.WriteLine("Version: " + Version);
                    Console.WriteLine("Architecture: " + Architecture + "\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred:" + ex.Message);
            }

            return Run(
                "dism",
                string.Format(
                    "/NoRestart /Online /Enable-Feature {0}",
                    string.Join(
                        " ",
                        featureNames.Select(name => string.Format("/FeatureName:{0}", name)))));
        }

        static string Run(string fileName, string arguments)
        {
            Console.WriteLine("Enabling IIS features...");
            Console.WriteLine(arguments);

            using (var process = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            }))
            {
                //process.WaitForExit();
                //return process.StandardOutput.ReadToEnd();

                string results = "";
                while (process != null && !process.StandardOutput.EndOfStream)
                {
                    var msg = process.StandardOutput.ReadLine();
                    Console.WriteLine("installing status: {0}",msg);
                    results += msg;
                }

                return results;
            }
        }
    }
}

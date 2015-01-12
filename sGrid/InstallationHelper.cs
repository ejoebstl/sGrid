using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Boinc;
using System.Reflection;
using Microsoft.Win32;
using System.Security.Cryptography;

namespace sGrid
{
    class InstallationHelper
    {        
        //Constant path literals. 
        private const string BoincPath = "Boinc\\boinc.exe";

        //Constant registry literals. 
        private const string BoincRegistryPath32 = "SOFTWARE\\Wow6432Node\\Space Sciences Laboratory, U.C. Berkeley\\BOINC Setup";
        private const string BoincRegistryPath64 = "SOFTWARE\\Space Sciences Laboratory, U.C. Berkeley\\BOINC Setup";
        private const string BoincDataDirKey = "DATADIR";

        //The executable directory of the current process.
        private string executableDirectory;

        public bool BoincInstallationDetected
        {
            get
            {
                return BoincDataDirInternal != null;
            }
        }

        private string BoincDataDirInternal
        {
            get
            {
                RegistryKey key32 = Registry.LocalMachine.OpenSubKey(BoincRegistryPath32);
                RegistryKey key64 = Registry.LocalMachine.OpenSubKey(BoincRegistryPath64);

                if (key32 != null)
                {
                    return (string)key32.GetValue(BoincDataDirKey);
                }
                if (key64 != null)
                {
                    return (string)key64.GetValue(BoincDataDirKey);
                }

                return null;
            }
        }

        public string BoincDataDir
        {
            get
            {
                string dir = BoincDataDirInternal;

                if (dir == null)
                {
                    dir = executableDirectory;
                }

                return dir;
            }
        }

        private bool BoincPasswordExists
        {
            get
            {
                return File.Exists(BoincClient.BoincGuiRpcAuthFileLocation);
            }
        }

        public InstallationHelper()
        {
            executableDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public void ConfigureBoincPaths()
        {
            BoincClient.BoincExecutableLocation = Path.Combine(executableDirectory, BoincPath);
            BoincClient.BoincGuiRpcAuthFileLocation = Path.Combine(BoincDataDir, BoincClient.GuiRpcAuthFile);
            BoincClient.BoincWorkingDirectory = BoincDataDir;

            if (!BoincPasswordExists)
            {
                InitialPasswordSetup();
            }
        }

        /// <summary>
        /// Generates a password for Boinc.
        /// </summary>
        private void InitialPasswordSetup()
        {
            RandomNumberGenerator rng = RNGCryptoServiceProvider.Create();

            byte[] random = new byte[32];
            rng.GetBytes(random);

            string password = Convert.ToBase64String(random);
            password = password.Substring(0, password.Length - 2);

            File.WriteAllText(BoincClient.BoincGuiRpcAuthFileLocation, password);
        }
    }
}

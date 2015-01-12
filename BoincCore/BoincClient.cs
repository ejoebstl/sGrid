// This source file is part of BoincGuiRpc.Net
//
// Author: 	    Emanuel Jöbstl <emi@eex-dev.net>
// Weblink: 	http://boincguirpc.codeplex.com
//
// Licensed under the MIT License
//
// (c) 2012-2013

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Xml;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using System.Globalization;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;

namespace Boinc
{
    /// <summary>
    /// This class wraps the remote procedure call interface of Boinc and 
    /// provides ways to change configuration or to gather information 
    /// from the Boinc application. 
    /// </summary>
    /// <remarks>
    /// Notes on the Boinc Xml parser: 
    /// * Boinc uses ASCII 0x03 (End-Of-Text) after each message, which has to be filtered
    ///   out on receiving, or .NET's XmlReader will crash. 
    /// * Boinc stores booleans in Xml the following way: If an node 
    ///   with the corresponding name exists, the value is true, otherwise false. 
    /// * Boinc does not handle whitespace in self-ending Xml tags well, use a seperate
    ///   start and end tag instead. 
    ///   
    /// Notes on the parsing in this class:
    /// * This class uses Lambda expressions to recursively parse Xml. For each method, the following
    ///   scheme applies: First, read the start element with the given name, then, execute the given action, 
    ///   finally, read the end element with the given name. 
    /// </remarks>
    /// <example>
    ///   The expression
    ///   <code>
    ///     ReadBoincRequest(() => { ReadElement("Test", () => { ReadValue(); }); });
    ///   </code>  
    ///   reads Xml with the structure 
    ///   <code>
    ///   <![CDATA[ 
    ///     <boinc_gui_rpc_request>
    ///        <test>
    ///             Value
    ///         </test>
    ///      </boinc_gui_rpc_request>
    ///   ]]>
    ///   </code>
    /// </example>
    public class BoincClient
    {
        //Connection information.

        /// <summary>
        /// The TCP Port where the Boinc client is listening. 
        /// </summary>
        private int port; 

        /// <summary>
        /// The hostname or address of the Boinc client as string. 
        /// </summary>
        private string host;

        /// <summary>
        /// The password of the Boinc client. 
        /// </summary>
        private string password;

        //Connection and stream helpers
        private Socket connection;
        private XmlWriter writer;
        private XmlReader reader;
        private FilterStream filterStream;
        private NetworkStream netStream;

        // XML tag literals used for Boinc RPC
        #region XML tag literals  
        private const string StatusTag = "status";
        private const string SuccessTag = "success";
        private const string AuthorizedTag = "authorized";

        private const string SetGlobalPrefsOverrideTag = "set_global_prefs_override";
        private const string ReadGlobalPrefsOverrideTag = "read_global_prefs_override";
        private const string RunOnBatteriesTag = "run_on_batteries";
        private const string CpuCountLimitTag = "max_cpus";
        private const string CpuCountPercentageTag = "max_ncpus_pct";
        private const string DiskMaxUsedTag = "disk_max_used_gb";
        private const string RunGpuIfUserIsActiveTag = "run_gpu_if_user_active";
        private const string CpuUsageLimitTag = "cpu_usage_limit";

        private const string SetGlobalPrefsOverrideReplyTag = "set_global_prefs_override_reply";

        private const string GetGlobalPrefsOverrideTag = "get_global_prefs_override";
        private const string GlobalPrefsTag = "global_preferences";

        private const string GetProxySettingsTag = "get_proxy_settings";
        private const string ProxyInfoTag = "proxy_info";

        private const string HttpServerNameTag = "http_server_name";
        private const string HttpServerPortTag = "http_server_port";
        private const string HttpUserNameTag = "http_user_name";
        private const string HttpPasswordTag = "http_user_passwd";

        private const string UseHttpProxyTag = "use_http_proxy";
        private const string UseSocksProxyTag = "use_socks_proxy";
        private const string UseHttpAuthTag = "use_http_auth";
        private const string NoProxyTag = "noproxy";

        private const string SetProxySettingsTag = "set_proxy_settings";

        private const string ProjectAttachTag = "project_attach";
        private const string ProjectUrlTag = "project_url";
        private const string AuthenticatorTag = "authenticator";
        private const string ProjectNameTag = "project_name";
        
        private const string Auth1Tag = "auth1";
        private const string Auth2Tag = "auth2";
        private const string NonceTag = "nonce";
        private const string NonceHashTag = "nonce_hash";

        private const string GetProjectStatusTag = "get_project_status";
        private const string ProjectsTag = "projects";
        private const string ProjectTag = "project";
        private const string MasterUrlTag = "master_url";
        private const string DontRequestMoreWorkTag = "dont_request_more_work";
        private const string SuspendedViaGuiTag = "suspended_via_gui";
        private const string ProjectEndedTag = "ended";

        private const string GetDiskUsageTag = "get_disk_usage";
        private const string DiskUsageSummaryTag = "disk_usage_summary";
        private const string DiskUsageTag = "disk_usage";

        private const string GetResultsTag = "get_results";
        private const string ActiveOnlyTag = "active_only";

        private const string ResultsTag = "results";
        private const string ResultTag = "result";

        private const string NameTag = "name";
        private const string ResourcesTag = "resources";
        private const string WorkUnitNameTag = "wu_name";
        private const string ExitStatusTag = "exit_status";
        private const string PidTag = "pid";
        private const string SignalTag = "signal";
        private const string StateTag = "state";
        private const string VersionNumTag = "version_num";
        private const string ActiveTaskStateTag = "active_task_state";
        private const string CurrentCpuTimeTag = "current_cpu_time";
        private const string ElapsedTimeTag = "elapsed_time";
        private const string EstimatedCpuTimeRemainingTag = "estimated_cpu_time_remaining";
        private const string FinalCpuTimeTag = "final_cpu_time";
        private const string FinalElapsedTimeTag = "final_elapsed_time";
        private const string ReceivedTimeTag = "received_time";
        private const string ReportDeadlineTag = "report_deadline";
        private const string FractionDoneTag = "fraction_done";
        private const string ServerAckTag = "got_server_ack";
        private const string ReadyToReportTag = "ready_to_report";
        private const string ActiveTaskTag = "active_task";

        private const string ExchangeVersionsTag = "exchange_versions";
        private const string ServerVersionTag = "server_version";
        private const string MajorTag = "major";
        private const string MinorTag = "minor";
        private const string ReleaseTag = "release";

        private const string QuitTag = "quit";
        #endregion

        /// <summary>
        /// The name of the boinc configuration directory
        /// </summary>
        private const string boincDir = "BOINC";
        /// <summary>
        /// The name of the boinc executable file
        /// </summary>
        public const string BoincExecutable = "boinc.exe"; 
        /// <summary>
        /// The name of the boinc gui rpc auth configuration file, which holds the password for rpc authentication
        /// </summary>
        public const string GuiRpcAuthFile = "gui_rpc_auth.cfg";

        /// <summary>
        /// Gets or sets the location of the Boinc main executable (boinc.exe). This property is
        /// only relevant when Boinc is automatically started, or StartClient() or IsRunning is called. 
        /// </summary>
        public static string BoincExecutableLocation {get; set;}

        /// <summary>
        /// Gets or seths the location of the boinc gui rpc authentication file, which contains the password.
        /// This property is only relevant when the passwort is automatically retreived. 
        /// </summary>
        public static string BoincGuiRpcAuthFileLocation { get; set; }

        /// <summary>
        /// Gets or sets a boolean which indicates whether boinc's command window should be hidden. 
        /// </summary>
        public static bool HideBoincWindow { get; set; }

        /// <summary>
        /// Gets or sets the working directory of Boinc. Null means default. 
        /// </summary>
        public static string BoincWorkingDirectory { get; set; }



        /// <summary>
        /// An empty delegate, so we can use Lambda patterns for XML parsing
        /// </summary>
        private delegate void EmptyDelegate();

        /// <summary>
        /// Gets or sets the HTTP proxy settings. If the proxy settings are set, 
        /// the proxy settings of the underlying client are automatically changed. 
        /// </summary>
        public ProxySettings ProxySettings
        {
            get { return GetProxySettings(); }
            set { SetProxySettings(value); }
        }

        /// <summary>
        /// Gets a boolean indicating whether this BoincClient is connected to Boinc. 
        /// </summary>
        public bool IsConnected
        {
            get { return connection != null; }
        }

        /// <summary>
        /// Gets a bool indicating whether Boinc is already running on the local host. 
        /// </summary>
        public bool BoincIsRunning
        {
            get
            {
                Process boinc = GetBoincProcess();

                return boinc != null;
            }
        }

        /// <summary>
        /// Static constructor, used to initialize BoincGuiRpcAuthFileLocation and BoincExecutableLocation.
        /// </summary>
        static BoincClient()
        {
            //The password should be located in a file in the application data directory.
            string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData); 
            BoincGuiRpcAuthFileLocation =  Path.Combine(appDataDir, Path.Combine(boincDir, GuiRpcAuthFile));

            //The executable is located in the program files directory by default. 
            string programFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            BoincExecutableLocation = Path.Combine(Path.Combine(programFilesDir, boincDir), BoincExecutable);

            HideBoincWindow = true;
        }

        /// <summary>
        /// Creates a new instance of this class associated with the connection 
        /// specified by the given parameters.
        /// </summary>
        /// <param name="host">The host to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        /// <param name="password">The Boinc application’s password. 
        /// If this parameter is set to null and the host is localhost, the password is automatically retreived
        /// from the boinc configuration file.</param>
        /// <param name="autoStartBoinc">A bool indicating whether Boinc should automatically be started on the local host, 
        /// if it is not already running.</param>
        public BoincClient(string host = "localhost", int port = 31416, string password = null, bool autoStartBoinc = false)
        {
            //Store params
            this.host = host;
            this.port = port;

            //If the password was given by the user, use it. 
            if (password != null)
            {
                this.password = password;
            }
            else
            {
                //If not, try to retreive it from the boinc gui rpc auth file. 

                //First, check whether we want to connect to the localhost...
                bool isLocalhost = false;

                foreach(IPAddress addr in Dns.GetHostAddresses(host)) {
                    if(addr.Equals(IPAddress.Loopback)) {
                        isLocalhost = true;
                        break;
                    }
                }

                if(!isLocalhost) 
                {
                    //..if not, fail. 
                    throw new InvalidOperationException("Cannot load password for remote clients. Please supply password");
                }

                //Then try to load the password. 
                try
                {
                    this.password = File.ReadAllText(BoincGuiRpcAuthFileLocation);
                }
                catch (Exception ex)
                {
                    throw new BoincApiException("Could not load password from Boinc gui_rpc_auth.cfg file", ex);
                }
            }

            //We should start Boinc.
            if (autoStartBoinc)
            {
                if (!BoincIsRunning)
                {
                    StartClient();
                }
            }

        }

        /// <summary>
        /// Starts Boinc on the local host.  
        /// </summary>
        /// <remarks>
        /// This method throws an exception if boinc is already started. 
        /// </remarks>
        public void StartClient()
        {
            if (BoincIsRunning)
            {
                throw new InvalidOperationException("Boinc is already running.");
            }

            //If Boinc is not running, we should start it, using parameters which indicate that our application
            //is responsible for managing Boinc. 
            ProcessStartInfo processStartInfo;

            if (HideBoincWindow)
            {
                processStartInfo = new ProcessStartInfo(BoincExecutableLocation, " --allow_remote_gui_rpc --detach_console --launched_by_manager");
            }
            else
            {
                processStartInfo = new ProcessStartInfo(BoincExecutableLocation);
            }

            //The CLI-Window of Boinc should be supressed. 

            if (HideBoincWindow)
            {
                processStartInfo.CreateNoWindow = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }

            if (BoincWorkingDirectory != null)
            {
                processStartInfo.WorkingDirectory = BoincWorkingDirectory;
            }

            Process boinc = Process.Start(processStartInfo);
        }


        /// <summary>
        /// Applies the given override preferences to Boinc.
        /// <remarks>This function automatically calls RefreshPreferences()</remarks>
        /// </summary>
        /// <param name="prefs">The preferences to apply.</param>
        public void SetPreferences(Preferences prefs)
        {
            CheckConnection();

            //Write override preferences to Boinc. 
            WriteRequest(() =>
            {
                WriteElement(SetGlobalPrefsOverrideTag, () =>
                {
                    WriteElement(GlobalPrefsTag, () =>
                    {
                        WriteElement(RunOnBatteriesTag, () =>
                        {
                            Write(prefs.RunOnBatteries ? "1" : "0");
                        });

                        WriteElement(RunGpuIfUserIsActiveTag, () =>
                        {
                            Write(prefs.UseGpuIfUserIsActive ? "1" : "0");
                        });

                        WriteElement(CpuUsageLimitTag, () =>
                        {
                            Write(((int)prefs.CpuUsageLimit).ToString());
                        });

                        WriteElement(DiskMaxUsedTag, () =>
                        {
                            Write(prefs.DiskUsageLimit.ToString());
                        });

                        WriteElement(CpuCountLimitTag, () =>
                        {
                            Write((prefs.CpuCountLimit).ToString());
                        });

                        WriteElement(CpuCountPercentageTag, () =>
                        {
                            Write((prefs.CpuCountPercentage).ToString());
                        });
                    });
                });
            });

            //Read Boinc's reply and check if there was an error. 
            ReadReply(() =>
            {
                ReadElement(SetGlobalPrefsOverrideReplyTag, () =>
                {
                    ReadElement(StatusTag, () =>
                    {
                        if (ReadIntAndExitNode() != 0)
                        {
                            throw new BoincApiException("Boinc returned an error when setting the preferences.");
                        }
                    }, false);
                });
            });

            RefreshPreferences();
        }

        /// <summary>
        /// Causes Boinc to re-read the local global preferences override file. 
        /// </summary>
        public void RefreshPreferences()
        {
            //Write the 
            WriteRequest(() =>
            {
                WriteElement(ReadGlobalPrefsOverrideTag);
            });

             //Read Boinc's reply and ensure success. 
            ReadReply(() =>
            {
                ReadElement(SuccessTag);
            });
        }

        /// <summary>
        /// Returns the override preferences of Boinc.
        /// If no override preferences exist, the default
        /// preferences are returned. 
        /// </summary>
        /// <returns>The Boinc override preferences.</returns>
        public Preferences GetPreferences()
        {
            CheckConnection();

            //Initialize with boinc defaults
            bool useGpu = true;
            bool runOnBattery = true;
            double cpuUsageLimit = 100;
            int cpuCountLimit = 0;
            double diskUsageLimit = 0;

            //Get the global preferences
            WriteRpc(GetGlobalPrefsOverrideTag);

            ReadReply(() =>
            {
                //Iterate over the received information and keep what is useful. 
                while (reader.Read() && reader.Name != BoincHelperExtensions.GuiRpcReplyTag)
                {
                    switch (reader.Name)
                    {
                        case RunOnBatteriesTag: runOnBattery = ReadIntAndExitNode() == 1; break;
                        case RunGpuIfUserIsActiveTag: useGpu = ReadIntAndExitNode() == 1; break;
                        case CpuUsageLimitTag: cpuUsageLimit = ReadDoubleAndExitNode(); break;
                        case DiskMaxUsedTag: diskUsageLimit = ReadDoubleAndExitNode(); break;
                        case CpuCountLimitTag: cpuCountLimit = ReadIntAndExitNode(); break;
                    }
                }
            }, false);


            return new Preferences(runOnBattery, useGpu, (float)cpuUsageLimit, (float)diskUsageLimit, (int)(cpuCountLimit));
        }

        /// <summary>
        /// Gets the HTTP proxy settings of Boinc. 
        /// </summary>
        /// <returns>The HTTP proxy settings or null, if no proxy is set.</returns>
        public ProxySettings GetProxySettings()
        {
            CheckConnection();

            string password = "";
            string username = "";
            string hostname = "";
            string port = "";
            bool useProxy = false;
            bool useAuth = false;

            //Get the proxy settings
            WriteRpc(GetProxySettingsTag);

            ReadReply(() =>
            {
                ReadElement(ProxyInfoTag, () =>
                {
                    //Iterate over the received proxy info and keep what is useful. 
                    while (reader.Read() && reader.Name != ProxyInfoTag)
                    {
                        switch (reader.Name)
                        {
                            case HttpServerNameTag: hostname = ReadValueAndExitNode(); break;
                            case HttpServerPortTag: port = ReadValueAndExitNode(); break;
                            case HttpUserNameTag: username = ReadValueAndExitNode(); break;
                            case HttpPasswordTag: password = ReadValueAndExitNode(); break;
                            case UseHttpProxyTag: useProxy = true; ExitNode(); break;
                            case UseHttpAuthTag: useAuth = true; ExitNode(); break;
                        }
                    }
                }, false);
            });

            //If we did not find tags for proxy or auth, 
            //return null. 
            if (useProxy)
            {
                if (useAuth)
                {
                    return new ProxySettings(password, hostname + ":" + port, username);
                }
                else
                {
                    return new ProxySettings("", hostname + ":" + port, "");
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sets the HTTP proxy settings of Boinc.
        /// </summary>
        /// <param name="proxy">The HTTP proxy settings to set, or null to erase the settings.</param>
        public void SetProxySettings(ProxySettings proxy)
        {
            CheckConnection();

            string password = "";
            string username = "";
            string hostname = "";
            string port = "";
            bool useProxy = false;
            bool useAuth = false;

            //First, prepare the proxy settings values
            if (proxy != null)
            {
                password = proxy.Password;
                username = proxy.Username;

                useAuth = username != "";
                useProxy = true;

                int i = proxy.ProxyServer.IndexOf(':');

                //Split port and hostname
                if (i != -1)
                {
                    hostname = proxy.ProxyServer.Substring(0, i);
                    port = proxy.ProxyServer.Substring(i);
                }
                else
                {
                    hostname = proxy.ProxyServer;
                }
            }

            //Since we don't want to dismiss other proxy settings, like SOCKS proxy,
            //we have to get the proxy settings here and then write them back, but with changing
            //all HTTP parts. 

            //Gets settings and write reply simultaneously. 
            WriteRpc(GetProxySettingsTag);

            WriteRequest(() =>
            {
                ReadReply(() =>
                {
                    WriteElement(SetProxySettingsTag, () =>
                    {
                        //Read the proxy info tag. The method should not throw when reading an unexpected tag, since 
                        //we cannot predict the inner XML and don't know when to stop reading.  
                        ReadElement(ProxyInfoTag, () =>
                        {
                            WriteElement(ProxyInfoTag, () =>
                            {
                                //Read over settings and write back non-HTTP settings. 
                                while (reader.Read() && reader.Name != ProxyInfoTag)
                                {
                                    System.Diagnostics.Debug.WriteLine(reader.Name);

                                    //HTTP settings should be replaced. 
                                    switch (reader.Name)
                                    {
                                        case HttpServerNameTag: WriteElement(reader.Name, () => { Write(hostname); ReadValueAndExitNode(); }); break;
                                        case HttpServerPortTag: WriteElement(reader.Name, () => { Write(port); ReadValueAndExitNode(); }); break;
                                        case HttpUserNameTag: WriteElement(reader.Name, () => { Write(username); ReadValueAndExitNode(); }); break;
                                        case HttpPasswordTag: WriteElement(reader.Name, () => { Write(password); ReadValueAndExitNode(); }); break;
                                        case NoProxyTag: WriteElement(reader.Name); ReadValueAndExitNode(); break;
                                        case UseHttpProxyTag: if (useProxy) { WriteElement(reader.Name); }; ExitNode(); break;
                                        case UseSocksProxyTag: WriteElement(reader.Name); ExitNode(); break;
                                        case UseHttpAuthTag: if (useAuth) { WriteElement(reader.Name); }; ExitNode(); break;
                                        default: WriteElement(reader.Name, () => { Write(ReadValueAndExitNode()); }); break;
                                    }
                                }
                            });

                        }, false);
                    });
                });
            });

            //Read the reply and ensure success. 
            ReadReply(() =>
            {
                ReadElement(SuccessTag);
            });
        }

        /// <summary>
        /// Attaches the client to a new project.
        /// </summary>
        /// <param name="url">The url of the project to attach to.</param>
        /// <param name="accountKey">The account key of the user.</param>
        /// <param name="projectName">The name of the project to attach to.</param>
        public void AttachToProject(string url, string accountKey, string projectName)
        {
            CheckConnection();

            //Write thr project attach request using the given parameters. 
            WriteRequest(() =>
            {
                WriteElement(ProjectAttachTag, () =>
                {
                    WriteElement(ProjectUrlTag, () =>
                    {
                        Write(url);
                    });
                    WriteElement(AuthenticatorTag, () =>
                    {
                        Write(accountKey);
                    });
                    WriteElement(ProjectNameTag, () =>
                    {
                        Write(projectName);
                    });
                });
            });

            //Read the reply and ensure success. 
            ReadReply(() =>
            {
                ReadElement(SuccessTag);
            });
        }

        /// <summary>
        /// Closes the underlying connection.
        /// </summary>
        public void Close()
        {
            CheckConnection();

            //Cloase readers and sockets
            reader.Close();
            writer.Close();

            filterStream.Close();

            netStream.Close();
            
            connection.Close();
            connection = null;          
        }

        /// <summary>
        /// Opens the underlying connection. 
        /// </summary>
        public void Connect()
        {
            //Check for invalid state.
            if (connection != null)
            {
                throw new InvalidOperationException("The connection is already open");
            }

            try
            {

                //Create a new socket and streams. 
                connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                connection.Connect(host, port);
            }
            catch
            {
                //Connect failed - throw.
                connection = null;
                throw;
            }

            netStream = new NetworkStream(connection);

            //We have to use our custom filter stream, to filter out
            //special chars, which are used by boinc. 
            filterStream = new FilterStream(netStream);

            //Create an Xml Writer
            //These settings are important since Boinc uses non-conformat Xml syntax and encoding. 
            //Especially, each message is ended with a 0x03 (End-Of-Text) char. 
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.ConformanceLevel = ConformanceLevel.Fragment;
            writerSettings.Indent = true;
            writerSettings.IndentChars = "";
            writerSettings.NewLineChars = ((char)0x0A).ToString();
            writerSettings.NewLineHandling = NewLineHandling.Replace;
            writerSettings.Encoding = Encoding.ASCII;
            writerSettings.OmitXmlDeclaration = true;

            writer = XmlTextWriter.Create(filterStream, writerSettings);

            //Start authentication, so some input is here to guess the encoding on reader initialization. 
            StartAuth();

            //These settings are important since Boinc uses non-conformat Xml syntax and encoding. 
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.ConformanceLevel = ConformanceLevel.Fragment;
            readerSettings.ValidationType = ValidationType.None;
            readerSettings.IgnoreWhitespace = true;
            readerSettings.IgnoreComments = true;
            readerSettings.IgnoreProcessingInstructions = true;

            reader = XmlReader.Create(filterStream, readerSettings);

            try
            {
                //Perform and finish the authenticaiton. 
                PerformAuth();
            }
            catch
            {
                //Authentication was invalid - close. 
                Close();
                throw;
            }
        }

        /// <summary>
        /// Reads the authentication salt and performs the authentication with Boinc. 
        /// StartAuth() must have been called first. 
        /// </summary>
        private void PerformAuth()
        {
            string salt = "";

            //Try to get the authentication salt. 
            ReadReply(() =>
            {
                ReadElement(NonceTag, () =>
                {
                     salt = ReadValue();
                });
            });

            //Convert and hash the salt and the password
            byte[] digest = Encoding.ASCII.GetBytes(salt + password);

            MD5 hasher = MD5.Create();
            hasher.ComputeHash(digest);

            //Now convert the hash to a hex string
            StringBuilder hash = new StringBuilder();

            foreach (byte b in hasher.Hash)
                hash.AppendFormat("{0:x2}", b);

            //Finally, send the hash back.  
            WriteRequest(() => 
            {
                WriteElement(Auth2Tag, () =>
                {
                    WriteElement(NonceHashTag, () =>
                    {
                        Write(hash.ToString());
                    });
                });
            });

            //Try to read the next status message to check wether we are authorized now. 
            ReadReply(() =>
            {
                ReadElement(AuthorizedTag);
            });
        }

        /// <summary>
        /// Starts the authentication process
        /// </summary>
        private void StartAuth()
        {
            WriteRpc(Auth1Tag);
        }

        /// <summary>
        /// Gets all projects which are currently running. 
        /// </summary>
        /// <returns>All projects which are currently running as enumeration of Project objects. </returns>
        public Project[] GetProjects()
        {
            CheckConnection();

            //Get and parse all projects.
            WriteRpc(GetProjectStatusTag);

            List<Project> projects = new List<Project>();

            ReadReply(() =>
            {
                ReadElement(ProjectsTag, () =>
                {
                    while (ReadElement(ProjectTag, () =>
                    {
                        projects.Add(ParseProject());
                    }, false, false));
                }, false);
            });

            //Now, get disk usage information. 
            WriteRpc(GetDiskUsageTag);

            ReadReply(() =>
            {
                ReadElement(DiskUsageSummaryTag, () =>
                {
                    //Read disk usage information for each project.
                    while (ReadElement(ProjectTag, () =>
                    {
                        string masterUrl = "";
                        double diskUsage = 0;

                        while (reader.Read() && reader.Name != ProjectTag)
                        {
                            switch (reader.Name)
                            {
                                case MasterUrlTag: masterUrl = ReadValueAndExitNode(); break;
                                case DiskUsageTag: diskUsage = ReadDoubleAndExitNode(); break;
                            }
                        }

                        //Finally copy the information into the corresponding project object. 
                        foreach (Project p in projects)
                        {
                            if (p.MasterUrl == masterUrl)
                                p.DiskUsage = (int)(diskUsage / 1048576); //Convert bytes to megabytes. 
                        }
                    }, false, false)) ;

                    SkipToEnd(DiskUsageSummaryTag);

                }, false);

            });

            return projects.ToArray();
        }

        /// <summary>
        /// Parses a project from the current XmlReader
        /// </summary>
        /// <returns>The parsed project object</returns>
        private Project ParseProject()
        {
            //Initialize working variables. 
            bool allowMoreWork = true;
            int diskUsage = 0;
            bool suspended = false;
            bool hasEnded = false;
            string masterUrl = "";
            string projectName = "";

            //Read everything inside the project node. 
            while (reader.Read() && reader.Name != ProjectTag)
            {
                //If we find an interresting node, copy it's contest into the given attribute. 
                switch (reader.Name)
                {
                    case MasterUrlTag: masterUrl = ReadValueAndExitNode(); break;
                    case ProjectNameTag: projectName = ReadValueAndExitNode(); break;
                    case DontRequestMoreWorkTag: allowMoreWork = false; ExitNode(); break;
                    case SuspendedViaGuiTag: suspended = true; ExitNode(); break;
                    case ProjectEndedTag: hasEnded = true; ExitNode(); break;
                }
            }

            return new Project(allowMoreWork, diskUsage, suspended, hasEnded, masterUrl, projectName, this);
        }

        /// <summary>
        /// Sends the given project operation for the project with the given url to Boinc.
        /// </summary>
        /// <param name="projectUrl">The project url.</param>
        /// <param name="projectOp">The project operation.</param>
        internal void WriteProjectOp(string projectUrl, string projectOp)
        {
            CheckConnection();

            WriteRequest(() =>
            {
                WriteElement(projectOp, () =>
                {
                    WriteElement(ProjectUrlTag, () =>
                    {
                        Write(projectUrl);
                    });
                });
            });

            //Read the reply and ensure success. 
            ReadReply(() =>
            {
                ReadElement(SuccessTag);
            });
            
        }

        /// <summary>
        /// Exits the current node, if the reader is standing on the node's value. 
        /// </summary>
        private void ExitNode()
        {
            if (!reader.IsEmptyElement)
                reader.Read();
        }

        /// <summary>
        /// Skips the next node, including all child nodes. 
        /// </summary>
        private void SkipNode()
        {
            reader.Read();
            if (reader.IsEmptyElement)
                return;
            
            string nodeName = reader.Name;

            SkipToEnd(nodeName);
        }

        /// <summary>
        /// Skips to the end of the given node. 
        /// </summary>
        /// <param name="name">The node to skip to.</param>
        private void SkipToEnd(string name)
        {
            while (reader.Read() && name != reader.Name) ;
        }

        /// <summary>
        /// Reads the value of the current node as string, exits the node and
        /// returns the value. 
        /// </summary>
        /// <returns>The node's value</returns>
        private string ReadValueAndExitNode()
        {
            reader.Read();
            string value = reader.Value;

            while (reader.Name == "")
            {
                //Only read a second time if there was a value. 
                //Else, we will write once too much. 
                reader.Read();
            }
            System.Diagnostics.Debug.WriteLine(value);

            return value;
        }

        /// <summary>
        /// Reads the value of the current node as double, exits the node and
        /// returns the value. 
        /// </summary>
        /// <returns>The node's value as double</returns>
        private double ReadDoubleAndExitNode()
        {
            return Double.Parse(ReadValueAndExitNode(), NumberFormatInfo.InvariantInfo); //Use international encoding. 
        }


        /// <summary>
        /// Reads the value of the current node as integer, exits the node and
        /// returns the value. 
        /// </summary>
        /// <returns>The node's value as integer</returns>
        private int ReadIntAndExitNode()
        {
            //Use double parsing as base for int parsing
            return (int)ReadDoubleAndExitNode(); 
        }

        /// <summary>
        /// Reads the value of the current node as DateTime, exits the node and
        /// returns the value. 
        /// </summary>
        /// <returns>The node's value as DateTime</returns>
        private DateTime ReadTimestampAndExitNode()
        {
            return DateFromUnixTime((long)ReadDoubleAndExitNode()); ;
        }

        /// <summary>
        /// Reads the value of the current node as TimeSpan, exits the node and
        /// returns the value. 
        /// </summary>
        /// <returns>The node's value as TimeSpan</returns>
        private TimeSpan ReadTimespanAndExitNode()
        {
            return new TimeSpan(0, 0, (int)ReadDoubleAndExitNode());
        }

        /// <summary>
        /// Gets all results.
        /// </summary>
        /// <param name="activeOnly">A bool indicating whether only active results should be returned.</param>
        /// <returns>An enumeration of Result objects.</returns>
        public Result[] GetResults(bool activeOnly = true)
        {
            CheckConnection();

            List<Result> results = new List<Result>();

            //Write result request to get all results
            WriteRequest(() =>
            {
                WriteElement(GetResultsTag, () =>
                {
                    WriteElement(ActiveOnlyTag, () =>
                    {
                        Write(activeOnly ? "1" : "0");
                    });
                });
            });

            //Read and parse all results
            ReadReply(() =>
            {
                ReadElement(ResultsTag, () =>
                {
                    while (ReadElement(ResultTag, () =>
                    {
                        results.Add(ParseResult());
                    }, false, false)) ;
                }, false);
            });

            return results.ToArray();
        }

        /// <summary>
        /// Parses a Result.
        /// </summary>
        /// <returns>The parsed Result object.</returns>
        private Result ParseResult()
        {
            Result result = new Result();

            //Read everything inside the result node. 
            while (reader.Read() && reader.Name != ResultTag)
            {
                //If we find an interresting node, copy it's contest into the given attribute. 
                switch (reader.Name)
                {
                    case NameTag: result.Name = ReadValueAndExitNode(); break;
                    case ProjectUrlTag: result.ProjectUrl = ReadValueAndExitNode(); break;
                    case ResourcesTag: result.Resources = ReadValueAndExitNode(); break;
                    case WorkUnitNameTag: result.WorkUnitName = ReadValueAndExitNode(); break;

                    case ExitStatusTag: result.ExitStatus = ReadIntAndExitNode(); break;
                    case PidTag: result.ProcessId = ReadIntAndExitNode(); break;
                    case SignalTag: result.Signal = ReadIntAndExitNode(); break;
                    case StateTag: result.State = ReadIntAndExitNode(); break;
                    case VersionNumTag: result.VersionNumber = ReadIntAndExitNode(); break;
                    case ActiveTaskStateTag: result.TaskState = (TaskState)ReadIntAndExitNode(); break;

                    case CurrentCpuTimeTag: result.CurrentCpuTime = ReadTimespanAndExitNode(); break;
                    case ElapsedTimeTag: result.ElapsedTime = ReadTimespanAndExitNode(); break;
                    case EstimatedCpuTimeRemainingTag: result.EstimatedCpuTimeRemaining = ReadTimespanAndExitNode(); break;
                    case FinalCpuTimeTag: result.FinalCpuTime = ReadTimespanAndExitNode(); break;
                    case FinalElapsedTimeTag: result.FinalElapsedTime = ReadTimespanAndExitNode(); break;

                    case ReceivedTimeTag: result.ReceivedTime = ReadTimestampAndExitNode(); break;
                    case ReportDeadlineTag: result.ReportDeadline = ReadTimestampAndExitNode(); break;

                    case FractionDoneTag: result.FractionDone = ReadDoubleAndExitNode(); break;

                    case ServerAckTag: result.Acknowledged = true; ExitNode(); break;
                    case ReadyToReportTag: result.ReadyToReport = true; ExitNode(); break;
                    case ActiveTaskTag: result.IsActive = true; break;
                }
            }

            return result;
        }

        /// <summary>
        /// Converts a date given in unix time (seconds since 1970) to a DateTime object.
        /// </summary>
        /// <param name="seconds">The elapsed seconds since 1970.</param>
        /// <returns>The DateTime object generated.</returns>
        private DateTime DateFromUnixTime(long seconds)
        {
            DateTime result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return result.AddSeconds(seconds);
        }

        /// <summary>
        /// Gets the version information of Boinc. 
        /// </summary>
        /// <returns>The version information.</returns>
        public Version GetVersion()
        {
            CheckConnection(); 
            
            //First, we get our own version. 
            Version myVersion = Assembly.GetEntryAssembly().GetName().Version;

            int major = 0;
            int minor = 0;
            int release = 0;

            //Then, we send our version to Boinc. 
            WriteRequest(() =>
            {
                WriteElement(ExchangeVersionsTag, () =>
                {
                    WriteElement(MajorTag, () =>
                    {
                        Write(myVersion.Major.ToString());
                    }); 

                    WriteElement(MinorTag, () =>
                    {
                        Write(myVersion.Minor.ToString());
                    }); 

                    WriteElement(ReleaseTag, () =>
                    {
                        Write(myVersion.Revision.ToString());
                    });
                });
            });

            //Finally, we receive Boinc's version as reply.
            ReadReply(() =>
            {
                  ReadElement(ServerVersionTag, () =>
                  {
                      while (reader.Read() && reader.Name != ServerVersionTag)
                      {
                          switch (reader.Name)
                          {
                              case MajorTag: major = ReadIntAndExitNode(); break;
                              case MinorTag: minor = ReadIntAndExitNode(); break;
                              case ReleaseTag: release = ReadIntAndExitNode(); break; 
                          }
                      }
                  }, false);
            });

            return new Version(major, minor, release);
        
        }

        /// <summary>
        /// Tells the Boinc application to quit. 
        /// <param name="waitForExit">True, if the method should wait until the Boinc process exited, else, false.</param>
        /// </summary>
        public void QuitClient(bool waitForExit = true)
        {
            CheckConnection();

            //Try to find the process to wait for, if applicable.
            Process boinc = null;
            if (waitForExit)
            {
                boinc = GetBoincProcess();
            }

            //Quit Boinc, close the connection and then wait for boinc to exit. 
            WriteRpc(QuitTag);
            Close();

            if (boinc != null && !boinc.HasExited)
            {
                boinc.WaitForExit();
            }
        }

        /// <summary>
        /// Gets the Boinc process.
        /// </summary>
        /// <returns>The Boinc process, or null, if no process was found.</returns>
        private Process GetBoincProcess()
        {
            //Check if Boinc is already running.
            //We can do this by fetching a list of all processes with the name Boinc and then searching for 
            //a process which has the boinc executable loaded as it's main module.
            Process[] processList = Process.GetProcessesByName("boinc");

            foreach (Process process in processList)
            {
                try
                {
                    //If we can find such a process, we can assume that boinc has already started. 
                    if (process.MainModule.FileName == BoincExecutableLocation)
                    {
                        return process;
                    }
                }
                catch (Win32Exception)
                {
                    //Used to silently catch Win32-Exceptions, which can happen when accessing other processes.
                }
            }
            return null;
        }

        /// <summary>
        /// Checks whether this client is connected to boinc. 
        /// Throws an InvalidOperationException if not. 
        /// </summary>
        private void CheckConnection()
        {
            if (connection == null)
            {
                throw new InvalidOperationException("Cannot operate when the connection is closed.");
            }
        }

        /// <summary>
        /// Writes out the RPC request with the given name, 
        /// without any parameters. 
        /// </summary>
        /// <param name="rpcName">The RPC request to write</param>
        private void WriteRpc(string rpcName)
        {
            WriteRequest(() =>
            {
                WriteElement(rpcName);
            });
        }

        /// <summary>
        /// Writes the start tag of a Boinc request,
        /// then executes the given <paramref name="action"/>, then 
        /// writes the end tag of a Boinc request. 
        /// </summary>
        /// <param name="action">The action to execute, or null, if no action should be executed.</param>
        private void WriteRequest(EmptyDelegate action = null)
        {
            writer.WriteStartBoincRequest();

            if (action != null)
                action();

            writer.WriteEndBoincRequest();
            netStream.EndSendBoincRequest();
        }

        /// <summary>
        /// Writes the start tag of an element,
        /// then executes the given <paramref name="action"/>, then 
        /// writes the end tag of an element.
        /// </summary>
        /// <param name="name">The name of the element to write.</param>
        /// <param name="action">The action to execute, or null, if no action should be executed.</param>
        private void WriteElement(string name, EmptyDelegate action = null)
        {
            writer.WriteStartBoincElement(name);

            if (action != null)
                action();

            writer.WriteEndBoincElement();
        }

        /// <summary>
        /// Writes a raw string.
        /// </summary>
        /// <param name="raw">The string to write.</param>
        private void Write(string raw)
        {
            writer.WriteRaw(raw);
        }

        /// <summary>
        /// Reads the start tag of a Boinc reply,
        /// then executes the given <paramref name="action"/>, then 
        /// reads the end tag of a Boinc reply. 
        /// </summary>
        /// <param name="action">The action to execute, or null, if no action should be executed.</param>
        /// <param name="readEnd">A bool indicating whether the end tag should be read. Set this to false if the given action already reads the end tag of this node.</param>
        private void ReadReply(EmptyDelegate action = null, bool readEnd = true)
        {
            reader.ReadStartBoincReply();

            if (action != null)
                action();

            if (readEnd)
                reader.ReadEndBoincReply();
        }

        /// <summary>
        /// Reads the start tag of an element,
        /// then executes the given <paramref name="action"/>, then 
        /// reads the end tag of an element.
        /// </summary>
        /// <param name="name">The name of the element to read.</param>
        /// <param name="action">The action to execute, or null, if no action should be executed.</param>
        /// <param name="readEnd">A bool indicating whether the end tag should be read. Set this to false if the given action already reads the end tag of this node.</param>
        /// <param name="throwOnUnexpected">A bool indicating whether an exception should be thrown when an unexpected tag is found. 
        /// If set to false, the method returns false in case of an error instead of throwing an exception.</param>
        private bool ReadElement(string name, EmptyDelegate action = null, bool readEnd = true, bool throwOnUnexpected = true)
        {
            //Tries to read an element
            bool success = reader.ReadBoinc(name, throwOnUnexpected);

            if (!success)
                return false;

            //If read to end is set, check if reading to the end is necassary
            if (readEnd)
                readEnd = !reader.IsEmptyElement;

            if (action != null)
                action();

            //Read end
            if (readEnd)
                reader.ReadBoinc(name, throwOnUnexpected);

            return success;
        }

        /// <summary>
        /// Reads the value of the current node as raw string. 
        /// </summary>
        /// <returns>The value of the current node</returns>
        private string ReadValue()
        {
            reader.Read();
            return reader.Value;
        }
    }

    /// <summary>
    /// Represents a Boinc api exception. 
    /// </summary>
    public class BoincApiException : Exception
    {
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="message">The mesage of the exception.</param>
        public BoincApiException(string message) :
            base(message) { }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="message">The mesage of the exception.</param>
        /// <param name="innerException">The inner exception.</param>
        public BoincApiException(string message, Exception innerException) :
            base(message, innerException) { }
    }
}

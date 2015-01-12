using System;
using System.Collections.Generic;
using System.Text;
using Boinc;

//******************************************************
// This source file is only here for testing/debugging
// and will be deleted. 
//******************************************************

namespace BoincCoreTests
{
    class Program
    {
        static void Main(string[] args)
        {
            //Runs simple tests.
            //Outputs can be compared to the output of boinccmd for verification. 

            //Creates a new Boinc Gui RPC client for the local host 
            Boinc.BoincClient b = new Boinc.BoincClient();
            //if (!b.BoincIsRunning)
            //    b.StartClient();
            b.Connect();

            BoincWatcher watcher = new BoincWatcher(b);

            //Attaches to a project
            //b.AttachToProject("http://www.worldcommunitygrid.org", "b6c56e43daf4dd0760c41cb3cf51862a", "this is my stupid project name");

            //Gets and lists current projects
            /*Console.WriteLine("Current projects:");
            Console.WriteLine();
            foreach(Project p in b.GetProjects()) {
                Console.WriteLine("{0} ({1})", p.ProjectName, p.MasterUrl);
                Console.WriteLine("Disk Usage: {0} MB", p.DiskUsage);
                if (p.Suspended)
                    Console.WriteLine("Suspended via gui");
                if (p.MoreWorkAllowed)
                    Console.WriteLine("Allow more work");
                if (p.HasEnded)
                    Console.WriteLine("Project has ended");
            }
            Console.WriteLine("Results:");
            foreach (Result r in b.GetResults())
            {
                Console.WriteLine(r.WorkUnitName);
            }*/

            watcher.ProjectAdded += new EventHandler<CollectionModifiedEventArgs<Project>>(watcher_ProjectAdded);
            watcher.ProjectRemoved += new EventHandler<CollectionModifiedEventArgs<Project>>(watcher_ProjectRemoved);
            watcher.TaskAdded += new EventHandler<CollectionModifiedEventArgs<Result>>(watcher_TaskAdded);
            watcher.TaskRemoved += new EventHandler<CollectionModifiedEventArgs<Result>>(watcher_TaskRemoved);
            watcher.TaskStateChanged += new EventHandler<TaskStateChangedEventArgs>(watcher_TaskStateChanged);

            bool run = true;

            while (run)
            {
                watcher.RefreshState();
                System.Threading.Thread.Sleep(1000);

                if (Console.KeyAvailable)
                {
                    switch (Console.ReadKey().KeyChar)
                    {
                        case 'x': run = false;
                            break;
                        case 's': Preferences prefs = b.GetPreferences();
                            Console.WriteLine("Settings");
                            Console.WriteLine("     Cores to use: " + prefs.CpuCountLimit);
                            break;
                        case 'p': Console.WriteLine("Pause. Press any key to continue.");
                            Console.ReadKey();
                            break;
                        case 't': Result[] tasks = watcher.Results;
                            Console.WriteLine("Tasks: ");
                            foreach (Result t in tasks)
                            {
                                Console.WriteLine("     " + t.Name);
                            }
                            break;
                    }
                }
            }


            //Gets the client version
            //Console.WriteLine("Client version: {0}", b.GetVersion());

            //Gets results, proxy settings and preferences
            //IEnumerable<Result> results = b.GetResults();
            //ProxySettings prox = b.GetProxySettings();
            //Preferences pref = b.GetPreferences();

            //Sets preferences
            //b.SetPreferences(new Preferences(true, false, 50, 10, 1));

            //Closes the connection
            b.Close();
        }

        static void watcher_TaskStateChanged(object sender, TaskStateChangedEventArgs e)
        {
            Console.WriteLine("Task Changed: " + e.NewState.Name);
            Console.WriteLine("     Percentage: " + e.NewState.FractionDone);
            Console.WriteLine("     Remaining Time: " + e.NewState.EstimatedCpuTimeRemaining);
            Console.WriteLine("     Ready To Report: " + e.NewState.ReadyToReport);
            Console.WriteLine("     Ack: " + e.NewState.Acknowledged);
        }

        static void watcher_TaskRemoved(object sender, CollectionModifiedEventArgs<Result> e)
        {
            Console.WriteLine("Task Removed: " + e.ModifiedItem.Name);
        }

        static void watcher_TaskAdded(object sender, CollectionModifiedEventArgs<Result> e)
        {
            Console.WriteLine("Task Added: " + e.ModifiedItem.Name);
        }

        static void watcher_ProjectRemoved(object sender, CollectionModifiedEventArgs<Project> e)
        {
            Console.WriteLine("Project Removed: " + e.ModifiedItem.ProjectName);
        }

        static void watcher_ProjectAdded(object sender, CollectionModifiedEventArgs<Project> e)
        {
            Console.WriteLine("Project Added: " + e.ModifiedItem.ProjectName);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Boinc
{
    /// <summary>
    /// This class provides an event driven way to interact with Boinc.
    /// <remarks>
    /// The user of this class is responsible for calling RefreshState, which 
    /// queries information from Boinc and raises events. 
    /// </remarks>
    /// </summary>
    public class BoincWatcher
    {
        /// <summary>
        /// The Boinc client to watch.
        /// </summary>
        private BoincClient client;

        //Arrays to memorize the state. 
        private Project[] lastProjects;
        private Result[] lastResults;

        /// <summary>
        /// Returns the currently knwon set of attached projects. 
        /// </summary>
        public Project[] Projects
        {
            get{
                if (lastProjects == null)
                {
                    return new Project[0];
                }
                return lastProjects;
            }
        }

        /// <summary>
        /// Returns the currently knwon set of running tasks. 
        /// </summary>
        public Result[] Results
        {
            get
            {
                if (lastResults == null)
                {
                    return new Result[0];
                }
                return lastResults;
            }
        }

        /// <summary>
        /// Creates a new instance of this class. 
        /// </summary>
        /// <param name="client">The connected client to interact with.</param>
        public BoincWatcher(BoincClient client)
        {
            this.client = client;
        }

        /// <summary>
        /// This event is raised whenever a new task (result) has been added.
        /// </summary>
        public event EventHandler<CollectionModifiedEventArgs<Result>> TaskAdded;
        /// <summary>
        /// This event is raised whenever a task (result) has been removed.
        /// </summary>
        public event EventHandler<CollectionModifiedEventArgs<Result>> TaskRemoved;
        /// <summary>
        /// This event is raised whenever a new project has been added.
        /// </summary>
        public event EventHandler<CollectionModifiedEventArgs<Project>> ProjectAdded;
        /// <summary>
        /// This event is raised whenever a  project has been removed.
        /// </summary>
        public event EventHandler<CollectionModifiedEventArgs<Project>> ProjectRemoved;
        /// <summary>
        /// This event is raised whenever the state of a task (result) has changed. 
        /// </summary>
        public event EventHandler<TaskStateChangedEventArgs> TaskStateChanged;

        /// <summary>
        /// Clears the stored state of this BoincController object.
        /// </summary>
        public void ClearState()
        {
            lastProjects = null;
            lastResults = null;
        }

        /// <summary>
        /// Queries new information from Boinc and raises events if applicatble.
        /// <remarks>
        /// No events are raised on the first call of this method or when ClearState() was called before. 
        /// </remarks>
        /// </summary>
        public void RefreshState()
        {
            //Gather new information. 
            Project[] projects = client.GetProjects();
            Result[] results = client.GetResults();

            Project[] lastProjects = this.lastProjects;
            Result[] lastResults = this.lastResults;

            //Save the new information as state. 
            this.lastProjects = projects;
            this.lastResults = results;

            //If we have a known state, compare. 
            if (lastProjects != null)
            {
                CompareArrays(lastProjects, projects, ProjectRemoved, ProjectAdded, CompareProject);
            }
            if (lastResults != null)
            {
                CompareArrays(lastResults, results, TaskRemoved, TaskAdded, CompareResult);
                CompareResults(lastResults, results);
            }
            
        }

        /// <summary>
        /// Compares old result states with new result states and raises the TaskStateChanged event
        /// when a result state has changed. 
        /// </summary>
        /// <param name="oldValues">The old results. </param>
        /// <param name="newValues">The new results. </param>
        private void CompareResults(Result[] oldValues, Result[] newValues)
        {
            if (TaskStateChanged != null)
            {
                //Iterate over each old result. 
                foreach (Result oldResult in oldValues)
                {
                    //Find the corresponding new result.
                    Result newResult = Array.Find(newValues, delegate(Result x) { return CompareResult(oldResult, x) == 0; });

                    //If we found a new result, compare the results. 
                    if (newResult != null)
                    {
                        if (oldResult.Acknowledged != newResult.Acknowledged ||
                           oldResult.CurrentCpuTime != newResult.CurrentCpuTime ||
                           oldResult.FractionDone != newResult.FractionDone ||
                           oldResult.IsActive != newResult.IsActive ||
                           oldResult.ReadyToReport != newResult.ReadyToReport ||
                           oldResult.EstimatedCpuTimeRemaining != newResult.EstimatedCpuTimeRemaining)
                        {
                            //If the result was different, raise the event. 
                            TaskStateChanged(this, new TaskStateChangedEventArgs(oldResult, newResult));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Compares two results by their names,
        /// </summary>
        /// <param name="a">The first result.</param>
        /// <param name="b">The second result.</param>
        /// <returns>The result of the comparison.</returns>
        private int CompareResult(Result a, Result b)
        {
            return a.Name.CompareTo(b.Name);
        }

        /// <summary>
        /// Compares two projects by their master urls,
        /// </summary>
        /// <param name="a">The first project.</param>
        /// <param name="b">The second project.</param>
        /// <returns>The result of the comparison.</returns>
        private int CompareProject(Project a, Project b)
        {
            return a.MasterUrl.CompareTo(b.MasterUrl);
        }

        /// <summary>
        /// Finds differences between oldValues and newValues and raises the corresponding events. 
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="oldValues">An array containing old values.</param>
        /// <param name="newValues">An array containing new values.</param>
        /// <param name="removedHandler">An event to raise when an item was removed.</param>
        /// <param name="addedHandler">An event to raise when an item was added.</param>
        /// <param name="compare">A comparison delegate for T.</param>
        private void CompareArrays<T>(T[] oldValues, T[] newValues, EventHandler<CollectionModifiedEventArgs<T>> removedHandler, EventHandler<CollectionModifiedEventArgs<T>> addedHandler, Comparison<T> compare)
        {
            //Find added and removed values.
            List<T> addedValues = FindMissingValues(newValues, oldValues, compare);
            List<T> removedValues = FindMissingValues(oldValues, newValues, compare);

            //Raise events. 
            if (addedHandler != null)
            {
                foreach (T val in addedValues)
                {
                    addedHandler(this, new CollectionModifiedEventArgs<T>(val));
                }
            }
            if (removedHandler != null)
            {
                foreach (T val in removedValues)
                {
                    removedHandler(this, new CollectionModifiedEventArgs<T>(val));
                }
            }
        }

        /// <summary>
        /// Finds all values of first which are not contained in second. 
        /// </summary>
        /// <typeparam name="T">The types.</typeparam>
        /// <param name="first">The first array.</param>
        /// <param name="second">The second array.</param>
        /// <param name="compare">A comparison delegate for T.</param>
        /// <returns>A list of values which were contained in first, but not in second.</returns>
        private List<T> FindMissingValues<T>(T[] first, T[] second, Comparison<T> compare)
        {
            List<T> missing = new List<T>();
            foreach (T val in first)
            {
                if (Array.Find(second, delegate(T a) { return compare(val, a) == 0; }) == null)
                {
                    missing.Add(val);
                }
            }

            return missing;
        }
    }

    /// <summary>
    /// Represents arguments for the TaskStateChanged event. 
    /// </summary>
    public class TaskStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The old task (result) state.
        /// </summary>
        public Result OldState { get; private set; }

        /// <summary>
        /// The new task (result) state.
        /// </summary>
        public Result NewState { get; private set; }

        /// <summary>
        /// Creates a new instance of this class. 
        /// </summary>
        /// <param name="oldState">The old task (result) state.</param>
        /// <param name="newState">The new task (result) state.</param>
        public TaskStateChangedEventArgs(Result oldState, Result newState)
        {
            this.OldState = oldState;
            this.NewState = newState;
        }
    }

    /// <summary>
    /// Represents arguments for add or remove operations. 
    /// </summary>
    public class CollectionModifiedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// The item which has been modified.
        /// </summary>
        public T ModifiedItem { get; private set; }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="item">The item associated with this event.</param>
        public CollectionModifiedEventArgs(T item)
        {
            ModifiedItem = item;
        }
    }
}

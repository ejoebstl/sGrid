using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.DataAccessLayer;

namespace sGridServer.Code.GridProviders
{
    /// <summary>
    /// This abstract class provides an interface for accessing
    /// grid providers and auxiliary methods for all classes 
    /// deriving from this class. 
    /// </summary>
    public abstract class GridProvider
    {
        /// <summary>
        /// The data context of this class.
        /// </summary>
        protected SGridDbContext DataContext { get; set; }

        /// <summary>
        /// Returns all results. 
        /// </summary>
        public IEnumerable<CalculatedResult> Results
        {
            get { return DataContext.CalculatedResults.AsNoTracking(); }
        }

        /// <summary>
        /// Gets the description of this GridProvider object. 
        /// </summary>
        public GridProviderDescription Description { get; private set; }


        /// <summary>
        /// This event is raised whenever the performance data associated with a project and a user changes. 
        /// </summary>
        public static event EventHandler<ResultStateChangedEventArgs> ResultStateChanged;

        /// <summary>
        /// Creates a new instance of this class, remembering the given GridProviderDescription object. 
        /// </summary>
        /// <param name="description">The description associated with the new instance. </param>
        protected GridProvider(GridProviderDescription description)
        {
            this.DataContext = new SGridDbContext();
            this.Description = description;
        }

        /// <summary>
        /// Returns the grid authentication token for the given user and the given grid project. 
        /// </summary>
        /// <param name="u">The user to get the token for.</param>
        /// <param name="gridProject">The GridProjectDescription to get the token for.</param>
        /// <returns>The authentication token for the grid project, which can be sent to the client using the ClientApi class.</returns>
        public static GridProviderAuthenticationData GetAuthTokenForUser(User u, GridProjectDescription gridProject)
        {
            SGridDbContext dataContext = new SGridDbContext();

            return (from a in dataContext.DataForGridProvider.AsNoTracking()
                    where a.User.Id == u.Id && a.ProviderId == gridProject.Provider.Id 
                    select a).FirstOrDefault();
        }

        /// <summary>
        /// Returns all calculation results associated with the given user and project. 
        /// </summary>
        /// <param name="u">The user to get the results for.</param>
        /// <param name="proj">The project to get the results for. </param>
        /// <returns>An enumeration of all calculated results which match the given user and project.</returns>
        public static IEnumerable<CalculatedResult> GetResults(User u, GridProjectDescription proj)
        {
            return GetResults(u, proj.ShortName);
        }

        /// <summary>
        /// Returns all calculation results associated with the given user and project. 
        /// </summary>
        /// <param name="u">The user to get the results for.</param>
        /// <param name="projectShortName">The short name of the project to get the results for. </param>
        /// <returns>An enumeration of all calculated results which match the given user and project.</returns>
        public static IEnumerable<CalculatedResult> GetResults(User u, string projectShortName)
        {
            SGridDbContext dataContext = new SGridDbContext();

            return from c in dataContext.CalculatedResults.AsNoTracking()
                   where c.ProjectShortName == projectShortName && c.User.Id == u.Id
                   select c;
        }

        /// <summary>
        /// Notifies the grid provider that the state of a result changed. 
        /// If the result does not already exist, it is added to the database. 
        /// If a result already exists is determined using the ProjectShortName, the WorkUnitName and the UserId.
        /// If the result state is changed, the appropriate times are added automatically.
        /// This method also grants coins for the done results, if applicable. 
        /// </summary>
        /// <param name="result">The result to register.</param>
        /// <param name="newState">The new state of the result.</param>
        /// <param name="valid">A bool indicating whether the validation has been successful. Only used when newState equals ResultState.Validated.</param>
        public void RegisterResultEvent(CalculatedResult result, ResultState newState, bool valid = false)
        {
            if (result.State != newState)
            {
                result.State = newState;
            }

            CalculatedResult workingCopy = (from r in DataContext.CalculatedResults 
                                            where r.ProjectShortName == result.ProjectShortName && 
                                                  r.WorkUnitName == r.WorkUnitName && 
                                                  r.UserId == result.UserId select r).FirstOrDefault();

            GridProjectDescription description = GridProviderManager.ProjectForName(result.ProjectShortName);

            if (workingCopy == null)
            {
                //If we create a new entry, set these two to a predefined value, as we rely on their correctness when granting coins. 
                result.ServerSentTimestamp = DateTime.MaxValue;
                result.ServerReceivedTimestamp = DateTime.MaxValue;

                if (newState == ResultState.ReceivedByClient)
                    result.ClientReceivedTimestamp = DateTime.Now;
                if (newState == ResultState.SentToClient)
                    result.ServerSentTimestamp = DateTime.Now;

                result = DataContext.CalculatedResults.Add(result);

                NotifyProjectDataChanged(result.User, description, result);
            }
            else
            {
                if (workingCopy.State != newState)
                {
                    //Only trigger events when state has changed. 

                    if (newState == ResultState.ReceivedByClient)
                        result.ClientReceivedTimestamp = DateTime.Now;
                    if (newState == ResultState.SentToClient)
                        result.ServerSentTimestamp = DateTime.Now;
                    if (newState == ResultState.ReceivedByClient)
                        result.ClientReceivedTimestamp = DateTime.Now;
                    if (newState == ResultState.SentToServer)
                        result.ClientSentTimestamp = DateTime.Now;
                    if (newState == ResultState.Validated)
                    {
                        result.ValidatedTimestamp = DateTime.Now;
                        result.Valid = valid;
                    }

                    if (newState == ResultState.Validated && result.Valid)
                    {
                        if (result.ServerSentTimestamp != result.ServerReceivedTimestamp && result.ServerReceivedTimestamp != DateTime.MaxValue)
                        {
                            GrantCoins(result.User, description, result.ServerReceivedTimestamp - result.ServerSentTimestamp);
                        }
                    }

                    NotifyProjectDataChanged(workingCopy.User, description, result);
                }
                result.Id = workingCopy.Id;
                DataContext.Entry(workingCopy).CurrentValues.SetValues(result);
            }
            DataContext.SaveChanges();

        }

        /// <summary>
        /// Registers the given user with the grid provider and stores the grid authentication token in the database. 
        /// </summary>
        /// <param name="u">The user to register. </param>
        /// <returns>The grid authentication token. </returns>
        public GridProviderAuthenticationData RegisterUser(User u)
        {
            string id = this.Description.Id;

            //Check whether the given user is already registered.
            if (DataContext.DataForGridProvider.Where(x => x.UserId == u.Id && x.ProviderId == id).Any())
            {
                throw new InvalidOperationException("The given user is already registered");
            }

            //Perform the registration.
            GridProviderAuthenticationData data = RegisterUserWithProjectServer(u);

            //Store the registration data.
            DataContext.DataForGridProvider.Add(data);
            DataContext.SaveChanges();

            return data;
        }

        /// <summary>
        /// Registers the given user with the grid provider, if the user is not already registered. 
        /// </summary>
        /// <param name="u">The user to register. </param>
        /// <returns>The grid authentication token. </returns>
        protected abstract GridProviderAuthenticationData RegisterUserWithProjectServer(User u);

        /// <summary>
        /// Removes the given users registration from the grid provider and deletes the data for the grid provider from the database. 
        /// </summary>
        /// <param name="u">The user to unregister.</param>
        public void UnRegisterUser(User u)
        {
            string id = this.Description.Id;

            //Check whether the given user is registered.
            GridProviderAuthenticationData data = DataContext.DataForGridProvider.Where(x => x.UserId == u.Id && x.ProviderId == id).FirstOrDefault();

            if (data == null)
            {
                throw new InvalidOperationException("The given user is not registered");
            }

            //Performs the un-registration. 
            RemoveUserFromProjectServer(u, data);

            //Save the changes. 
            DataContext.DataForGridProvider.Remove(data);
            DataContext.SaveChanges();
        }

        /// <summary>
        /// Removes the given users registration from the grid provider. 
        /// </summary>
        /// <param name="u">The user to unregister.</param>
        /// <param name="data">The data for the grid provider associated with the current user.</param>
        protected abstract void RemoveUserFromProjectServer(User u, GridProviderAuthenticationData data);

        /// <summary>
        /// Transfers the coins for completing a result of the given 
        /// project to the users coin account, according to the coin 
        /// grant formula. This method also calls the GrantParameters.ModifyGrant 
        /// method to apply multipliers and other bonuses. 
        /// </summary>
        /// <param name="u">The user to grant the coins to.</param>
        /// <param name="proj">The project to grant the coins for.</param>
        /// <param name="timeNeeded">The time which was needed to calculate the result.</param>
        protected void GrantCoins(User u, GridProjectDescription proj, TimeSpan timeNeeded)
        {
            const int scalingFactor = 2;

            //Calculate the coins to grant, according to the grant formula. 
            int coinsToGrant = (int)((Math.Log(1 + ((proj.AverageCalculationTime) - timeNeeded.TotalMinutes) / scalingFactor) + 1) * proj.CoinsPerResult);

            //Call the grant modifier. 
            coinsToGrant = GrantParameters.ModifyGrant(u, coinsToGrant, proj);

            //Grant the coins. 
            CoinExchange.CoinExchange exchange = new CoinExchange.CoinExchange(u);
            exchange.Grant(proj, coinsToGrant);
        }

        /// <summary>
        /// Raises the project data changed event with the given parameters. 
        /// </summary>
        /// <param name="user">The user associated with the event.</param>
        /// <param name="project">The project for which the result statistics have changed. </param>
        /// <param name="result">The result associated with the event.</param>
        protected static void NotifyProjectDataChanged(User user, GridProjectDescription project, CalculatedResult result)
        {
            if (ResultStateChanged != null)
            {
                ResultStateChanged(null, new ResultStateChangedEventArgs(user, project, result));
            }
        }

    }
}
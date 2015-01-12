using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Code.GridProviders
{
    /// <summary>
    /// This class describes a grid provider and is capable of creating a 
    /// GridProvider object. 
    /// </summary>
    public class GridProviderDescription
    {
        /// <summary>
        /// Gets the System.Type of the GridProvider class associated with this provider. 
        /// </summary>
        private Type providerType;

        /// <summary>
        /// Gets all projects for this provider, as Array of GridProjectDescription.
        /// </summary>
        public GridProjectDescription[] AvailableProjects { get; private set; }

        /// <summary>
        /// Gets the description of this grid provider object.
        /// </summary>
        public MultiLanguageString Description { get; private set; }

        /// <summary>
        /// Gets the url of the icon associated with this grid provider object.
        /// </summary>
        public string IconUrl { get; private set; }

        /// <summary>
        /// Gets the identifier for this grid provider object.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the human readable name of this grid provider object.
        /// </summary>
        public MultiLanguageString Name { get; private set; }

        /// <summary>
        /// Gets the slogan for this grid provider object.
        /// </summary>
        public MultiLanguageString Slogan { get; private set; }

        /// <summary>
        /// Gets the url pointing to the website of this grid provider object.
        /// </summary>
        public string WebsiteUrl { get; private set; }

        /// <summary>
        /// Gets the url pointing to the boinc working directory on the provider's server. 
        /// </summary>
        public string WorkspaceUrl { get; private set; }

        /// <summary>
        /// Creates a new instance of this class and stores all given 
        /// parameters into the corresponding properties. 
        /// <remarks>
        /// This constructor also sets the Provider property of all projects given in availableProjecs to this object.
        /// </remarks>
        /// </summary>
        /// <param name="id">An identifier for this grid provider.</param>
        /// <param name="name">The human readable name of this grid provider.</param>
        /// <param name="iconUrl">The url of the icon associated with this grid provider.</param>
        /// <param name="description">A description of this grid provider.</param>
        /// <param name="slogan">A slogan for this grid provider.</param>
        /// <param name="websiteUrl">The url pointing to the website of this grid provider.</param>
        /// <param name="workspaceUrl">The url pointing to the boinc working directory on the provider's server. .</param>
        /// <param name="availableProjects">All projects for this provider.</param>
        /// <param name="providerType">The System.Type of the GridProvider class associated with this provider. 
        /// The given Type must have an empty default constructor, so dynamic instantiation is possible.</param>
        public GridProviderDescription(string id, MultiLanguageString name, string iconUrl, MultiLanguageString description,
            MultiLanguageString slogan, string websiteUrl, string workspaceUrl, GridProjectDescription[] availableProjects, Type providerType)
        {
            this.AvailableProjects = availableProjects;
            this.Description = description;
            this.IconUrl = iconUrl;
            this.Id = id;
            this.Name = name;
            this.Slogan = slogan;
            this.WebsiteUrl = websiteUrl;
            this.WorkspaceUrl = workspaceUrl;

            //Check if the given type has a default constructor and inherits from GridProvider. 
            if (providerType.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new ArgumentException("The given provider type has no default constructor defined.");
            }

            if (!(typeof(GridProvider).IsAssignableFrom(providerType)))
            {
                throw new ArgumentException("The given type is not of type GridProvider.");
            }

            this.providerType = providerType;

            //For each given project, check whether the project is not already associated with another provider. 
            foreach (GridProjectDescription project in availableProjects)
            {
                if (project.Provider != null)
                {
                    throw new ArgumentException("The project " + project.Name + " is already associated with another provider (" + project.Provider.Name + ")");
                }

                //Associate the project with this provider. 
                project.Provider = this;
            }
        }

        /// <summary>
        /// Creates an instance of the GridProvider class associated
        /// with this description by instantiating provider type. 
        /// </summary>
        /// <returns>The created GridProvider object.</returns>
        public GridProvider CreateProvider()
        {
            return (GridProvider)Activator.CreateInstance(providerType);
        }
    }
}
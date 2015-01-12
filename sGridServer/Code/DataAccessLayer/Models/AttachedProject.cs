using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This class represents an attached project, 
    /// which is a project a user is currently working on or has worked on.
    /// </summary>
    public class AttachedProject
    {
        /// <summary>
        /// Gets or sets the id of the element in the attached project database set.
        /// </summary>
        [Key]
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets a bool indicating whether this project is currently attached to the user, 
        /// or whether it is an old project which is not in use.
        /// </summary>
        public bool Current { get; set; }

        /// <summary>
        /// Gets or sets the timestamp indicating when the user attached to this project.
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the short name of the project.
        /// </summary>
        public String ShortName { get; set; }


        /// <summary>
        /// Gets or sets the account id of the user, who is working on this project.
        /// </summary>
        [ForeignKey("User")]
        public int UserId { get; set; }


        /// <summary>
        /// Gets or sets the user, who is working on this project.
        /// </summary>
        public virtual User User { get; set; }


        /// <summary>
        /// A default constructor of the attached project.
        /// </summary>
        public AttachedProject()
        {
            this.Date = DateTime.Now;
            this.ShortName = "";
        }
    }
}
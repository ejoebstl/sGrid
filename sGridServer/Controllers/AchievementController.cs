using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.Achievements;
using sGridServer.Code.Security;
using sGridServer.Code.DataAccessLayer;
using System.Reflection;
using sGridServer.Models;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Utilities;
using Resource = sGridServer.Resources.Achievements.AchievementRes;
using sGridServer.Properties;
using System.Drawing;

namespace sGridServer.Controllers
{
    /// <summary>
    /// This controller is responsible for managing the
    /// AchievementCreateFormView.
    /// </summary>
    public class AchievementController : Controller
    {
        /// <summary>
        /// The session key of the return url. 
        /// </summary>
        private const string ReturnUrlSessionKey = "AchievementControllerReturnUrl";

        /// <summary>
        /// Gets or sets the url to return to after having finished
        /// execution.
        /// </summary>
        protected String ReturnUrl
        {
            get
            {
                return (string)Session[ReturnUrlSessionKey];
            }
            private set
            {
                Session[ReturnUrlSessionKey] = value;
            }
        }

        /// <summary>
        /// Stores the return url and creates a model from an 
        /// existing achievement, or a new achievement depending on
        /// the id and then returns the AchievementCreateFormView.
        /// </summary>
        /// <param name="returnUrl">The url to return to.</param>
        /// <param name="achievementType">The type of the achievement
        /// to create.</param>
        /// <param name="achievementId">The id of the achievement to
        /// be changed, or -1 if a new achievement has to be created.</param>
        /// <returns>AchievementCreateFormView with the created
        /// Achievement object as model.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult AchievementCreateForm(String returnUrl, String achievementType, int achievementId)
        {
            this.ReturnUrl = returnUrl;
            ViewBag.AchievementErrorMessage = null;

            AchievementManager manager = new AchievementManager();
            AchievementItem achievement;
            AchievementModel model = new AchievementModel();

            if (achievementId < 0)
            {
                // -> new achievement
                achievement = manager.GetAchievementFromType(achievementType);

                /* 
                 * For every additionnal Property that can be changed
                 * (that is every Property indicated by PropertyNames),
                 * an entry in the dictionnary is made.
                 */
                PropertyInfo info;
                foreach (String propName in achievement.PropertyNames)
                {
                    info = achievement.GetType().GetProperty(propName);
                    model.Properties.Add(new AchievementProperty()
                    {
                        PropertyName = propName,
                        ShowName = propName, //Todo Jerome get localized name
                        Value = info.GetValue(achievement, null).ToString()
                    });
                }
            }
            else
            {
                // -> changed achievement
                achievement = AchievementItem.ToAchievementItem(manager.GetAchievementById(achievementId));
            }

            model.AchievementType = achievement.AchievementType;
            model.AchievementId = achievement.AchievementId;
            model.Name = achievement.Name;
            model.Description = achievement.Description;
            model.Icon = achievement.Icon;
            model.Active = achievement.Active;

            return View(model);
        }

        /// <summary>
        /// Cancels the creation of the achievement and returns to url
        /// specified in ReturnUrl. 
        /// </summary>
        /// <returns>A redirect to the ReturnUrl.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult Cancel()
        {
            return Redirect(ReturnUrl);
        }

        /// <summary>
        ///  Tries to submit the created achievement. In case of
        ///  failure the AchievementCreateFormView will be returned.
        ///  If the submission succeeds, the user is redirected to
        ///  ReturnUrl and the achievement will be saved.
        /// </summary>
        /// <param name="model">The achievement to be saved.</param>
        /// <param name="file">The file representing the icon.</param>
        /// <returns>The AchievementCreateFormView, or a redirect to the
        /// ReturnUrl.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        [HttpPost]
        public ActionResult Submit(AchievementModel model, HttpPostedFileBase file)
        {
            ViewBag.AchievementErrorMessage = null;

            AchievementManager manager = new AchievementManager();
            AchievementItem achievement;
            if (model.AchievementId < 0)
            {
                // -> new achievement
                achievement = manager.GetAchievementFromType(model.AchievementType);
            }
            else
            {
                // -> changed achievement
                achievement = AchievementItem.ToAchievementItem(manager.GetAchievementById(model.AchievementId));
            }

            achievement.AchievementId = model.AchievementId;
            achievement.Name = model.Name;
            achievement.Description = model.Description;
            achievement.Active = model.Active;

            //the icon is stored
            BlobStorage storage = new BlobStorage(Settings.Default.AchievementPictureStorageContainer);
            if (file != null)
            {
                if (achievement.Icon != "")
                {
                    storage.RemoveBlob(achievement.Icon);
                }
                achievement.Icon = storage.StoreBlob(
                        ImageUtil.ResizeImage(file.InputStream,
                            Settings.Default.AchievementIconWidth,
                            Settings.Default.AchievementIconHeight,
                            Color.White));
            }
            else if (achievement.Icon == "")
            {
                achievement.Icon = GetDefaultAchievementIcon();
            }

            if (model.AchievementId < 0)
            {
                // -> new achievement

                //transfer the properties and test, if they are correct
                if (TransferProperties(achievement, model))
                {
                    //test, if no invalid values have been entered
                    string errorMessage;
                    if (!achievement.PrepareForSaving(out errorMessage))
                    {
                        ViewBag.AchievementErrorMessage = String.Format(Resource.Error, errorMessage);
                    }
                    else
                    {
                        //a test if there are 2 achievements behaving the same
                        if ((from Achievement ach in manager.GetAllExistingAchievements()
                             where (ach.AchievementType == achievement.AchievementType)
                             && (ach.ExtendedParameters == achievement.ExtendedParameters)
                             select ach).Any())
                        {
                            ViewBag.AchievementErrorMessage = Resource.IdentityError;
                        }
                        else
                        {
                            //now the achievement is valid and can be saved
                            manager.CreateAchievement(achievement);

                            return Redirect(ReturnUrl);
                        }
                    }
                }
            }
            else
            {
                // -> changed achievement

                //the changes are now saved
                manager.EditAchievement(achievement);

                return Redirect(ReturnUrl);
            }
            //the achievement was not accepted
            return View("AchievementCreateForm", model);
        }

        /// <summary>
        /// Copies the default achievement icon into the blob storage and returns the storage url. 
        /// </summary>
        /// <returns>The url to the stored picture in the blob storage.</returns>
        public static string GetDefaultAchievementIcon()
        {
            return ImageUtil.GetPicturyByUrl(Properties.Settings.Default.DefaultAchievementPicture,
                Properties.Settings.Default.AchievementIconWidth,
                Properties.Settings.Default.AchievementIconHeight,
                Properties.Settings.Default.AchievementPictureStorageContainer);
        }

        /// <summary>
        /// Transfers the value of the properties from the model to
        /// the achievement.
        /// </summary>
        /// <param name="achievement">The achievement to transfer
        /// the properties to.</param>
        /// <param name="model">The model to transfer the
        /// properties from.</param>
        /// <returns>True if all the values were correctly parsable.</returns>
        private bool TransferProperties(AchievementItem achievement, AchievementModel model)
        {
            PropertyInfo info;
            foreach (AchievementProperty property in model.Properties)
            {
                info = achievement.GetType().GetProperty(property.PropertyName);
                if (info.PropertyType == typeof(int))
                {
                    //the integers are transferred
                    int i;
                    if (Int32.TryParse(property.Value, out i))
                    {
                        info.SetValue(achievement, i, null);
                    }
                    else
                    {
                        ViewBag.AchievementErrorMessage = String.Format(Resource.PropertyError, property.ShowName);
                        return false;
                    }
                }
                else //if (info.GetType() == typeof(String))
                {
                    //the strings are transferred
                    info.SetValue(achievement, property.Value, null);
                }
            }
            return true;
        }
    }
}

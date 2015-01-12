using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.Security;
using sGridServer.Code.Rewards;
using sGridServer.Code.DataAccessLayer;
using sGridServer.Models;
using System.Reflection;
using sGridServer.Code.DataAccessLayer.Models;
using Resource = sGridServer.Resources.Rewards.RewardRes;
using sGridServer.Code.Utilities;
using sGridServer.Properties;
using System.Drawing;

namespace sGridServer.Controllers
{
    /// <summary>
    /// This controller is responsible for managing the
    /// RewardCreateFormView.
    /// </summary>
    public class RewardController : Controller
    {
        /// <summary>
        /// The session key of the return url. 
        /// </summary>
        private const string ReturnUrlSessionKey = "RewardControllerReturnUrl";

        /// <summary>
        /// Gets or sets the url to return to after having finished
        /// execution.
        /// </summary>
        protected string ReturnUrl
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
        /// Cancels the creation/editing of the Reward object and
        /// returns to the ReturnUrl.
        /// </summary>
        /// <returns>A redirect to the ReturnUrl.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.CoinPartner | SiteRoles.Admin)]
        public ActionResult Cancel()
        {
            return Redirect(ReturnUrl);
        }

        /// <summary>
        /// Stores the return url and creates a model from an 
        /// existing reward, or a new reward depending on the id 
        /// and then returns the RewardCreateFormView.
        /// </summary>
        /// <param name="returnUrl">The url to return to. </param>
        /// <param name="rewardType">The type of the reward to create.</param>
        /// <param name="rewardId">The id of the reward to change,
        /// or -1, if a reward is to be created.</param>
        /// <returns>RewardCreateFormView with the created Reward as
        /// model.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.CoinPartner | SiteRoles.Admin)]
        public ActionResult RewardCreateForm(string returnUrl, string rewardType, int rewardId)
        {
            this.ReturnUrl = returnUrl;
            ViewBag.RewardErrorMessage = null;

            RewardManager manager = new RewardManager();
            RewardItem reward;
            RewardModel model = new RewardModel();
            
            if (rewardId < 0)
            {
                // -> new reward
                reward = manager.GetRewardFromType(rewardType);
            }
            else
            {
                // -> changed reward
                reward = RewardItem.ToRewardItem(manager.GetRewardById(rewardId));

                //test if the user is allowed to change this reward
                //it is allowed if: the user is an admin, or if the user is the partner of this reward and the reward is inactive
                if (!(
                    (SecurityProvider.CurrentUser.UserPermission == SiteRoles.Admin)
                    || ((SecurityProvider.Context.ID == reward.CoinPartnerId) && !manager.IsActive(reward.Reward))))
                {
                    throw new UnauthorizedAccessException("You are not allowed to change this reward.");
                }
            }
            model.RewardType = reward.RewardType;
            model.RewardId = reward.Id;
            model.Name = reward.Name;
            model.Description = reward.Description;
            model.ShortDescription = reward.ShortDescription;
            model.Icon = reward.Picture;

            /*
             * For every Property that can be changed (that is every
             * Property indicated by PropertyNames), an entry in the
             * dictionnary is made.
             */
            PropertyInfo info;
            foreach (string propName in reward.PropertyNames)
            {
                info = reward.GetType().GetProperty(propName);
                if (info.PropertyType == typeof(DateTime))
                {
                    model.DateTimeProperties.Add(new RewardProperty<DateTime>()
                    {
                        PropertyName = propName,
                        ShowName = propName, //Todo Jerome get localized name
                        Value = (DateTime)info.GetValue(reward, null)
                    });
                }
                else
                {
                    model.StringProperties.Add(new RewardProperty<string>()
                    {
                        PropertyName = propName,
                        ShowName = propName, //Todo Jerome get localized name
                        Value = info.GetValue(reward, null).ToString()
                    });
                }
            }

            return View(model);
        }

        /// <summary>
        /// Tries to submit the created/changed reward. In case of
        /// failure the RewardCreateFormView will be returned. If the
        /// submission succeeds, the user is redirected to ReturnUrl and
        /// the reward will be saved.
        /// </summary>
        /// <param name="model">The reward which is to save.</param>
        /// <param name="file">The file representing the icon.</param>
        /// <returns>The RewardCreateFormView or a redirect to ReturnUrl.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.CoinPartner | SiteRoles.Admin)]
        [HttpPost]
        public ActionResult Submit(RewardModel model, HttpPostedFileBase file)
        {
            ViewBag.RewardErrorMessage = null;

            RewardManager manager = new RewardManager();
            RewardItem reward;
            if (model.RewardId < 0)
            {
                // -> new reward
                reward = manager.GetRewardFromType(model.RewardType);

                //set the coin Partner
                if (SecurityProvider.CurrentUser.UserPermission == SiteRoles.Admin)
                {
                    reward.CoinPartnerId = model.CoinPartnerId;
                }
                else
                {
                    reward.CoinPartnerId = SecurityProvider.Context.ID;
                }
            }
            else
            {
                // -> changed reward
                reward = RewardItem.ToRewardItem(manager.GetRewardById(model.RewardId));

                //test the coin partner
                if (!(
                    (SecurityProvider.CurrentUser.UserPermission == SiteRoles.Admin)
                    || ((SecurityProvider.Context.ID == reward.CoinPartnerId) && !manager.IsActive(reward.Reward))))
                {
                    throw new UnauthorizedAccessException("You are not allowed to change this reward.");
                }
            }

            reward.Name = model.Name;
            reward.Description = model.Description;
            reward.ShortDescription = model.ShortDescription;

            //the icon is stored
            BlobStorage storage = new BlobStorage(Settings.Default.RewardIconStorageContainer);
            if (file != null)
            {
                if (reward.Picture != "")
                {
                    storage.RemoveBlob(reward.Picture);
                }
                reward.Picture = storage.StoreBlob(
                        ImageUtil.ResizeImage(file.InputStream,
                            Settings.Default.RewardIconWidth,
                            Settings.Default.RewardIconHeight,
                            Color.White));
            }
            else if (reward.Picture == "")
            {
                reward.Picture = GetDefaultRewardIcon();
            }

            //the other properties are now transferred from the
            //model to the reward and they are tested
            if (TransferProperties(reward, model))
            {
                string errorMessage;
                if (!reward.PrepareForSaving(out errorMessage))
                {
                    ViewBag.RewardErrorMessage = String.Format(Resource.Error, errorMessage);
                }
                else
                {
                    //now the reward is valid
                    if (model.RewardId < 0)
                    {
                        // -> new reward
                        manager.CreateReward(reward);
                    }
                    else
                    {
                        // -> changed reward
                        manager.EditReward(reward);
                    }
                    return Redirect(ReturnUrl);
                }
            }
            //the reward was not accepted
            return View("RewardCreateForm", model);
        }

        /// <summary>
        /// Copies the default reward icon into the blob storage and returns the storage url. 
        /// </summary>
        /// <returns>The url to the stored picture in the blob storage.</returns>
        public static string GetDefaultRewardIcon()
        {
            return ImageUtil.GetPicturyByUrl(Properties.Settings.Default.DefaulRewardIcon,
                Properties.Settings.Default.RewardIconWidth,
                Properties.Settings.Default.RewardIconHeight,
                Properties.Settings.Default.RewardIconStorageContainer);
        }

        /// <summary>
        /// Transfers the value of the properties from the model to
        /// the reward
        /// </summary>
        /// <param name="reward">The reward to transfer the
        /// properties to.</param>
        /// <param name="model">The model to transfer the properties
        /// from.</param>
        /// <returns>True if all the values were correctly parsable.</returns>
        private bool TransferProperties(RewardItem reward, RewardModel model)
        {
            PropertyInfo info;
            foreach (RewardProperty<string> stringProperty in model.StringProperties)
            {
                info = reward.GetType().GetProperty(stringProperty.PropertyName);
                if (info.PropertyType == typeof(int))
                {
                    //the integers are transferred
                    int i;
                    if (Int32.TryParse(stringProperty.Value, out i))
                    {
                        info.SetValue(reward, i, null);
                    }
                    else
                    {
                        ViewBag.RewardErrorMessage = String.Format(Resource.PropertyError, stringProperty.ShowName);
                        return false;
                    }
                }
                else //if (info.GetType() == typeof(string))
                {
                    //the strings are transferred
                    info.SetValue(reward, stringProperty.Value, null);
                }
            }
            foreach (RewardProperty<DateTime> dateTimeProperty in model.DateTimeProperties)
            {
                //the dates are transferred
                info = reward.GetType().GetProperty(dateTimeProperty.PropertyName);
                info.SetValue(reward, dateTimeProperty.Value, null);
            }
            return true;
        }
    }
}

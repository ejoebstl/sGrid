using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Models;
using sGridServer.Code.DataAccessLayer.Models;
using Resource = sGridServer.Resources.IdProviders.EMailIdProvider;
using sGridServer.Code.Utilities;
using sGridServer.Code.Security;
using System.Web.Mail;
using System.Net.Mail;

namespace sGridServer.Controllers
{
    /// <summary>
    /// This class is derived from the abstract IdProviderController class and provides authentication, 
    /// registration and password reset for users who want to register and authenticate themselves with 
    /// an e-mail address and password, which is managed by our application. 
    /// </summary>
    public class EMailIdProviderController : IdProviderController
    {
        /// <summary>
        /// The unique provider identifier for EMailIdProvider.
        /// </summary>
        public const string ProviderIdentifier = "EMail";

        /// <summary>
        /// Gets the Description of this provider. 
        /// </summary>
        public static IdProviderDescription Description
        {
            get
            {
                return new IdProviderDescription("EMailIdProvider", "~/Content/images/idProviders/email.png", "E-Mail");
            }
        }

        //Keys for session access. 
        private const string EMailIdProviderCaptchaKey = "EMailIdProviderCaptchaKey";
        private const string ResetPasswordTimeoutKey = "EMailIdProviderResetPasswordTimeout";
        private const string ResetPasswordUserIdKey = "EMailIdProviderResetPasswordUserId";
        private const string ResetPasswordQuestionAnsweredKey = "EMailIdProviderResetPasswordQuestionAnswered";
        private const string ResetPasswordQuestionCounterKey = "EMailIdProviderResetPasswordQuestionCounter";
        
        //Constants for password reset. 
        private const int ResetPasswortTimeout = 5;
        private const int ResetPasswordQuestionTryLimit = 3;

        /// <summary>
        /// Gets or sets an integer indicating the number of tries to enter a security question correctly. 
        /// </summary>
        private int ResetPasswordQuestionCounter
        {
            get
            {
                Nullable<int> value = (Nullable<int>)Session[ResetPasswordQuestionCounterKey];

                return value != null ? value.Value : 0; 
            }
            set
            {
                Session[ResetPasswordQuestionCounterKey] = new Nullable<int>(value);
            }
        }

        /// <summary>
        /// Shows the Login view. 
        /// </summary>
        /// <returns>The Login view. </returns>
        protected override ActionResult PerformAuthentication()
        {
            return View("Login", new Tuple<string, string>("", ReturnUrl));
        }

        /// <summary>
        /// Shows the Register view. 
        /// </summary>
        /// <returns>The Register view.</returns>
        protected override ActionResult PerformRegistration()
        {
            //We need a captcha for registration, so create one. 
            CaptchaGenerator captchaGenerator = new CaptchaGenerator(this.ControllerContext, EMailIdProviderCaptchaKey);
            EMailRegistrationData data =  new EMailRegistrationData();
            data.Captcha = captchaGenerator.CreateCaptcha();

            return View("Register", data);
        }

        /// <summary>
        /// Shows the SendPasswordResetMail view. 
        /// </summary>
        /// <returns></returns>
        protected override ActionResult PerformResetPassword()
        {
            CaptchaGenerator captchaGenerator = new CaptchaGenerator(this.ControllerContext, EMailIdProviderCaptchaKey);
            PasswordRecoveryMailData data = new PasswordRecoveryMailData();
            data.Captcha = captchaGenerator.CreateCaptcha();

            return View("SendPasswordResetMail", data);
        }

        /// <summary>
        /// Validates username and password. If valid, calls FinishAuthentication, if invalid shows the LoginView again with an error message and the entered address. 
        /// </summary>
        /// <param name="username">The e-mail address of the user who wants to log in.</param>
        /// <param name="password">The password of the user who wants to log in. </param>
        /// <returns>An ActionResult performing one of the operations above. </returns>
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            Account account = MemberManager.Accounts.Where(x => x.EMail == username).FirstOrDefault();

            //Check if the username was associated with an account. 
            if (account == null)
            {
                ViewBag.LoginMessage = Resource.InvalidUsername;
                return View("Login", new Tuple<string, string>(username, ReturnUrl));
            }

            //Check if the user is associated with this provider.  
            if (account.IdType != ProviderIdentifier)
            {
                ViewBag.LoginMessage = Resource.InvalidIdType;
                return View("Login", new Tuple<string, string>(username, ReturnUrl));
            }

            //Check if the password was correct. 
            if(!MemberManager.ValidatePassword(account, password))
            {
                ViewBag.LoginMessage = Resource.InvalidPassword;
                return View("Login", new Tuple<string, string>(username, ReturnUrl));
            }

            return FinishAuthentication(new Code.Security.UserContext(account));
        }

        /// <summary>
        /// Validates the captcha and the data entered by the user. If the data is valid, conducts the registration, 
        /// if not, shows the registration view again with validation messages and the entered data without the password. 
        /// </summary>
        /// <param name="data">The data entered by the user. </param>
        /// <returns>An ActionResult performing one of the operations above.</returns>
        [HttpPost]
        public ActionResult Register(EMailRegistrationData data)
        {
            //Check if the entered values were valid. 
            if (!ModelState.IsValid)
            {
                return View(data);
            }

            CaptchaGenerator captchaGenerator = new CaptchaGenerator(this.ControllerContext, EMailIdProviderCaptchaKey);

            bool error = false;

            //Check if the password and its confirmation matched.
            if (data.Password != data.PasswordConfirmation)
            {
                ViewBag.PasswordMessage = Resource.PasswordMismatch;
                error = true;
            }

            //Check if the captcha was entered correctly.
            if (!captchaGenerator.ValidateCaptcha(data.Captcha))
            {
                ViewBag.CaptchaMessage = Resource.CaptchaError;
                error = true;
            }

            //Check if the entered mail address is unique. 
            if(MemberManager.Accounts.Where(x => x.EMail == data.UserData.EMail).Any()) 
            {
                ViewBag.EMailMessage = Resource.EMailAlreadyInUse;
                error = true;
            }

            //Check if the entered nickname is unique. 
            if (MemberManager.Accounts.Where(x => x.Nickname == data.UserData.Nickname).Any())
            {
                ViewBag.UsernameMessage = Resource.UsernameAlreadyInUse;
                error = true;
            }

            //In case of an error, show the Register view again. 
            if (error)
            {
                data.Captcha = captchaGenerator.CreateCaptcha();
                ModelState.Clear();
                return View(data);
            }

            //If the entered data was valid, create the new user. 
            User newUser = data.UserData;

            newUser.Picture = GetDefaultProfilePicture();
            newUser.Culture = LanguageManager.CurrentLanguage.Code;
            newUser.IdType = ProviderIdentifier;
            newUser.RegistrationDate = DateTime.Now;
            newUser.Active = true;

            return FinishRegistration(newUser, true, data.Password);
        }

        /// <summary>
        /// Sets the user’s new password and then calls the FinishPasswordReset action method.
        /// </summary>
        /// <param name="resetPasswordData">The model object containing the data necassary to reset the password.</param>
        /// <returns>The ActionResult returned by FinishPasswordReset.</returns>
        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordData resetPasswordData)
        {
            //Check the model state.
            if (!ModelState.IsValid)
            {
                return View(resetPasswordData);
            }

            //Check whether the current user has a password restore procedure running. 
            Account account = CheckPasswordResetProcedureState();

            if (account is CoinPartner)
            {
                //If the user is a coin partner and got here without answering the security question, 
                //someone forget with requests. Fail. 
                if (Session[ResetPasswordQuestionAnsweredKey] == null)
                {
                    throw new InvalidOperationException();
                }
            }

            //If we have an id mismatch, someone forged some requests. Fail. 
            if (account.Id != resetPasswordData.UserId)
            {
                throw new NotImplementedException();
            }

            //If the password did not match its confirmation, show the view again. 
            if (resetPasswordData.PasswordConfirmation != resetPasswordData.Password)
            {
                ModelState.Clear();
                resetPasswordData.Password = "";
                resetPasswordData.PasswordConfirmation = "";
                ViewBag.PasswordMessage = Resource.PasswordMismatch;
                return View(resetPasswordData);
            }

            //Elese, set the password. 
            MemberManager.SetPassword(account, resetPasswordData.Password);

            //Then, clear the password reset state. 
            ClearPasswordResetState();

            return FinishPasswordReset(new UserContext(account));
        }

        /// <summary>
        /// Checks whether the current session is in a sufficient state for password recovery.
        /// </summary>
        /// <returns>The account of the currently associated user if the session is in the right state, throws an exception, else.</returns>
        private Account CheckPasswordResetProcedureState()
        {
            //If there is no state data, someone forged with requests. Fail. 
            if (Session[ResetPasswordTimeoutKey] == null || Session[ResetPasswordUserIdKey] == null)
            {
                throw new NotImplementedException();
            }

            DateTime timestamp = (DateTime)Session[ResetPasswordTimeoutKey];

            //If the state is expired, someone could be trying to re-use an old password reset request. Fail. 
            if (timestamp == null || (DateTime.Now - timestamp).TotalMinutes > ResetPasswortTimeout)
            {
                throw new NotImplementedException();
            }

            Account account = MemberManager.GetAccountById((int)Session[ResetPasswordUserIdKey]);

            //If there is no account to reset, someone forged with requests. Fail. 
            if (account == null)
            {
                throw new NotImplementedException();
            }

            return account;
        }

        /// <summary>
        /// Sets the given userid to the currently associated user for password recovery.
        /// </summary>
        /// <param name="userId">The id of the user to set.</param>
        private void SetPasswordResetState(int userId)
        {
            Session[ResetPasswordTimeoutKey] = DateTime.Now;
            Session[ResetPasswordUserIdKey] = userId;
        }

        /// <summary>
        /// Clears the password reset state.
        /// </summary>
        private void ClearPasswordResetState()
        {
            Session.Abandon();
        }

        /// <summary>
        /// Validates the given answer for the security question. If valid, 
        /// shows the PasswordResetView with a textbox for password reset, 
        /// else, shows the SecurityQuestionView again. The SecurityQuestionView may be 
        /// submitted at most three times. After that, the password recovery procedure 
        /// fails with an error message. 
        /// </summary>
        /// <param name="question">The answer to the question the user entered. </param>
        /// <returns>An ActionResult performing one of the operations above.</returns>
        [HttpPost]
        public ActionResult SecurityQuestion(SecurityQuestion question)
        {
            CoinPartner account = CheckPasswordResetProcedureState() as CoinPartner;

            //If the account was not found, or the security question is unset, someone forged with requests. Fail. 
            if(account == null || account.SecurityQuestion == "" || account.SecurityAnswer == "")
            {
                throw new NotImplementedException();
            }

            //If the question was incorrectly answered, show the view again. 
            if (question.Answer == null || !question.Answer.Equals(account.SecurityAnswer))
            {
                ViewBag.ErrorMessage = Resource.InvalidSecurityAnswer;
                question.Question = account.SecurityQuestion;
                question.Answer = "";
                ModelState.Clear();

                ResetPasswordQuestionCounter++;
                if (ResetPasswordQuestionCounter < ResetPasswordQuestionTryLimit)
                {
                    return View(question);
                }
                else
                {
                    Session.Abandon();
                    return View("SecurityQuestionInvalid");
                }
            }

            //Else, let the user reset his password. 

            //Set this session variable to a non-null value to signal that the coin partner has correctly
            //answered the question. 
            Session[ResetPasswordQuestionAnsweredKey] = new object();

            return View("ResetPassword", new ResetPasswordData() { UserId = account.Id });
        }

        /// <summary>
        /// Sends out a password recover mail to the given e-mail address, if there was a 
        /// user associated with it and the captcha was valid. If not, shows the password
        /// reset view again. After this, shows the PasswordResetView with a message which 
        /// tells the user to check his mails. If the user associated with the given e-mail 
        /// address is an sGrid team member, no e-mail is sent out and an error message is shown. 
        /// </summary>
        /// <param name="mailData">The data object containing e-mail address and captcha. </param>
        /// <returns>An ActionResult performing one of the actions above. </returns>
        [HttpPost]
        public ActionResult SendPasswordResetMail(PasswordRecoveryMailData mailData)
        {
            //Check the model state.
            if (!ModelState.IsValid)
            {
                return View(mailData);
            }

            CaptchaGenerator captchaGenerator = new CaptchaGenerator(this.ControllerContext, EMailIdProviderCaptchaKey);

            bool error = false;

            //Check the entered captcha.
            if (!captchaGenerator.ValidateCaptcha(mailData.Captcha))
            {
                ViewBag.CaptchaMessage = Resource.CaptchaError;
                error = true;
            }

            Account account = MemberManager.Accounts.Where(x => x.EMail == mailData.Address).FirstOrDefault();

            //Check whether the entered account was valid. 
            if (account == null)
            {
                ViewBag.EMailMessage = Resource.EMailNotFound;
                error = true;
            }
            else if (account.IdType != ProviderIdentifier)
            {
                ViewBag.EMailMessage = Resource.InvalidIdType;
                error = true;
            }

            //In case of an error, re-show the view. 
            if (error)
            {
                mailData.Captcha = captchaGenerator.CreateCaptcha();
                mailData.Address = "";
                ModelState.Clear();
                return View(mailData);
            }

            //If the user is a CoinPartner and has no security question set, redirect the user to an error page. 
            if (account.UserPermission == SiteRoles.CoinPartner && (((CoinPartner)account).SecurityAnswer == "" || ((CoinPartner)account).SecurityQuestion == ""))
            {
                return Redirect("SecurityQuestionUnset");
            }

            //Password reset for admins is not allowed. 
            if (account.UserPermission == SiteRoles.Admin)
            {
                throw new InvalidOperationException();
            }

            //Generate a password for usage as reset code. 
            account.PasswordResetCode = System.Web.Security.Membership.GeneratePassword(32, 5);

            MemberManager.SaveAccount(account);

            //Now, send out a password reset mail containing the URL to the reset page. 
            string subject = Resource.RecoveryEMailSubject;
            string body = String.Format(
                Resource.RecoveryEMailBody, 
                account.Nickname, 
                Url.Action("VerifyCode", "EMailIdProvider", new { code = account.PasswordResetCode, userId = account.Id, returnUrl = ReturnUrl}, Request.Url.Scheme ));

            System.Net.Mail.MailMessage emailMessage = new System.Net.Mail.MailMessage(NotificationMailer.SenderAddress, account.EMail, subject, body);

            SmtpClient mailer = new SmtpClient("localhost");

            mailer.Send(emailMessage);

            ViewBag.Success = true;

            //Then, show the MailSent view. 
            return Redirect("MailSent");
        }

        /// <summary>
        /// Shows the MailSent view. 
        /// </summary>
        /// <returns>The MailSent view.</returns>
        public ActionResult MailSent()
        {
            return View();
        }

        /// <summary>
        /// Shows the SecurityQuestionUnset view. 
        /// </summary>
        /// <returns>The SecurityQuestionUnset view.</returns>
        public ActionResult SecurityQuestionUnset()
        {
            return View();
        }

        /// <summary>
        /// Verifies the submitted password reset code. If invalid, shows an error message 
        /// on PasswordResetView, if valid, shows a text box to reset the password on 
        /// PasswordResetView, or, in case of a partner, shows the SecurityQuestionView 
        /// for that partner. This action method is usually called when the user clicks 
        /// on the password reset link in the password reset e-mail he receives. 
        /// </summary>
        /// <param name="code">The security code.</param>
        /// <param name="userId">The id of the user who wants to reset his password. </param>
        /// <param name="returnUrl">The return url to return to. </param>
        /// <returns>An ActionResult object performing one of the operations above. </returns>
        public ActionResult VerifyCode(string code, int userId, string returnUrl)
        {
            Account account = MemberManager.GetAccountById(userId);

            //If the given account was not found, someone forged with requests. Fail. 
            if (account == null)
            {
                throw new NotImplementedException();
            }

            //If the given account has no passwort reset code set, or the reset code is wrong, someone forged with accounts. Fail. 
            if (account.PasswordResetCode == "" || !account.PasswordResetCode.Equals(code))
            {
                throw new NotImplementedException();
            }

            //Else, continue with password reset procedure. 

            //Set the return url. 
            this.ReturnUrl = returnUrl;

            //Clear the used password reset code. 
            account.PasswordResetCode = "";
            MemberManager.SaveAccount(account);

            //Set the password reset state to the current session. 
            SetPasswordResetState(userId);
            
            if (account.UserPermission == SiteRoles.CoinPartner)
            {
                //If the current user is a CoinPartner, redirect him to the SecurityQuestion view, if applicable. 
                CoinPartner partner = account as CoinPartner;
                if (partner.SecurityQuestion == "" || partner.SecurityAnswer == "")
                {
                    return View("SecurityQuestionUnset");
                }
                else
                {
                    return View("SecurityQuestion", new SecurityQuestion() { Question = partner.SecurityQuestion, Answer = "" });
                }
            }
            else
            {
                //Else show the ResetPassword view. 
                return View("ResetPassword", new ResetPasswordData() { UserId = userId });
            }
        }

         
    }
}

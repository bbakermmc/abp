﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Account.Web.Settings;
using Volo.Abp.Identity;
using Volo.Abp.Settings;
using Volo.Abp.UI;
using Volo.Abp.Uow;

namespace Volo.Abp.Account.Web.Pages.Account
{
    public class RegisterModel : AccountPageModel
    {
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ReturnUrlHash { get; set; }

        [BindProperty]
        public PostInput Input { get; set; }

        public virtual async Task OnGet()
        {
            await CheckSelfRegistrationAsync();
        }

        [UnitOfWork] //TODO: Will be removed when we implement action filter
        public virtual async Task<IActionResult> OnPostAsync()
        {
            ValidateModel();

            await CheckSelfRegistrationAsync();

            var user = new IdentityUser(GuidGenerator.Create(), Input.UserName, Input.EmailAddress, CurrentTenant.Id);

            var result = await UserManager.CreateAsync(user, Input.Password);

            if (!result.Succeeded)
            {
                throw new NotImplementedException();
            }

            await UserManager.SetEmailAsync(user, Input.EmailAddress);

            await SignInManager.SignInAsync(user, isPersistent: false);

            return Redirect(ReturnUrl ?? "/"); //TODO: How to ensure safety? IdentityServer requires it however it should be checked somehow!
        }

        protected virtual async Task CheckSelfRegistrationAsync()
        {
            if (!await SettingManager.IsTrueAsync(AccountSettingNames.IsSelfRegistrationEnabled))
            {
                throw new UserFriendlyException(L["SelfRegistrationDisabledMessage"]);
            }
        }

        public class PostInput
        {
            [Required]
            [StringLength(32)]
            [DisplayName(nameof(UserName))]
            public string UserName { get; set; }

            [Required]
            [EmailAddress]
            [StringLength(255)]
            [DisplayName(nameof(EmailAddress))]
            public string EmailAddress { get; set; }

            [Required]
            [StringLength(32)]
            [DataType(DataType.Password)]
            [DisplayName(nameof(Password))]
            public string Password { get; set; }
        }
    }
}

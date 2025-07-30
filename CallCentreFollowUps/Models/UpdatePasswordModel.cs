using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using DataType = System.ComponentModel.DataAnnotations.DataType;

namespace CallCentreFollowUps.Models
{
    public class UpdatePasswordModel
    {

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Old Password")]
            public string OldPassword { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "New Password")]
            public string NewPassword { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            [Display(Name = "Confirm New Password")]
            public string ConfirmNewPassword { get; set; }
        
    }
}
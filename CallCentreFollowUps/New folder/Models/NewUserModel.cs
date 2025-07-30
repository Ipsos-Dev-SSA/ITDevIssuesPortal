using System.ComponentModel.DataAnnotations;

namespace CallCentreFollowUps.Models
{
    public class NewUserModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "First Name cannot exceed 100 characters.")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Last Name cannot exceed 100 characters.")]
        public string LastName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
        public string Password { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Role Name cannot exceed 100 characters.")]
        public string RoleName { get; set; }

     
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        public int CountryID { get; set; }
       
        public int TeamID { get; set; }
    }
}

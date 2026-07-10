using System.ComponentModel.DataAnnotations;

namespace HexaERP.Entities.Models.UserManagement
{
    class LoginViewModel
    {
        [Required]
        public long UserId { get; set; }
        [Required]
        public string UserName { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        //[HiddenInput(DisplayValue = false)]
        //public int OrgId { get; set; }
    }
}

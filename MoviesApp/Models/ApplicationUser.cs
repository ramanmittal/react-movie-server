
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
namespace MoviesApp.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        public byte[] ImageData { get; set; }

        public virtual ICollection<Movie> Movies { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    }


}

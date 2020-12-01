using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesApp.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Revoked { get; set; }

        public string ReplacedByToken { get; set; }

        public bool IsActive => Revoked == null && !IsExpired;

        public bool IsExpired => DateTime.UtcNow >= Expires;
        public virtual ApplicationUser ApplicationUser { get; set; }
        public int UserId { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesApp.Models
{
    public class UserlistModel
    {
        public int Total { get; set; }
        public List<UserlistItemModel> Data { get; set; }
    }
    public class UserlistItemModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MoviesApp.Models;

namespace MoviesApp.Controllers
{
    public class UserController : Controller
    {
        [HttpPost]
        [Authorize(Roles= "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Index([FromBody]CreateUserModel model, [FromServices] MovieContext movieContext, [FromServices]UserManager<ApplicationUser> userManager)
        {
            if (ModelState.IsValid)
            {
                using (var tr = movieContext.Database.BeginTransaction())
                {
                    var user = new ApplicationUser
                    {
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        UserName = model.Email,
                    };
                    Func<byte[]> exp = () =>
                      {
                          using (MemoryStream ms = new MemoryStream())
                          {
                              model.ImageData.CopyTo(ms);
                              return ms.ToArray();
                          }
                      };
                    user.ImageData = model.ImageData == null ? (byte[])null : exp();
                    try
                    {
                        var result = await userManager.CreateAsync(user, model.Password);
                        if (result.Succeeded)
                        {
                            user = await userManager.FindByEmailAsync(user.Email);
                            await userManager.AddToRoleAsync(user, Roles.User.ToString()); 
                        }
                        tr.Commit();
                    }
                    catch (Exception)
                    {
                        tr.Rollback();
                        throw;
                    }
                }
                return Ok();
            }
            return BadRequest();
        }

        [HttpPut]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Update([FromBody]UpdateUserModel model, [FromServices] MovieContext movieContext, [FromServices]UserManager<ApplicationUser> userManager)
        {
            if (ModelState.IsValid)
            {
                using (var tr = movieContext.Database.BeginTransaction())
                {
                    var user = movieContext.Users.FirstOrDefault(x => x.Id == model.Id);
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    Func<byte[]> exp = () =>
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            model.ImageData.CopyTo(ms);
                            return ms.ToArray();
                        }
                    };
                    user.ImageData = model.ImageData == null ? (byte[])null : exp();
                    try
                    {
                        await movieContext.SaveChangesAsync();
                        tr.Commit();
                    }
                    catch (Exception)
                    {
                        tr.Rollback();
                        throw;
                    }
                }
                return Ok();
            }
            return BadRequest();
        }




        [HttpGet]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAll([FromServices] MovieContext movieContext, [FromServices]UserManager<ApplicationUser> userManager, [FromServices]IConfiguration _configuration, int pagenumber = 1, int pagesize = 10)
        {
            var users = movieContext.Users.Where(x => x.UserName != _configuration["AdminEmail"]);
            return Ok(new UserlistModel
            {
                Data = users.Skip((pagenumber - 1) * pagesize).Take(pagesize).Select(x => new UserlistItemModel
                {
                    Id = x.Id,
                    Email = x.Email,
                    FirstName = x.FirstName,
                    LastName = x.LastName
                }).ToList(),
                Total = users.Count()
            });
        }
        [HttpGet]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetUser(int id, [FromServices] MovieContext movieContext)
        {
            return Ok(movieContext.Users.Where(x => x.Id == id).Select(x => new UserlistItemModel
            {
                Id = x.Id,
                Email = x.Email,
                FirstName = x.FirstName,
                LastName = x.LastName
            }).FirstOrDefault());
        }

        [HttpDelete]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult DeleteUser(int id, [FromServices] MovieContext movieContext)
        {
            var user = movieContext.Users.FirstOrDefault(x => x.Id == id);
            movieContext.Users.Remove(user);
            movieContext.SaveChanges();
            return Ok();
        }

        [HttpGet]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> IsEmailAvailiable([FromQuery]string email, [FromServices]MovieContext movieContext)
        {
            return Ok(! await movieContext.Users.AnyAsync(x => x.Email == email));
        }
    }
}
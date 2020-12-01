using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviesApp.Models;

namespace MoviesApp.Controllers
{
    public class MovieController : Controller
    {
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult List([FromServices] MovieContext movieContext, int pagenumber = 1, int pagesize = 10, int? userId = null)
        {
            if (User.IsInRole(Roles.Admin.ToString()))
            {
                if (!userId.HasValue)
                {
                    return BadRequest();
                }
            }
            else
            {
                userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            }
            var movies = movieContext.Movies.Where(x => x.UserId == userId.Value).Select(x => new
            {
                x.Id,
                x.Title,
                x.Director,
                x.DateReleased
            });
            return Ok(new
            {
                Total = movies.Count(),
                Data = movies.Skip((pagenumber - 1) * pagesize).Take(pagesize).ToList(),
            });


        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Index(int id, [FromServices] MovieContext movieContext)
        {
            return Ok(movieContext.Movies.Where(x => x.Id == id).Select(x => new { x.Id, x.Title, x.Director, x.DateReleased }).FirstOrDefault());
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public IActionResult Delete(int id,[FromServices] MovieContext movieContext)
        {
            movieContext.Movies.Remove(movieContext.Movies.First(x => x.Id == id));
            movieContext.SaveChanges();
            return Ok();
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public IActionResult CreateMovie([FromBody]CreateMovieModel model, [FromServices] MovieContext movieContext)
        {
            var movie = new Movie
            {
                DateReleased = model.DateReleased,
                Title = model.Title,
                Director = model.Director,
                UserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier))
            };
            movieContext.Movies.Add(movie);
            movieContext.SaveChanges();
            return Ok(movie.Id);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public IActionResult EditMovie([FromBody]EditMovieModel model, [FromServices] MovieContext movieContext)
        {
            var movie = movieContext.Movies.First(x => x.Id == model.Id);
            movie.DateReleased = model.DateReleased;movie.Director = model.Director; movie.Title = model.Title;
            movieContext.SaveChanges();
            return Ok();
        }
    }
}
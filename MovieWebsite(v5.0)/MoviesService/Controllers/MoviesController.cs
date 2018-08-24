﻿
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using MoviesService.Data;
using MoviesService.Models;
using MoviesService.ViewModels;
using MoviesService.IntegrationEvents.Events;
using MoviesService.IntegrationEvents;

namespace MoviesService.Controllers
{
    [Route("MoviesService")]
    public class MoviesController : Controller
    {
        private readonly MovieContext _context;
        private readonly IMovieIntegrationEventService _movieIntegrationEventService;

        public MoviesController(MovieContext context,IMovieIntegrationEventService movieIntegrationEventService)
        {
            _context = context;
            _movieIntegrationEventService = movieIntegrationEventService;
        }

        //GET: MoviesService/GetSelectList
        [Route("GetSelectList")]
        public async Task<IActionResult> GetSelectList()
        {
            var companys = await _context.PublishCompany.ToListAsync<PublishCompany>();
            return Ok(GetSelectListCompanys(companys,null));
        }

        //POST: MoviesService/CreateMovie
        //Must use [FromBody] attribute, or the movie will be null
        [HttpPost]
        [Route("CreateMovie")]
        public async Task<IActionResult> CreateMovie([FromBody]Movie movie)
        {
            _context.Add(movie);
            await _context.SaveChangesAsync();
            return Ok(movie);
        }

        //GET: MoviesService/ShowMovie
        [Route("ShowMovie")]
        public async Task<IActionResult> ShowMovie(int ? id)
        {
            Movie movie = await _context.Movie.SingleOrDefaultAsync(m => m.ID == id);
            var companys = await _context.PublishCompany.ToListAsync<PublishCompany>();
            var movieitemVM = new MovieItemViewModel
            {
                Movie = movie,
                Companys = GetSelectListCompanys(companys, movie.CompanyID)
            };

            return Ok(movieitemVM);
        }

        //GET: MoviesService/UpdateMovie
        //Must use [FromBody] attribute, or the movie will be null
        [HttpPost]
        [Route("UpdateMovie")]
        public async Task<IActionResult> UpdateMovie([FromBody]Movie movie)
        {
            _context.Update(movie);
            var basketitem = new BasketItem()
            {
                Id = movie.ID,
                Title = movie.Title,
                Price = movie.Price,
                CompanyName = movie.PublishCompany.CompanyName,
                Quantity = 1
            };
           // Create Integration Event to be published through the Event Bus
            var priceChangedEvent = new MovieDetailChangedIntegrationEvent(movie.ID, basketitem);
           // Achieving atomicity between original Catalog database operation and the IntegrationEventLog thanks to a local transaction
           await _movieIntegrationEventService.SaveEventAndMovieContextChangesAsync(priceChangedEvent);
           // Publish through the Event Bus and mark the saved event as published
           await _movieIntegrationEventService.PublishThroughEventBusAsync(priceChangedEvent);
            return Ok();
        }

    //POST: MoviesService/DeleteMovie
        [HttpPost]
        [Route("DeleteMovie")]
        public async Task<IActionResult> DeleteMovie(int ? id)
        {
            var movie = await _context.Movie.SingleOrDefaultAsync(m => m.ID == id);
            _context.Movie.Remove(movie);
            // Create Integration Event to be published through the Event Bus
            var movieDeleteEvent = new MovieDeleteIntegrationEvent(movie.ID);
            // Achieving atomicity between original Catalog database operation and the IntegrationEventLog thanks to a local transaction
            await _movieIntegrationEventService.SaveEventAndMovieContextChangesAsync(movieDeleteEvent);
            // Publish through the Event Bus and mark the saved event as published
            await _movieIntegrationEventService.PublishThroughEventBusAsync(movieDeleteEvent);
            return Ok();
        }

        //GET: MoviesService/ShowMovies
        [HttpGet]
        [Route("ShowMovies")]
        public async Task<IActionResult> ShowMovies()
        {
            var MovieList = await _context.Movie.ToListAsync();
            return Ok(MovieList);
        }

        [HttpGet]
        [Route("GetCompanyName")]
        public async Task<IActionResult> GetCompanyName(int? CompanyID)
        {
            var company = await _context.PublishCompany.SingleOrDefaultAsync(pc => pc.CompanyID == CompanyID);
            var companyname = company.CompanyName;
            return Ok(companyname);
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.ID == id);
        }

        public List<SelectListItem> GetSelectListCompanys(List<PublishCompany> list,int ? CompanyID)
        {
            var items = new List<SelectListItem>();
            foreach(PublishCompany c in list)
            {
                var item = new SelectListItem() { Value = c.CompanyID.ToString(), Text = c.CompanyID.ToString() + " " + c.CompanyName };
                if (CompanyID != null)
                {
                    if (item.Value.Equals(CompanyID.ToString()))
                    {
                        item.Selected = true;
                    }
                }
                items.Add(item);
            }
           
            return items;
        }

       
    }
}

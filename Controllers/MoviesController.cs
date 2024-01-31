﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoviePro.Data;
using MoviePro.Models.Database;
using MoviePro.Models.Settings;
using MoviePro.Services.Interfaces;

namespace MoviePro.Controllers
{
    public class MoviesController : Controller
    {

        private readonly AppSettings _appSettings;
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly IRemoteMovieService _tmdbMovieService;
        private readonly IDataMappingService _tmdbMappingService;

        public MoviesController(IOptions<AppSettings> appSettings, ApplicationDbContext context, IImageService imageService, IRemoteMovieService tmdbMovieService, IDataMappingService tmdbMappingService)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _imageService = imageService;
            _tmdbMovieService = tmdbMovieService;
            _tmdbMappingService = tmdbMappingService;
        }

        // GET Import 
        public async Task<IActionResult> Import()
        {

            var movies = await _context.Movie.ToListAsync();
            return View(movies);
        }


        // POST Import 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(int id)
        {
            // If we already have this movie we can just warn the user instead of importing it again
            if(_context.Movie.Any(m => m.MovieId == id))
            {
                var localMovie = await _context.Movie.FirstOrDefaultAsync(m => m.MovieId == id);
                
                return RedirectToAction("Details", "Movies", new { id = localMovie.Id, local = true });
            }

            // Step 1: Get the raw data from the API
            var movieDetail = await _tmdbMovieService.MovieDetailAsync(id);

            // Step 2: Run the data through a mapping procedure
            var movie = await _tmdbMappingService.MapMovieDetailAsync(movieDetail);


            // Step 3: Add the new movie
            _context.Add(movie);
            await _context.SaveChangesAsync();

            // Step 4: Assign it to the deafult All Collection
            await AddToMovieCollection(movie.Id, _appSettings.MovieProSettings.DefaultCollection.Name);

            return RedirectToAction("Import");
        
        }

        //GET 
        public async Task<IActionResult> Library()
        {
            var movies = await _context.Movie.ToListAsync();

            return View(movies);
        }


        private async Task AddToMovieCollection(int movieId, string collectionName)
        {
            var collection = await _context.Collection.FirstOrDefaultAsync(c => c.Name == collectionName);
            _context.Add(
                new MovieCollection()
                {
                    CollectionId = collection.Id,
                    MovieId = movieId,
                }
            );
            await _context.SaveChangesAsync();
        }

        private async Task AddToMovieCollection(int movieId, int collectionId)
        {
            _context.Add(
                new MovieCollection()
                {
                    CollectionId = collectionId,
                    MovieId = movieId,
                }
            );

            await _context.SaveChangesAsync();
        }

    }
}
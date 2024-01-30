using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviePro.Data;
using MoviePro.Models;
using MoviePro.Models.Database;
using MoviePro.Models.ViewModels;
using MoviePro.Services.Interfaces;
using MoviePro.Enums;
using System.Diagnostics;

namespace MoviePro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IRemoteMovieService _tmdbMovieService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IRemoteMovieService tmdbMovieService)
        {
            _logger = logger;
            _context = context;
            _tmdbMovieService = tmdbMovieService;
        }

        public async Task<IActionResult> Index()
        {
            const int count = 16;

            var data = new LandingPageVM()
            {
                CustomCollections = await _context.Collection
                                          .Include(c => c.MovieCollection)
                                          .ThenInclude(mc => mc.Movie).ToListAsync(),
                NowPlaying = await _tmdbMovieService.SearchMoviesAsync(MovieCategory.now_playing, count),
                Popular = await _tmdbMovieService.SearchMoviesAsync(MovieCategory.now_playing, count),
                TopRated = await _tmdbMovieService.SearchMoviesAsync(MovieCategory.now_playing, count),
                Upcoming = await _tmdbMovieService.SearchMoviesAsync(MovieCategory.now_playing, count)
            };

            return View(data);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

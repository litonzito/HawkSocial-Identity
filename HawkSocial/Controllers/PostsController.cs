using HawkSocial.Data;
using HawkSocial.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HawkSocial.Controllers
{
    public class PostsController : Controller
    {
        private readonly HawkSocialDbContext _context;
        private readonly UserManager<IdentityUser> _users;
        private static readonly TimeSpan EditWindow = TimeSpan.FromMinutes(5); // ventana de edicion de posts 
        private const int PageSize = 25; // cantidad de posts por pagina en el feed 

        public PostsController(HawkSocialDbContext context, UserManager<IdentityUser> users)
        {
            _context = context;
            _users = users;
        }

        // FEED (HOME) //
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var feed = await _context.Posts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAtUtc)
                .Take(PageSize)
                .AsNoTracking() // mejora el rendimiento ademas de que no se haran cambios desde el feed, es puramente lectura
                .ToListAsync();

            return View(feed);
        }

        // POST : se requiere autorizacion para crear posts //
        [Authorize, HttpPost]
        public async Task<IActionResult> Create(Post input) 
        {
            // validacion basica del contenido del post //
            if (!ModelState.IsValid) return RedirectToAction("Index");

            var user = await _users.GetUserAsync(User);
            if (user == null) return Challenge(); // en caso de que el usuario no este autenticado, redirige al login

            var post = new Post
            {
                UserId = user.Id,
                Content = (input.Content ?? string.Empty).Trim(),
                CreatedAtUtc = DateTimeOffset.UtcNow
            };

            _context.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: Editar post //
        [Authorize, HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var user = await _users.GetUserAsync(User);
            if (user == null) return Challenge(); // en caso de que el usuario no este autenticado, redirige al login
            if (post.UserId != user!.Id) return Forbid();

            // verifica si el post esta dentro de la ventana de edicion permitida 
            if (DateTimeOffset.UtcNow - post.CreatedAtUtc >= EditWindow) return Forbid();
            
            return View(post);
        }

        // POST: Editar post //
        [Authorize, HttpPost]
        public async Task<IActionResult> Edit(int id, Post input)
        {
            if (!ModelState.IsValid) return View(input);

            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var user = await _users.GetUserAsync(User);
            if (user == null) return Challenge(); // en caso de que el usuario no este autenticado, redirige al login
            if (post.UserId != user!.Id) return Forbid();

            // verifica si el post esta dentro de la ventana de edicion permitida 
            if (DateTimeOffset.UtcNow - post.CreatedAtUtc >= EditWindow) return Forbid();

            post.Content = input.Content.Trim();
            post.IsEdited = true;
            post.UpdatedAtUtc = DateTimeOffset.UtcNow;
            _context.Update(post);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}

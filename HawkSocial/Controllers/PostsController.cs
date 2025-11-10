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
        public async Task<IActionResult> Feed()
        {
            var feed = await _context.Posts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAtUtc)
                .Take(PageSize)
                .AsNoTracking() // mejora el rendimiento ademas de que no se haran cambios desde el feed, es puramente lectura
                .ToListAsync();

            return View("Feed",feed);
        }

        // POST : se requiere autorizacion para crear posts //
        [Authorize, HttpPost]
        public async Task<IActionResult> Create(EditPostViewModel input)      

        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return Challenge(); // en caso de que el usuario no este autenticado, redirige al login
            // validacion basica del contenido del post //
            if (!ModelState.IsValid) return RedirectToAction("Feed", "Posts");

            var post = new Post
            {
                UserId = user.Id,
                Content = (input.Content ?? string.Empty).Trim(),
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Feed", "Posts");
        }

        // GET: Editar post //
        [Authorize, HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return Challenge(); // en caso de que el usuario no este autenticado, redirige al login

            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            if (post.UserId != user!.Id) return Forbid();

            // verifica si el post esta dentro de la ventana de edicion permitida(5 minutos) 
            if (DateTime.UtcNow - post.CreatedAtUtc >= EditWindow) return Forbid();
            var contentPost = new EditPostViewModel
            {
                Content = post.Content
            };
            return View(contentPost);
        }

        // POST: Editar post // - Se utiliza post ya qe los formularios HTML no soportan PUT o PATCH
        [Authorize, HttpPost]
        public async Task<IActionResult> Edit(int id, EditPostViewModel input)
        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return Challenge(); // en caso de que el usuario no este autenticado, redirige al login

            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            if (post.UserId != user!.Id) return Forbid();

            // verifica si el post esta dentro de la ventana de edicion permitida(5 minutos) 
            if (DateTime.UtcNow - post.CreatedAtUtc >= EditWindow) return Forbid();

            if (!ModelState.IsValid) return View(input);

            post.Content = input.Content.Trim();
            post.IsEdited = true;
            post.UpdatedAtUtc = DateTime.UtcNow;

            // _context.Update(post); -- no es necesario llamar a Update ya que el post ya esta siendo rastreado y modificado en las instrucciones de arriba. La parte de actualizar se hace abajo con SaveChangesAsync
            await _context.SaveChangesAsync();
            return RedirectToAction("Feed", "Posts");
        }
    }
}

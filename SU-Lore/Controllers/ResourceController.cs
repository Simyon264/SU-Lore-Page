using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SU_Lore.Data;
using SU_Lore.Database;
using SU_Lore.Database.Models.Accounts;
using SU_Lore.Helpers;
using File = SU_Lore.Database.Models.File;

namespace SU_Lore.Controllers;

[Controller]
[Route("/resources")]
public class ResourceController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly AuthenticationHelper _authHelper;
    private readonly IMemoryCache _cache;
    public ResourceController(ApplicationDbContext context, AuthenticationHelper authHelper, IMemoryCache cache)
    {
        _context = context;
        _authHelper = authHelper;
        _cache = cache;
    }
    
    /// <summary>
    /// Fetches an uploaded file from the server based on the path provided. This is used for fetching images, audio, etc.
    /// </summary>
    [HttpGet]
    [Route("/resources/{path}")]
    public IActionResult GetResource(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return NotFound();
        }
        
        var tempLocation = GetTemporaryFilePath(path);
        // Check if the file is on disk
        if (System.IO.File.Exists(tempLocation))
        {
            // ok cool, now we check the memory cache in order to get the mime type
            if (_cache.TryGetValue(path, out string? mimeType))
            {
                return PhysicalFile(tempLocation, mimeType, enableRangeProcessing: true);
            }
            // Shrug, we'll just read the file from the DB again
        }
        
        var file =_context.Files.FirstOrDefault(f => f.Name + f.Extension == path);
        if (file == null)
        {
            return NotFound();
        }
        
        WriteByteArrayToTemporaryFile(file.Data, path);
        _cache.Set(path, file.MimeType, new MemoryCacheEntryOptions()
        {
            Priority = CacheItemPriority.NeverRemove,
        });

        return PhysicalFile(tempLocation, file.MimeType, enableRangeProcessing: true);
    }
    
    [HttpPut]
    [Route("/resources/")]
    public async Task<IActionResult> UploadResource(IFormCollection? form)
    {
        if (form == null)
        {
            return BadRequest("No file was uploaded.");
        }
        var file = form.Files.FirstOrDefault();
        if (file == null)
        {
            return BadRequest("No file was uploaded.");
        }

        var path = file.FileName;
        
        // path cannot contain slashes, as it is used as a path in the URL.
        if (path.Contains("/"))
        {
            return BadRequest("The file name cannot contain slashes.");
        }
        
        // Check if the file extension is allowed.
        if (!Constants.AllowedExtensions.Contains(Path.GetExtension(file.FileName)))
        {
            return BadRequest("The file extension is not allowed; Allowed extensions: " + string.Join(", ", Constants.AllowedExtensions));
        }
        
        
        var account = await _authHelper.FetchAccount(User);
        if (account == null)
        {
            return Unauthorized("You must be logged in to upload files.");
        }

        if (!account.Roles.HasRole(Role.Whitelisted))
        {
            return Unauthorized("You do not have permission to upload files.");
        }
        
        var existingFile = _context.Files.FirstOrDefault(f => f.Name + f.Extension == path.ToString());
        if (existingFile != null)
        {
            return Conflict("A file with the same name already exists. PATCH, DELETE or pick a different name.");
        }
        
        var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var data = ms.ToArray();
        
        var newFile = new File()
        {
            Name = Path.GetFileNameWithoutExtension(path.ToString()),
            Extension = Path.GetExtension(path.ToString()),
            MimeType = file.ContentType,
            Data = data,
            Size = file.Length,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = account
        };
        
        _context.Files.Add(newFile);
        await _context.SaveChangesAsync();
        
        return Ok();
    }
    
    private string GetTemporaryFilePath(string path)
    {
        return Path.Combine(Path.GetTempPath(), path);
    }
    
    private async void WriteByteArrayToTemporaryFile(byte[] data, string path)
    {
        var cacheLocation = Path.Combine(Path.GetTempPath(), path);
        // Ensure the path is only a file name.
        var pathParts = path.Split("/");
        if (pathParts.Length > 1)
        {
            throw new Exception("The path cannot contain slashes.");
        }
        
        await System.IO.File.WriteAllBytesAsync(cacheLocation, data);
    }
}
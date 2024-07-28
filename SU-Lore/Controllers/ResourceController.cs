using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using SU_Lore.Data;
using SU_Lore.Database;
using SU_Lore.Database.Models;
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
        
        HttpContext.Request.Headers.Remove("If-Modified-Since");
        HttpContext.Request.Headers.Remove("If-None-Match");
        
        var tempPath = GetTempPathForFile(path);
        
        if (_cache.TryGetValue(path, out string? fileMimeType))
        {
            if (System.IO.File.Exists(tempPath))
            {
                return PhysicalFile(tempPath, fileMimeType, enableRangeProcessing: true);
            }
        }
        
        var file =_context.Files.FirstOrDefault(f => f.Name + f.Extension == path);
        if (file == null)
        {
            return NotFound();
        }
        
        var fullFile = RetrieveFileFromDatabase(file);
        if (fullFile.Length == 0)
        {
            return StatusCode(500, "Failed to retrieve file from database. The file is empty.");
        }
        
        System.IO.File.WriteAllBytes(tempPath, fullFile);
        _cache.Set(file.Name + file.Extension, file.MimeType, new MemoryCacheEntryOptions()
        {
            Priority = CacheItemPriority.NeverRemove,
        });
        
        return PhysicalFile(tempPath, file.MimeType, enableRangeProcessing: true);
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
            return Conflict("A file with the same name already exists.");
        }
        
        var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var data = ms.ToArray();
        
        var newFile = new File()
        {
            Name = Path.GetFileNameWithoutExtension(path.ToString()),
            Extension = Path.GetExtension(path.ToString()),
            MimeType = file.ContentType,
            Size = file.Length,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = account
        };
        
        _context.Files.Add(newFile);
        await _context.SaveChangesAsync();

        try
        {
            StoreFileInDatabase(newFile, data);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to store file in database.");
            // Delete the file from the database.
            _context.Files.Remove(newFile);
            await _context.SaveChangesAsync();
            return StatusCode(500, "Failed to store file in database.");
        }
        
        return Ok();
    }

    public static string GetTempPathForFile(string file)
    {
        if (string.IsNullOrWhiteSpace(file))
        {
            throw new ArgumentException("File path cannot be empty.");
        }
        
        if (file.Contains("/"))
        {
            throw new ArgumentException("File path cannot contain slashes.");
        }
        
        return Path.Combine(Path.GetTempPath(), file);
    }
    
    private void StoreFileInDatabase(File file, byte[] fullFile)
    {
        var chunkSize = file.ChunkSize;
        var totalChunks = (fullFile.Length + chunkSize - 1) / chunkSize;
        
        for (var i = 0; i < totalChunks; i++)
        {
            var offset = i * chunkSize;
            var length = Math.Min(chunkSize, fullFile.Length - offset);
            var chunkData = new byte[length];
            Array.Copy(fullFile, offset, chunkData, 0, length);

            var chunk = new FileChunk
            {
                FileId = file.Id,
                ChunkNumber = i,
                Data = chunkData
            };
            _context.FileChunks.Add(chunk);
        }
        
        _context.SaveChanges();
    }
    
    private byte[] RetrieveFileFromDatabase(File file)
    {
        var chunks = _context.FileChunks.Where(c => c.FileId == file.Id).OrderBy(c => c.ChunkNumber).ToList();
        var fullFile = new byte[file.Size];
        
        var offset = 0;
        foreach (var chunk in chunks)
        {
            Array.Copy(chunk.Data, 0, fullFile, offset, chunk.Data.Length);
            offset += chunk.Data.Length;
        }
        
        return fullFile;
    }
}
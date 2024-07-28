using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SU_Lore.Database.Models.Accounts;

namespace SU_Lore.Database.Models;

/// <summary>
/// A file that is uploaded and stored in the database. This is used for images, audio, etc.
/// </summary>
[PrimaryKey("Id")]
public class File
{
    /// <summary>
    /// The unique identifier of the file.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// The name of the file.
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// The extension of the file. For example, .png, .jpg, .mp3, etc.
    /// </summary>
    public required string Extension { get; set; }
    
    /// <summary>
    /// The Mime type used by browsers to determine how to handle the file. For example, image/png, image/jpeg, audio/mpeg, etc.
    /// </summary>
    public required string MimeType { get; set; }
    
    /// <summary>
    /// The size of the file in bytes.
    /// </summary>
    public required long Size { get; set; }
    
    /// <summary>
    /// When was this file uploaded?
    /// </summary>
    public required DateTime UploadedAt { get; set; }
    
    /// <summary>
    /// The account that uploaded this file.
    /// </summary>
    [ForeignKey("Account")]
    public required Account UploadedBy { get; set; }
    
    /// <summary>
    /// The chunks that make up this file. This is used for large files that are split into smaller chunks.
    /// Defaults to 1024 * 1024 * 10 bytes (10MB).
    /// </summary>
    public int ChunkSize { get; set; } = 1024 * 1024 * 10;
}

[PrimaryKey("Id")]
[Index(nameof(FileId))]
public class FileChunk
{
    public int Id { get; set; }
    public required int FileId { get; set; }
    
    public required int ChunkNumber { get; set; }
    
    public required byte[] Data { get; set; }
}
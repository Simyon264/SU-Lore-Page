using System.Text;

namespace SU_Lore.Middleware;

/// <summary>
/// Reads the contents of the body and sets the content-length header to the length of the body.
/// </summary>
public class ResponseBodyOverride
{
    private readonly RequestDelegate _next;
    public ResponseBodyOverride(RequestDelegate next)
    {
        _next = next;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        var stream = request.Body;         
        var originalReader = new StreamReader(stream);
        var originalContent = await originalReader.ReadToEndAsync();
        var newStream = new MemoryStream();
        var newWriter = new StreamWriter(newStream);
        newWriter.Write(originalContent);
        newWriter.Flush();
        newStream.Seek(0, SeekOrigin.Begin);
        context.Request.Body = newStream;
        context.Request.ContentLength = newStream.Length;
        
        await _next(context);
    }
}
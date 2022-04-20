namespace Animato.Sso.Application.Models;
public class StreamWithName
{
    public StreamWithName(Stream stream, string name)
    {
        Stream = stream;
        Name = name;
    }

    public Stream Stream { get; }

    public string Name { get; }
}

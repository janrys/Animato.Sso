namespace Animato.Sso.Application.Models;

using System;
using System.IO;

public class CreateAsset
{
    public string Name { get; set; }
    public Stream Stream { get; set; }
    public string ContentType { get; set; }
    public List<string> Transformations { get; set; } = new List<string>();
    public Guid? TransformationId { get; set; }
}

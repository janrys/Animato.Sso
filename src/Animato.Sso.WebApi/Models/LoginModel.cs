namespace Animato.Sso.WebApi.Models;

public class LoginModel
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string QrCode { get; set; }
    public string Action { get; set; }
    public string RedirectUrl { get; set; }
}

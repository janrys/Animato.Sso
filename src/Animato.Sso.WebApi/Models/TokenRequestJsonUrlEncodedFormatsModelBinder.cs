namespace Animato.Sso.WebApi.Models;

using Animato.Sso.Application.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

public class TokenRequestJsonUrlEncodedFormatsModelBinder : IModelBinder
{

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext is null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        try
        {
            string requestBody;
            bindingContext.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);

            using (var reader = new StreamReader(bindingContext.HttpContext.Request.Body))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            if (bindingContext.HttpContext.Request.ContentType.Contains("x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
            {

            }
            else
            {
                bindingContext.Model = JsonConvert.DeserializeObject<TokenRequest>(requestBody);
            }
        }
        catch (Exception exception)
        {
            bindingContext.ModelState.AddModelError(
                bindingContext.ModelName, $"Cannot convert value to {nameof(TokenRequest)}. Exception {exception.GetType().Name} {exception.Message}");
        }
    }

    //protected override Task BindProperty(ModelBindingContext bindingContext)
    //{
    //    try
    //    {
    //        var result = base.BindProperty(bindingContext);
    //        if (bindingContext.Result.IsModelSet == false)
    //        {
    //            var request = bindingContext.HttpContext.Request;
    //            var body = request.Body;

    //            var buffer = new byte[Convert.ToInt32(request.ContentLength, CultureInfo.InvariantCulture)];

    //            request.Body.Read(buffer, 0, buffer.Length);

    //            var bodyAsText = Encoding.UTF8.GetString(buffer);

    //            var jobject = JObject.Parse(bodyAsText);
    //            var value = jobject.GetValue(bindingContext.FieldName, StringComparison.InvariantCultureIgnoreCase);
    //            var typeConverter = TypeDescriptor.GetConverter(bindingContext.ModelType);
    //            var model = typeConverter.ConvertFrom(
    //                context: null,
    //                culture: CultureInfo.InvariantCulture,
    //                value: value.ToString());
    //            bindingContext.Result = ModelBindingResult.Success(model);
    //            request.Body.Seek(0, SeekOrigin.Begin);

    //        }
    //        return result;
    //    }
    //    catch (Exception)
    //    {
    //        throw;
    //    }
    //}
}

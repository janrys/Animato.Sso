namespace Animato.Sso.Infrastructure.Transformations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;

public abstract class BaseTransformation : IAssetTransformation
{
    protected const string PARAMETER_SPLIT = ",";
    protected const string PARAMETER_VALUE_SPLIT = "=";
    protected const string ASSET_TYPE_ANY = "*";

    protected BaseTransformation(string code, string description, params string[] assetTypes)
    {
        Code = code;
        Description = description;
        AssetTypes = assetTypes;
    }


    public string Code { get; private set; }

    public string Description { get; private set; }

    public IEnumerable<string> AssetTypes { get; private set; }

    public virtual bool CanTransform(string assetType)
    {
        if (AssetTypes.Any(t => t.Equals(ASSET_TYPE_ANY, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return AssetTypes.Any(t => t.Equals(assetType, StringComparison.OrdinalIgnoreCase));
    }
    public abstract Task<Stream> Transform(Stream asset, string parameters = null);

    protected virtual List<KeyValuePair<string, string>> ParseParameters(string parameters)
    {
        var parsedParameters = new List<KeyValuePair<string, string>>();

        if (string.IsNullOrEmpty(parameters))
        {
            return parsedParameters;
        }

        var splitParams = parameters.Split(PARAMETER_SPLIT);

        foreach (var parameterWithValue in splitParams)
        {
            var splitParamValue = parameterWithValue.Split(PARAMETER_VALUE_SPLIT, 2);
            var value = splitParamValue.Length == 2 ? splitParamValue[1].Trim() : null;
            var paramValue = new KeyValuePair<string, string>(splitParamValue[0].Trim(), value);
            parsedParameters.Add(paramValue);
        }

        return parsedParameters;
    }
}

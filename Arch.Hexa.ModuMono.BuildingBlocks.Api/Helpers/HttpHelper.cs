using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace Arch.Hexa.ModuMono.BuildingBlocks.Api.Helpers;

public static class HttpHelper
{
    private const string Unknown = "Unknown";

    #region Headers

    public static IHeaderDictionary? GetHttpHeaders(IHttpContextAccessor httpContextAccessor)
    {
        return httpContextAccessor.HttpContext?.Request.Headers;
    }

    #endregion

    #region User Agent

    public static string GetUserFromHttpContext(IHttpContextAccessor httpContextAccessor)
    {
        return GetUserFromHeaders(httpContextAccessor.HttpContext?.Request.Headers);
    }

    private static string GetUserFromHeaders(IHeaderDictionary? headers)
    {
        var userAgentString = headers?.UserAgent.ToString();

        if (string.IsNullOrWhiteSpace(userAgentString))
        {
            userAgentString = Unknown;
        }

        return userAgentString;
    }

    #endregion

    #region Culture Info

    public static string GetCultureInfoFromHttpContext(IHttpContextAccessor httpContextAccessor)
    {
        return GetCultureInfoFromHeaders(httpContextAccessor.HttpContext?.Request.Headers);
    }

    private static string GetCultureInfoFromHeaders(IHeaderDictionary? headers)
    {
        var cultureInfoString = headers?.AcceptLanguage.ToString();

        if (string.IsNullOrWhiteSpace(cultureInfoString))
            cultureInfoString = "en-US";

        if (!CultureExist(cultureInfoString))
            cultureInfoString = "en-US";

        return cultureInfoString;
    }

    public static bool CultureExist(string cultureName)
    {
        return CultureInfo.GetCultures(CultureTypes.AllCultures).Any(culture => string.Equals(culture.Name, cultureName, StringComparison.CurrentCultureIgnoreCase));
    }

    #endregion
}
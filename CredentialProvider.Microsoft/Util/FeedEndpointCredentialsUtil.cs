﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ILogger = NuGetCredentialProvider.Logging.ILogger;

namespace NuGetCredentialProvider.Util;

public static class FeedEndpointCredentialsUtil
{
    public static Dictionary<string, EndpointCredentials> ParseFeedEndpointsJsonToDictionary(ILogger logger)
    {
        string feedEndPointsJson = Environment.GetEnvironmentVariable(EnvUtil.EndpointCredentials);
        if (string.IsNullOrWhiteSpace(feedEndPointsJson))
        {
            logger.Warning(Resources.InvalidJsonWarning);
            return null;
        }

        try
        {
            logger.Verbose(Resources.ParsingJson);
            if (!feedEndPointsJson.Contains("':"))
            {
                logger.Warning(Resources.InvalidJsonWarning);
            }
            Dictionary<string, EndpointCredentials> credsResult = new Dictionary<string, EndpointCredentials>(StringComparer.OrdinalIgnoreCase);
            EndpointCredentialsContainer endpointCredentials = JsonConvert.DeserializeObject<EndpointCredentialsContainer>(feedEndPointsJson);
            if (endpointCredentials == null)
            {
                logger.Verbose(Resources.NoEndpointsFound);
                return credsResult;
            }

            foreach (var credentials in endpointCredentials.EndpointCredentials)
            {
                if (credentials == null)
                {
                    logger.Verbose(Resources.EndpointParseFailure);
                    break;
                }

                if (credentials.ClientId == null)
                {
                    logger.Verbose(Resources.EndpointParseFailure);
                    break;
                }

                if (!Uri.TryCreate(credentials.Endpoint, UriKind.Absolute, out var endpointUri))
                {
                    logger.Verbose(Resources.EndpointParseFailure);
                    break;
                }

                var urlEncodedEndpoint = endpointUri.AbsoluteUri;
                if (!credsResult.ContainsKey(urlEncodedEndpoint))
                {
                    credsResult.Add(urlEncodedEndpoint, credentials);
                }
            }

            return credsResult;
        }
        catch (Exception e)
        {
            logger.Verbose(string.Format(Resources.VstsBuildTaskExternalCredentialCredentialProviderError, e));
            throw;
        }
    }


    public static Dictionary<string, ExternalEndpointCredentials> ParseExternalFeedEndpointsJsonToDictionary(ILogger logger)
    {
        string feedEndPointsJson = Environment.GetEnvironmentVariable(EnvUtil.BuildTaskExternalEndpoints);
        if (string.IsNullOrWhiteSpace(feedEndPointsJson))
        {
            logger.Warning(Resources.InvalidJsonWarning);
            return null;
        }

        try
        {
            logger.Verbose(Resources.ParsingJson);
            if (!feedEndPointsJson.Contains("':"))
            {
                logger.Warning(Resources.InvalidJsonWarning);
            }
            Dictionary<string, ExternalEndpointCredentials> credsResult = new Dictionary<string, ExternalEndpointCredentials>(StringComparer.OrdinalIgnoreCase);
            ExternalEndpointCredentialsContainer endpointCredentials = JsonConvert.DeserializeObject<ExternalEndpointCredentialsContainer>(feedEndPointsJson);
            if (endpointCredentials == null)
            {
                logger.Verbose(Resources.NoEndpointsFound);
                return credsResult;
            }

            foreach (var credentials in endpointCredentials.EndpointCredentials)
            {
                if (credentials == null)
                {
                    logger.Verbose(Resources.EndpointParseFailure);
                    break;
                }

                if (credentials.Username == null)
                {
                    credentials.Username = "VssSessionToken";
                }

                if (!Uri.TryCreate(credentials.Endpoint, UriKind.Absolute, out var endpointUri))
                {
                    logger.Verbose(Resources.EndpointParseFailure);
                    break;
                }

                var urlEncodedEndpoint = endpointUri.AbsoluteUri;
                if (!credsResult.ContainsKey(urlEncodedEndpoint))
                {
                    credsResult.Add(urlEncodedEndpoint, credentials);
                }
            }

            return credsResult;
        }
        catch (Exception e)
        {
            logger.Verbose(string.Format(Resources.VstsBuildTaskExternalCredentialCredentialProviderError, e));
            throw;
        }
    }
}


public class ExternalEndpointCredentials
{
    [JsonProperty("endpoint")]
    public string Endpoint { get; set; }
    [JsonProperty("username")]
    public string Username { get; set; }
    [JsonProperty("password")]
    public string Password { get; set; }
}

public class ExternalEndpointCredentialsContainer
{
    [JsonProperty("endpointCredentials")]
    public ExternalEndpointCredentials[] EndpointCredentials { get; set; }
}


public class EndpointCredentials
{
    [JsonProperty("endpoint")]
    public string Endpoint { get; set; }
    [JsonProperty("clientId")]
    public string ClientId { get; set; }
}

public class EndpointCredentialsContainer
{
    [JsonProperty("endpointCredentials")]
    public EndpointCredentials[] EndpointCredentials { get; set; }
}


using FlatTrade.Common.Transport;
using FlatTrade.Common.Types.Base;
using Newtonsoft.Json;
using System.Reflection;
using System.Text;

namespace FlatTrade.Common.Helpers
{
    public static class RestHttpClientExtension
    {
        //public static async Task<(TReceive?, string)> PostMessageAsync<TReceive>(this RestHttpClient restHttpClient, string uri, string msg)
        //{
        //    HttpContent content = new StringContent(msg, Encoding.UTF8);
        //    var (response, eMsg) = await restHttpClient.PostAsync<TReceive>(new Uri(uri), content);
        //    if (response is BaseErrorMessageResponse error && !string.IsNullOrEmpty(error.ErrorMsg))
        //    {
        //        eMsg = $"{error.ErrorMsg} - {eMsg}";
        //        response = default;
        //    }
        //    return (response, eMsg);
        //}

        public static void CheckJsonAgainstModel<T>(string json, bool isSchemaModelCheckByPassed)
        {
            if (isSchemaModelCheckByPassed)
                return;

            var jsonTokens = JsonConvert.DeserializeObject<object>(json);

            var allJsonFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (jsonTokens is Newtonsoft.Json.Linq.JObject obj)
            {
                foreach (var prop in obj.Properties())
                    allJsonFields.Add(prop.Name);
            }
            else if (jsonTokens is Newtonsoft.Json.Linq.JArray arr)
            {
                foreach (var item in arr.OfType<Newtonsoft.Json.Linq.JObject>())
                {
                    foreach (var prop in item.Properties())
                        allJsonFields.Add(prop.Name);
                }
            }

            // Collect model property names (considering JsonProperty attributes)
            var modelProps = GetModelName<T>().GetProperties()
                .Select(p =>
                {
                    var attr = p.GetCustomAttribute<JsonPropertyAttribute>();
                    return attr?.PropertyName ?? p.Name;
                })
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var extra = allJsonFields.Except(modelProps).ToList();
            var missing = modelProps.Except(allJsonFields).ToList();


            if (extra.Count != 0)
                Console.Error.WriteLine("Error: Extra JSON fields: " + GetModelName<T>().Name + ": " + string.Join(", ", extra));

            //if(missing.Any())
            //    Console.WriteLine("Missing JSON fields: " + GetModelName<T>().Name + ": " + string.Join(", ", missing));
        }

        static Type GetModelName<T>()
        {
            var t = typeof(T);

            // If it's an array → return element type
            if (t.IsArray) return t.GetElementType()!;

            // If it's a generic collection (IEnumerable<T>, List<T>, etc.)
            if (t.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(t))
                return t.GetGenericArguments()[0];

            // Otherwise, just return the type name
            return t;
        }

        public static async Task<(TReceive?, string)> PostMessageAsync<TReceive>(this RestHttpClient restHttpClient, string uri, string msg, bool isSchemaModelCheckByPassed = false)
        {
            var (responseString, status) = await PostMessageHelperAsync(restHttpClient, uri, msg); // Pass 'restHttpClient' explicitly to fix CS7036
            if (string.Compare(status, Constants.StatusOk, StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                return (default, $"{status} {responseString}");
            }

            string eMsg;
            try
            {
                CheckJsonAgainstModel<TReceive>(responseString, isSchemaModelCheckByPassed);
                return (JsonConvert.DeserializeObject<TReceive>(responseString), Constants.StatusOk);
            }
            catch (JsonReaderException e)
            {
                eMsg = $"[API Client Error]: POST JSON Reader Exception for {uri}: {e.Message} - Raw Response: '{responseString}'";
            }
            catch (JsonSerializationException e)
            {
                eMsg = $"[API Client Error]: POST JSON Serialization Exception for {uri}: {e.Message} - Raw Response: '{responseString}'";
            }
            catch (ArgumentOutOfRangeException e)
            {
                eMsg = $"[API Client Error]: POST JSON ArgumentOutOfRange Exception for {uri}: {e.Message} - Raw Response: '{responseString}'";
            }
            catch (Exception e)
            {
                eMsg = $"[API Client Error]: Unexpected error during POST for {uri}: {e.Message} - Raw Response: '{responseString}'";
            }
            return (default, eMsg);
        }

        public static async Task<(IEnumerable<TReceive>?, string)> PostMessageForDoubleSerializedAsync<TReceive>(this RestHttpClient restHttpClient, string uri, string msg)
        {
            HttpContent content = new StringContent(msg, Encoding.UTF8);
            string errorMsg = string.Empty;
            try
            {
                var (responseString, eMsg) = await restHttpClient.PostAsync(new Uri(uri), content);
                if (string.Compare(eMsg, Constants.StatusOk, StringComparison.InvariantCultureIgnoreCase) != 0 &&
                   string.IsNullOrEmpty(responseString))
                {
                    return (default, eMsg);
                }

                var innerJsonStrings = JsonConvert.DeserializeObject<IEnumerable<string>>(responseString);
                if (innerJsonStrings is null)
                {
                    eMsg = $"Failed to deserialize response into a list of strings. {eMsg}";
                    return (default, eMsg);
                }

                var deserializedObjects = innerJsonStrings.Select(jsonString => JsonConvert.DeserializeObject<TReceive>(jsonString)).ToList();
                if (deserializedObjects.Any(obj => obj is null))
                {
                    eMsg = $"One or more items in the response failed to deserialize. {eMsg}";
                    return (default, eMsg);
                }

                return ((IEnumerable<TReceive>)(object)deserializedObjects, Constants.StatusOk); // Cast to object and then to TReceive to resolve CS9174 and CS9176
            }
            catch (JsonSerializationException e)
            {
                errorMsg = $"[API Client Error]: POST JSON Serialization Exception for {uri}: {e.Message}";
            }
            catch (ArgumentOutOfRangeException e)
            {
                errorMsg = $"[API Client Error]: POST JSON Serialization Exception for {uri}: {e.Message}";
            }
            catch (Exception e)
            {
                errorMsg = $"[API Client Error]: Unexpected error during POST for {uri}: {e.Message}";
            }

            return (default, errorMsg);
        }

        private static string? GetErrorResponseIfAny(string responseString)
        {
            if (string.IsNullOrEmpty(responseString))
            {
                return default; // No response to process
            }
            string eMsg = string.Empty;
            try
            {
                BaseErrorMessageResponse? errorResponse;
                if (responseString.StartsWith('['))
                {
                    var anyErrorResponse = JsonConvert.DeserializeObject<IEnumerable<BaseErrorMessageResponse>>(responseString);
                    if (anyErrorResponse is null)
                    {
                        return default; // No error response to process
                    }

                    errorResponse = anyErrorResponse?.FirstOrDefault(resp => !string.IsNullOrEmpty(resp.ErrorMsg));
                }
                else
                {
                    errorResponse = JsonConvert.DeserializeObject<BaseErrorMessageResponse>(responseString);
                }

                if (errorResponse is not null && !string.IsNullOrEmpty(errorResponse.ErrorMsg))
                {
                    eMsg = $"{errorResponse.ErrorMsg}";
                    return eMsg;
                }
                return default;
            }
            catch (JsonSerializationException e)
            {
                eMsg = $"[API Client Error]: POST JSON Serialization Exception : {e.Message}";
                return eMsg;
            }
            catch (ArgumentOutOfRangeException e)
            {
                return $"[API Client Error]: GET JSON ArgumentOutOfRange Exception : {e.Message}";
            }
            catch (Exception e)
            {
                return $"[API Client Error]: Unexpected error during GetErrorResponseIfAny : {e.Message}";
            }
        }

        private static async Task<(string, string)> PostMessageHelperAsync(RestHttpClient restHttpClient, string uri, string msg)
        {
            HttpContent content = new StringContent(msg, Encoding.UTF8);
            string? errorMsg;
            try
            {
                var (responseString, eMsg) = await restHttpClient.PostAsync(new Uri(uri), content);
                if (string.Compare(eMsg, Constants.StatusOk, StringComparison.InvariantCultureIgnoreCase) != 0 &&
                   string.IsNullOrEmpty(responseString))
                {
                    return (string.Empty, eMsg);
                }

                errorMsg = GetErrorResponseIfAny(responseString);
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    return (errorMsg, Constants.StatusNotOk);
                }

                return (responseString, Constants.StatusOk);
            }
            catch (JsonSerializationException e)
            {
                errorMsg = $"[API Client Error]: POST JSON Serialization Exception for {uri}: {e.Message}";
            }
            catch (ArgumentOutOfRangeException e)
            {
                errorMsg = $"[API Client Error]: POST JSON Serialization Exception for {uri}: {e.Message}";
            }
            catch (Exception e)
            {
                errorMsg = $"[API Client Error]: Unexpected error during POST for {uri}: {e.Message}";
            }

            return (string.Empty, errorMsg);
        }

    }
}

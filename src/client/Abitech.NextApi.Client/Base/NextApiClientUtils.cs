using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Abitech.NextApi.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Abitech.NextApi.Client.Base
{
    /// <summary>
    /// Utils for NextApiClient
    /// </summary>
    internal static class NextApiClientUtils
    {
        /// <summary>
        /// Parse next api file response
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static async Task<NextApiFileResponse> ProcessNextApiFileResponse(HttpResponseMessage response)
        {
            var content = response.Content;
            var fileName = WebUtility.UrlDecode(content.Headers.ContentDisposition.FileName);
            var mimeType = content.Headers.ContentType.MediaType;
            var stream = await content.ReadAsStreamAsync();
            return new NextApiFileResponse(fileName, stream, mimeType);
        }

        /// <summary>
        /// Get default JSON serializer settings
        /// </summary>
        /// <returns></returns>
        public static JsonSerializerSettings GetJsonConfig()
        {
            return new JsonSerializerSettings
            {
                Converters = new JsonConverter[] {new StringEnumConverter()}
            };
        }
        /// <summary>
        /// Prepare multipart form for sending via HTTP
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static MultipartFormDataContent PrepareRequestForm(NextApiCommand command)
        {
            var args = command.Args.Where(arg => arg is NextApiArgument).ToArray();
            var form = new MultipartFormDataContent
            {
                {new StringContent(command.Service), "Service"},
                {new StringContent(command.Method), "Method"},
                {new StringContent(JsonConvert.SerializeObject(args, GetJsonConfig())), "Args"}
            };
            // send files
            var fileArgs = command.Args.Where(arg => arg is NextApiFileArgument).Cast<NextApiFileArgument>().ToArray();
            foreach (var nextApiFileArgument in fileArgs)
            {
                var stream = nextApiFileArgument.FileDataStream ??
                             new FileStream(nextApiFileArgument.FilePath, FileMode.Open);
                var fileName = nextApiFileArgument.FileName ??
                               Path.GetFileName(nextApiFileArgument.FilePath) ?? "noname.bin";
                var name = nextApiFileArgument.FileId;
                form.Add(new StreamContent(stream), name, fileName);
            }

            return form;
        }
        

        /// <summary>
        /// Prepare NextApiCommand for sending
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="serviceMethod"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static NextApiCommand PrepareCommand(string serviceName, string serviceMethod, INextApiArgument[] arguments)
        {
            return new NextApiCommand {Args = arguments, Method = serviceMethod, Service = serviceName};
        }
        
        /// <summary>
        /// Parse NextApiException from NextApiError
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public static NextApiException NextApiException(NextApiError error)
        {
            var message = error.Parameters["message"];
            return new NextApiException(error.Code, $"{error.Code} {message}", error.Parameters);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Abitech.NextApi.Model
{
    /// <summary>
    /// Default response wrapper from NextApi
    /// </summary>
    public class NextApiResponse
    {
        /// <summary>
        /// Indicates that request is successful
        /// </summary>
        public bool Success { get; } = true;

        /// <summary>
        /// Contains data of response
        /// </summary>
        public object Data { get; }

        /// <summary>
        /// Contains error if Success is false
        /// </summary>
        public NextApiError Error { get; }

        /// <inheritdoc />
        public NextApiResponse(object data, NextApiError error = null, bool success = true)
        {
            Data = data;
            Error = error;
            Success = success;
        }
    }

    /// <summary>
    /// Default response wrapper for file response
    /// </summary>
    public class NextApiFileResponse : IDisposable
    {
        /// <summary>
        /// Stream with file content
        /// </summary>
        public Stream FileStream { get; }

        /// <summary>
        /// Name of file
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Mime type for file
        /// </summary>
        public string MimeType { get; }

        /// <summary>
        /// Initialize instance of NextApiFileResponse
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <param name="fileStream">Stream with file data</param>
        /// <param name="mimeType">Mime type for file</param>
        public NextApiFileResponse(string fileName, Stream fileStream, string mimeType = "application/octet-stream")
        {
            FileStream = fileStream ?? throw new ArgumentNullException(nameof(fileStream));
            MimeType = mimeType;
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        }

        /// <summary>
        /// Saves file to specific folder
        /// </summary>
        /// <param name="folderPath">Path to folder</param>
        /// <returns></returns>
        public async Task SaveToFolder(string folderPath)
        {
            var filePath = Path.Combine(folderPath, FileName);
            await SaveAsFile(filePath);
        }

        /// <summary>
        /// Saves file to specific path
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task SaveAsFile(string filePath)
        {
            var fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
            using (fileStream)
            {
                await CopyToAsync(fileStream);
            }
        }

        /// <summary>
        /// Copy content from original content stream to another
        /// </summary>
        /// <param name="stream">Another stream</param>
        /// <returns></returns>
        public async Task CopyToAsync(Stream stream)
        {
            await FileStream.CopyToAsync(stream);
        }

        /// <summary>
        /// Get all bytes from content stream
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> GetBytes()
        {
            byte[] bytes;
            using (var memStream = new MemoryStream())
            {
                await CopyToAsync(memStream);
                bytes = memStream.ToArray();
            }

            return bytes;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            FileStream?.Dispose();
        }
    }

    /// <summary>
    /// Default error wrapper from NextApi
    /// </summary>
    public class NextApiError
    {
        /// <summary>
        /// Error code. For example: ServiceNotFound
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Additional parameters for current error
        /// </summary>
        public Dictionary<string, object> Parameters { get; private set; }

        /// <inheritdoc />
        public NextApiError(string code, Dictionary<string, object> parameters)
        {
            Code = code;
            Parameters = parameters;
        }
    }

    /// <summary>
    /// System error code for nextapi
    /// </summary>
    public enum NextApiErrorCode
    {
#pragma warning disable 1591
        // base services
        ServiceIsNotFound,
        OperationIsNotFound,
        ServiceIsOnlyForAuthorized,
        OperationIsNotAllowed,
        Unknown,
        IncorrectRequest,
        HttpError,
        SignalRError,
        EntityIsNotExist,
        EntitiesIsNotExist,
#pragma warning restore 1591
    }
}

using System.IO;

namespace Abitech.NextApi.Model
{
    /// <summary>
    /// Basic interface for NextApi service method argument
    /// </summary>
    [MessagePack.Union(1, typeof(NextApiArgument))]
    [MessagePack.Union(2, typeof(NextApiFileArgument))]
    public interface INextApiArgument
    {
    }

    /// <summary>
    /// Basic interface for NextApi service named method arguments
    /// </summary>
    public interface INamedNextApiArgument : INextApiArgument
    {
        /// <summary>
        /// Argument name
        /// </summary>
        string Name { get; set; }
    }

    /// <summary>
    /// NextApi service method argument
    /// </summary>
    public class NextApiArgument : INamedNextApiArgument
    {
        /// <summary>
        /// Argument value
        /// </summary>
        public object Value { get; set; }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <summary>
        /// NextApi service method argument
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        public NextApiArgument(string name, object value)
        {
            Name = name;
            Value = value;
        }

        /// <inheritdoc />
        public NextApiArgument()
        {
        }
    }

    /// <summary>
    /// NextApi service method file argument
    /// </summary>
    public class NextApiFileArgument : INextApiArgument
    {
        /// <summary>
        /// File identifier (used to match this file when request is processing)
        /// </summary>
        public string FileId { get; private set; }

        /// <summary>
        /// Path to file
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Alternate to FilePath, provide stream to File data
        /// </summary>
        public Stream FileDataStream { get; private set; }

        /// <summary>
        /// File name with extension
        /// </summary>
        public string FileName { get; private set; }


        /// <inheritdoc />
        public NextApiFileArgument(string fileId, string filePath)
        {
            FileId = fileId;
            FilePath = filePath;
        }

        /// <inheritdoc />
        public NextApiFileArgument(string fileId, string fileName, Stream fileDataStream)
        {
            FileId = fileId;
            FileDataStream = fileDataStream;
            FileName = fileName;
        }
    }
}

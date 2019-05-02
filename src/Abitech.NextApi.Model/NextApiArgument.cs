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
    /// NextApi service method argument
    /// </summary>
    public class NextApiArgument : INextApiArgument
    {
        /// <summary>
        /// Argument name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Argument value
        /// </summary>
        public object Value { get; set; }

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
        /// Path to file
        /// </summary>
        public string FilePath { get; private set; }
        /// <summary>
        /// Alternate to FilePath, provide stream to File data
        /// </summary>
        public Stream FileDataStream { get; private set; }
        public string FileName { get; private set; }
        

        /// <inheritdoc />
        public NextApiFileArgument(string filePath)
        {
            FilePath = filePath;
        }

        /// <inheritdoc />
        public NextApiFileArgument(Stream fileDataStream, string fileName)
        {
            FileDataStream = fileDataStream;
            FileName = fileName;
        }
    }
}

using NextApi.Common;

namespace NextApi.Client.Base
{
    /// <summary>
    /// Used for JSON parsing of NextApiResponse 
    /// </summary>
    /// <typeparam name="TDataType"></typeparam>
    internal class NextApiResponseJsonWrapper<TDataType>
    {
        public bool Success { get; set; }
        public TDataType Data { get; set; }
        public NextApiError Error { get; set; }
    }
}

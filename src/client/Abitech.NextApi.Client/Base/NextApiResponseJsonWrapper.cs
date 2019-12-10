using Abitech.NextApi.Model;

namespace Abitech.NextApi.Client.Base
{
    internal class NextApiResponseJsonWrapper<TDataType>
    {
        public bool Success { get; set; }
        public TDataType Data { get; set; }
        public NextApiError Error { get; set; }
    }
}

using System.Net;

namespace Benefit.CardReader.DataTransfer.Dto.Base
{
    public class ResponseResult<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public T Data { get; set; }
    }
}

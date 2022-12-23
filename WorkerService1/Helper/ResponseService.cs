using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WorkerService1.Helper
{
    public class ResponseService
    {

        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public Exception? Exception { get; set; }
        public dynamic Data { get; set; }
        public bool Success { get; set; }

        public ResponseService()
        {
            StatusCode = HttpStatusCode.OK;
            Data = new ExpandoObject();
        }
        public ResponseService(HttpStatusCode code, dynamic data, string message)
        {
            StatusCode = code;
            Message = message;
            Data = data;
        }

        public ResponseService(HttpStatusCode code, string message)
        {
            StatusCode = code;
            Message = message;
        }
        public ResponseService(HttpStatusCode code, dynamic data)
        {
            StatusCode = code;
            Data = data;
        }

        public ResponseService(Exception exception)
        {
            StatusCode = HttpStatusCode.InternalServerError;
            Exception = exception;
        }

        public ResponseService(dynamic data, string message, bool success)
        {
            Data = data;
            Message = message;
            Success = success;
        }


    }
}

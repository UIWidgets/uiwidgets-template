using System;

namespace UIWidgetsApp.Common.HttpUtil
{
    [Serializable]
    public class HttpErrorData
    {
        public string url;
        public string error;
        public ErrorBody body;
    }
    
    [Serializable]
    public class ErrorBody
    {
        public string errorCode;
        public string message;
        public string target;
    }
}
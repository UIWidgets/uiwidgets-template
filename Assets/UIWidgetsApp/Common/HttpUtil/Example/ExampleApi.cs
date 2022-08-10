using System;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace UIWidgetsApp.Common.HttpUtil.Example
{
    public class ExampleApi
    {
        // {
        //     "page": {
        //         "id": "kctbh9vrtdwd",
        //         "name": "GitHub",
        //         "url": "https://www.githubstatus.com",
        //         "time_zone": "Etc/UTC",
        //         "updated_at": "2022-08-10T01:31:09.929Z"
        //     },
        //     "status": {
        //         "indicator": "none",
        //         "description": "All Systems Operational"
        //     }
        // }
        
        [Serializable]
        public class Page
        {
            public string id;
            public string name;
            public string url;
            public string time_zone;
            public string updated_at;
        }
        
        [Serializable]
        public class Status
        {
            public string indicator;
            public string description;
        }
        
        [Serializable]
        public class StatusResponse
        {
            public Page page;
            public Status status;
        }
        
        public static Future<StatusResponse> GetGitHubStatus()
        {
            const string url = "https://www.githubstatus.com/api/v2/status.json";
            var request = HttpManager.GET(url: url);
            return HttpManager.resume(request: request).then_<StatusResponse>(responseText =>
            {
                if (responseText == null) throw new UIWidgetsError($"Unable to load: {url}");

                var initDataResponse = JsonUtility.FromJson<StatusResponse>(responseText);
                return FutureOr.value(value: initDataResponse);
            });
        }
    }
}
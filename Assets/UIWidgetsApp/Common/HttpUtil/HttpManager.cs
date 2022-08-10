using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.UIWidgets.async;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using UnityEngine.Networking;

namespace UIWidgetsApp.Common.HttpUtil
{
    public static class Method
    {
        public const string GET = "GET";
        public const string POST = "POST";
    }

    public static class HttpManager
    {
        private const string COOKIE = "Cookie";

        private static UnityWebRequest initRequest(
            string url,
            string method
        )
        {
            var request = new UnityWebRequest
            {
                url = url,
                method = method,
                downloadHandler = new DownloadHandlerBuffer()
            };
            UnityWebRequest.ClearCookieCache();
            // Set Request Header
            request.SetRequestHeader(name: COOKIE, _cookieHeader());

            return request;
        }

        public static UnityWebRequest GET(string url)
        {
            return initRequest(url: url, method: Method.GET);
        }

        public static UnityWebRequest POST(string url, Dictionary<string, object> parameter = null)
        {
            var request = initRequest(url: url, method: Method.POST);
            if (parameter != null)
            {
                var body = JsonUtility.ToJson(parameter);
                var bodyRaw = Encoding.UTF8.GetBytes(s: body);
                request.uploadHandler = new UploadHandlerRaw(data: bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
            }

            return request;
        }

        public static Future<string> resume(UnityWebRequest request)
        {
            var completer = Completer.create();
            var isolate = Isolate.current;
            var panel = UIWidgetsPanelWrapper.current.window;
            if (panel.isActive())
                panel.startCoroutine(sendRequest(request: request, completer: completer, isolate: isolate));

            return completer.future.to<string>();
        }

        private static IEnumerator sendRequest(UnityWebRequest request, Completer completer, Isolate isolate)
        {
            yield return request.SendWebRequest();

            using (Isolate.getScope(isolate: isolate))
            {
                if (request.responseCode == 401)
                {
                    // TODO: need logout
                    completer.completeError(
                        new Exception($"Failed to load from url \"{request.url}\": {request.error} BODY:{request.downloadHandler.text}"));
                    yield break;
                }

                if (isNetWorkError() || request.isNetworkError || request.isHttpError || request.responseCode != 200)
                {
                    try
                    {
                        var errorBody = JsonUtility.FromJson<ErrorBody>(request.downloadHandler.text);
                        var errorData = new HttpErrorData
                        {
                            url = request.url,
                            error = request.error,
                            body = errorBody
                        };
                        var dataString = JsonUtility.ToJson(errorData);
                        completer.completeError(new Exception(dataString));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        completer.completeError(
                            new Exception($"Failed to load from url \"{request.url}\": {request.error} BODY:{request.downloadHandler.text}"));
                        throw;
                    }
                    yield break;
                }

                if (request.GetResponseHeaders().ContainsKey("Set-Cookie"))
                {
                    var cookie = request.GetResponseHeaders()["Set-Cookie"];
                    updateCookie(newCookie: cookie);
                }

                var data = request.downloadHandler.text;

                completer.complete(value: data);
            }
        }

        private static string _cookieHeader()
        {
            return PlayerPrefs.GetString(key: COOKIE).isNotEmpty() ? PlayerPrefs.GetString(key: COOKIE) : "";
        }

        private static string getCookie()
        {
            return _cookieHeader();
        }

        public static string getCookie(string key)
        {
            var cookie = getCookie();
            if (cookie.isEmpty()) return "";

            var cookieArr = cookie.Split(';');
            foreach (var c in cookieArr)
            {
                var carr = c.Split('=');

                if (carr.Length != 2) continue;

                var name = carr[0].Trim();
                var value = carr[1].Trim();
                if (name == key) return value;
            }

            return "";
        }

        public static void updateCookie(string newCookie)
        {
            var cookie = PlayerPrefs.GetString(key: COOKIE);
            var cookieDict = new Dictionary<string, string>();
            var updateCookie = "";
            if (cookie.isNotEmpty())
            {
                var cookieArr = cookie.Split(';');
                foreach (var c in cookieArr)
                {
                    var name = c.Split('=').first();
                    cookieDict.Add(key: name, value: c);
                }
            }

            if (newCookie.isNotEmpty())
            {
                var newCookieArr = newCookie.Split(',');
                foreach (var c in newCookieArr)
                {
                    var item = c.Split(';').first();
                    var name = item.Split('=').first();
                    if (cookieDict.ContainsKey(key: name))
                        cookieDict[key: name] = item;
                    else
                        cookieDict.Add(key: name, value: item);
                }

                var updateCookieArr = cookieDict.Values;
                updateCookie = string.Join(";", values: updateCookieArr);
            }

            if (updateCookie.isEmpty()) return;

            PlayerPrefs.SetString(key: COOKIE, value: updateCookie);
            PlayerPrefs.Save();
        }
        
        public static void clearCookie()
        {
            PlayerPrefs.SetString(key: COOKIE, "");
            PlayerPrefs.Save();
        }
        
        public static bool isNetWorkError()
        {
            return Application.internetReachability == NetworkReachability.NotReachable;
        }

        public static HttpErrorData GetHttpErrorData(string errorString)
        {
            return errorString.isNotEmpty() ? JsonUtility.FromJson<HttpErrorData>(errorString) : null;
        }
    }
}
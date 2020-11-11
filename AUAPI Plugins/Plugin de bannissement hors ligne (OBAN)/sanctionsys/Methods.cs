using System;
using System.IO;
using System.Net;
using System.Text;

namespace SanctionSystem
{

    class Methods
    {

        public static String Post(String URL, String JSON)
        {
            HttpWebRequest request = (HttpWebRequest)
            WebRequest.Create(URL); request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";

            byte[] postBytes = Encoding.UTF8.GetBytes(JSON);

            request.ContentType = "application/json; charset=UTF-8";
            request.Accept = "application/json";
            request.Headers["Authorization"] = Plugin.TokenAPI;
            request.Headers["user"] = Plugin.UserAPI;
            request.Headers["userToken"] = Plugin.UserTokenAPI;
            request.ContentLength = postBytes.Length;
            Stream requestStream = request.GetRequestStream();

            requestStream.Write(postBytes, 0, postBytes.Length);
            requestStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string result;
            using (StreamReader rdr = new StreamReader(response.GetResponseStream()))
            {
                result = rdr.ReadToEnd();
            }

            return result;
        }

    }
}


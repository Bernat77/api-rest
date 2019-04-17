using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{


    class Program
    {
        //Credenciales de azure (aplicación, admin)
        private const string clientId = "e0cb57ca-ddc7-4dd0-8481-ae827d70843f";
        private const string aadInstance = "https://login.microsoftonline.com/{0}";
        private const string tenant = "Bernats.onmicrosoft.com";
        private const string resource = "https://graph.microsoft.com";
        private const string appKey = "haF?sO]Q(wb8mX3hMC?Z58NbM9m4e>H)REh$[cd)4Noi?k[]$wlba-Fp-^]";
        private const string username = "Bernat@bernats.onmicrosoft.com";
        private const string userpass = "Glad2link";
        private const string excelID = "01F5NWKYNBEKJT3GHQOVAZFTWZ2B5MMXUZ";
        static string authority = String.Format(aadInstance, tenant);

        // Http client y objetos del paquete active directory
        private static HttpClient httpclient = new HttpClient();
        private static AuthenticationContext authcontext = null;
        private static ClientCredential credential = null;


        static void Main(string[] args)
        {
            authcontext = new AuthenticationContext(authority);
            credential = new ClientCredential(clientId, appKey);



             Task<string> token = GetTokenUser();
             token.Wait();
             Console.WriteLine(token.Result + "\n");


             Task<string> request = GetRequest(token.Result);
             request.Wait();
             Console.WriteLine(request.Result + "\n");


            Task<string> session = GetWorkbookSession(excelID,token.Result);
            session.Wait();
            Console.WriteLine(session.Result + "\n");

            Task<string> worksheet = ObtenerWorksheetsWorkbook(excelID, token.Result, session.Result);
            worksheet.Wait();
            Console.WriteLine(worksheet.Result + "\n");

            Task<string> tables = ComprobarFila(excelID, token.Result, session.Result);
            tables.Wait();
            Console.WriteLine(tables.Result + "\n");

            Console.ReadLine();

        }

        private static async Task<string> GetTokenClientAsync()
        {
            AuthenticationResult result = null;
            string token1 = null;
            result = await authcontext.AcquireTokenAsync(resource, credential);
            token1 = result.AccessToken;

            return token1;
        }

        private static async Task<string> GetUsers(string result)
        {
            string users = null;
            var uri = "https://graph.microsoft.com/v1.0/users";
            httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result);
            var getResult = await httpclient.GetAsync(uri);

            if (getResult.Content != null)
            {
                users = await getResult.Content.ReadAsStringAsync();
            }

            return users;
        }

        private static async Task<string> GetRequest(string result)
        {
            string info = null;
            var uri = "https://graph.microsoft.com/v1.0/me/drive/items/01F5NWKYNBEKJT3GHQOVAZFTWZ2B5MMXUZ/workbook";
            httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result);
            var getResult = await httpclient.GetAsync(uri);

            if (getResult.Content != null)
            {
                info = await getResult.Content.ReadAsStringAsync();
            }

            return info;
        }

        private static async Task<string> GetWorkbookSession(string id,string token)
        {
            using (HttpClient client = new HttpClient()) { 
                string sessionID = null;
                var uri = "https://graph.microsoft.com/v1.0/me/drive/items/" + id + "/workbook/createSession";
                httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                string body = "{\"persistChanges\":true}";

            var getResult = await httpclient.PostAsync(uri, new StringContent(body,Encoding.UTF8, "application/json"));

            if (getResult.Content != null)
            {
                    var jsonresult = Newtonsoft.Json.Linq.JObject.Parse(await getResult.Content.ReadAsStringAsync());
                    sessionID = (string)jsonresult["id"];
                }

            return sessionID;
        }

        }

        private static async Task<string> ObtenerWorksheetsWorkbook(string id, string token, string session)
        {
            using(HttpClient client = new HttpClient())
            {
                var uri = "https://graph.microsoft.com/v1.0/me/drive/items/" + id + "/workbook/worksheets";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Add("workbook-session-id", session);
                httpclient.DefaultRequestHeaders.Add("Accept", "application/json");

                //var response = await client.PostAsync(uri, new StringContent("", Encoding.UTF8));
                var response = await client.GetAsync(uri);
                string content = null;

                if(response.Content != null)
                {
                    content = await response.Content.ReadAsStringAsync();
                }

                return content;
            }

        }

        private static async Task<string> ObtenerTablasWorkbook(string id, string token, string session)
        {
            using (HttpClient client = new HttpClient())
            {
                var uri = "https://graph.microsoft.com/v1.0/me/drive/items/" + id + "/workbook/worksheets" +
                    "(%27%7B00000000-0001-0000-0000-000000000000%7D%27)/tables";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Add("workbook-session-id", session);
                httpclient.DefaultRequestHeaders.Add("Accept", "application/json");

                //var response = await client.PostAsync(uri, new StringContent("", Encoding.UTF8));
                var response = await client.GetAsync(uri);
                string content = null;

                if (response.Content != null)
                {
                    content = await response.Content.ReadAsStringAsync();
                }

                return content;
            }

        }

        private static async Task<string> InsertarFila(string idfile, string token, string session)
        {
            using (HttpClient client = new HttpClient())
            {
                var uri = "https://graph.microsoft.com/v1.0/me/drive/items/" + idfile + "/workbook/tables('5')/rows";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Add("workbook-session-id", session);
                httpclient.DefaultRequestHeaders.Add("Accept", "application/json");
                string body = "{ \"values\": [ [ \"aha\", \"aha\", \"ahaaaa\",\"aha\", \"aha\", \"ahaaaa\"" +
                    ",\"aha\", \"aha\" ] ], \"index\": null }";

                var response = await client.PostAsync(uri, new StringContent(body, Encoding.UTF8,"application/json"));
                //var response = await client.GetAsync(uri);
                string content = null;

                if (response.Content != null)
                {
                    content = await response.Content.ReadAsStringAsync();
                }

                return content;
            }

        }

        private static async Task<string> ComprobarFila(string idfile, string token, string session)
        {
            using (HttpClient client = new HttpClient())
            {
                var uri = "https://graph.microsoft.com/v1.0/me/drive/items/" + idfile + "/workbook/tables('5')/columns(id='1')filter/apply";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Add("workbook-session-id", session);
                httpclient.DefaultRequestHeaders.Add("Accept", "application/json");
                string body = "{\"criteria\":{\"filterOn\": \"custom\",\"criterion1\":\"=1234\"}}";

                var response = await client.PostAsync(uri, new StringContent(body, Encoding.UTF8, "application/json"));
                //var response = await client.GetAsync(uri);
                string content = null;

                if (response.Content != null)
                {
                    content = await response.Content.ReadAsStringAsync();
                }

                return content;
            }

        }

        private static async Task<string> GetTokenUser()
        {
            string token = null;
            var tokenConnection = @"https://login.microsoftonline.com/common/oauth2/token";
            var content = "application/json";

            httpclient.DefaultRequestHeaders.Add("Accept", content);
            string postBody = "grant_type=password&resource=https://graph.microsoft.com" +
                "&client_id="+clientId+"&client_secret="+appKey+
                "&username="+username+"&password="+userpass+"&scope=Files.ReadWrite.All";

            using (var response = await httpclient.PostAsync(tokenConnection, 
                new StringContent(postBody, Encoding.UTF8, "application/x-www-form-urlencoded")))
            {
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonresult = Newtonsoft.Json.Linq.JObject.Parse(await response.Content.ReadAsStringAsync());
                        token = (string)jsonresult["access_token"];
                    }
                }
            return token;
        }
    }
}

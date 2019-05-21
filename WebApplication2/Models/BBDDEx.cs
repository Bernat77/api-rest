using System;
using System.Collections.Generic;
using System.Linq;
///////////////////////////////////
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Models;

namespace WebApplication2.Models
{
    public class BBDDEx
    {
        //Credenciales de azure (aplicación, admin)
        private const string clientId = "e0cb57ca-ddc7-4dd0-8481-ae827d70843f";
        private const string aadInstance = "https://login.microsoftonline.com/{0}";
        private const string tenant = "Bernats.onmicrosoft.com";
        private const string resource = "https://graph.microsoft.com";
        private const string appKey = "haF?sO]Q(wb8mX3hMC?Z58NbM9m4e>H)REh$[cd)4Noi?k[]$wlba-Fp-^]";
        private const string username = "Bernat@bernats.onmicrosoft.com";
        private const string userpass = "Glad2link";
        static string authority = String.Format(aadInstance, tenant);
        // Datos del documento Excel
        private const string excelName = "BookTest.xlsx";
        private const int table = 1;
        private static string id;
        private static string token;
        private static string session;
        
        // Http client y objetos del paquete active directory
        private static HttpClient httpclient = new HttpClient();
        private static AuthenticationContext authcontext = null;
        private static ClientCredential credential = null;

        public static string Main(FichajeDTO fichaje)
        {
            Management();

            Task<string> response = CheckRowById(id, token, session, fichaje);
            response.Wait();
            return response.Result;

        }

        private static void Management()
        {
            authcontext = new AuthenticationContext(authority);
            credential = new ClientCredential(clientId, appKey);

            Task<string> vartoken = GetTokenUser();
            vartoken.Wait();
            token = vartoken.Result;
          
            Task<string> varid = GetId(token);
            varid.Wait();
            id = varid.Result;
          
            Task<string> varsession = GetWorkbookSession(id, token);
            varsession.Wait();
            session = varsession.Result;

        }
        
        private static async Task<string> GetTokenUser()
        {
            string token = null;
            var tokenConnection = @"https://login.microsoftonline.com/common/oauth2/token";
            var content = "application/json";

            httpclient.DefaultRequestHeaders.Add("Accept", content);
            string postBody = "grant_type=password&resource=https://graph.microsoft.com" +
                "&client_id=" + clientId + "&client_secret=" + appKey +
                "&username=" + username + "&password=" + userpass + "&scope=Files.ReadWrite.All";

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
        
        private static async Task<string> GetId(string token)
        {
            string id = null;
            var uri = "https://graph.microsoft.com/v1.0/me/drive/root/children";
            httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var getResult = await httpclient.GetAsync(uri);

            if (getResult.Content != null)
            {
                var jarray = (Newtonsoft.Json.Linq.JArray)
                    Newtonsoft.Json.Linq.JObject.Parse(await getResult.Content.ReadAsStringAsync())["value"];
                foreach (var item in jarray)
                {

                    if (item["name"].ToString().Equals(excelName))
                    {
                        id = item["id"].ToString();
                    }
                }

            }

            return id;
        }

        private static async Task<string> GetWorkbookSession(string id, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                string sessionID = null;
                var uri = "https://graph.microsoft.com/v1.0/me/drive/items/" + id + "/workbook/createSession";
                httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                string body = "{\"persistChanges\":true}";

                var getResult = await httpclient.PostAsync(uri, new StringContent(body, Encoding.UTF8, "application/json"));

                if (getResult.Content != null)
                {
                    var jsonresult = Newtonsoft.Json.Linq.JObject.Parse(await getResult.Content.ReadAsStringAsync());
                    sessionID = (string)jsonresult["id"];
                }

                return sessionID;
            }

        }

        private static async Task<string> InsertRow(string idfile, string token, string session, FichajeDTO fichaje)
        {
            using (HttpClient client = new HttpClient())
            {
                var uri = $"https://graph.microsoft.com/v1.0/me/drive/items/{idfile}/workbook/tables('{table}')/rows";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Add("workbook-session-id", session);
                httpclient.DefaultRequestHeaders.Add("Accept", "application/json");
                string body = "{ \"values\": [ [ \"" + fichaje.id + "\", \"" + fichaje.dni + "\", \"" + fichaje.nombre + "\"," +
                    "\"" + fichaje.fechaentrada + "\", \"" + fichaje.horaentrada + "\"," +
                    "\"" + fichaje.fechasalida + "\",\"" + fichaje.horasalida + "\"," +
                    "\"" + fichaje.horastrabajadas + "\" ] ], \"index\": null }";

                var response = await client.PostAsync(uri, new StringContent(body, Encoding.UTF8, "application/json"));
             
                string content = null;

                if (response.Content != null)
                {
                    content = await response.Content.ReadAsStringAsync();
                }

                return content;
            }

        }

        private static async Task<string> UpdateRow(string idfile, string token, string session, FichajeDTO fichaje, int index)
        {
            using (HttpClient client = new HttpClient())
            {
                var uri = $"https://graph.microsoft.com/v1.0/me/drive/items/{idfile}/workbook/tables('{table}')/rows/" +
                    "/ItemAt(index=" + index + ")";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Add("workbook-session-id", session);
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                string body = "{ \"values\": [ [ \"" + fichaje.id + "\", \"" + fichaje.dni + "\", \"" + fichaje.nombre + "\"," +
                    "\"" + fichaje.fechaentrada + "\", \"" + fichaje.horaentrada + "\"," +
                    "\"" + fichaje.fechasalida + "\",\"" + fichaje.horasalida + "\"," +
                    "\"" + fichaje.horastrabajadas + "\" ] ], \"index\": " + index + " }";

                var httpRequestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), uri);
                httpRequestMessage.Content = new StringContent(body);

                var response = await client.SendAsync(httpRequestMessage);

                string content = null;

                if (response.Content != null)
                {
                    content = await response.Content.ReadAsStringAsync();
                }

                return content;
            }
        }


        private static async Task<string> CheckRowById(string idfile, string token, string session, FichajeDTO fichaje)
        {
            using (HttpClient client = new HttpClient())
            {
                var uri = $"https://graph.microsoft.com/v1.0/me/drive/items/{idfile}/workbook/tables('{table}')/rows";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Add("workbook-session-id", session);
                httpclient.DefaultRequestHeaders.Add("Accept", "application/json");

                var response = await client.GetAsync(uri);

                if (response.Content != null)
                {
                    var jsonresult = Newtonsoft.Json.Linq.JObject.Parse(await response.Content.ReadAsStringAsync());
                    var jarray = (Newtonsoft.Json.Linq.JArray)jsonresult["value"];
                    int index = FindRowById(jarray, fichaje.id);

                    if (index != -1)
                    {
                        return await UpdateRow(idfile, token, session, fichaje, index);
                    }
                    else
                    {
                        return await InsertRow(idfile, token, session, fichaje);
                    }
                }

                return "Error en la petición";

            }
        }

        private static int FindRowById(Newtonsoft.Json.Linq.JArray jArray, string idFichaje)
        {

            int index = -1;

            foreach (var item in jArray)
            {

                string id = item.ElementAt(2).ElementAt(0).ElementAt(0).ElementAt(0).ToString().Trim();
                if (id.Equals(idFichaje))
                {
                    index = int.Parse(item["index"].ToString().Trim());
                }
            }

            return index;
        }
               
    }
    
}


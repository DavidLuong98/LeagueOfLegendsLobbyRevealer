using System;
using System.Collections.Generic;
using System.Management;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LeagueLobby.Models;

namespace LeagueLobby
{
    public sealed class TeammateFinder
    {
        private static TeammateFinder? _instance;

        private const string GameName = "LeagueClientUx.exe";
        private const string ChampSelectEndpoint = "/chat/v5/participants/champ-select";
        private const string portKey = "--riotclient-app-port=";
        private const string authKey = "--riotclient-auth-token=";
        private const string encoding = "ISO-8859-1";

        private TeammateFinder() { }

        public static TeammateFinder Instance
        {
            get { return _instance ??= new TeammateFinder(); }
        }

        public async Task<string> GetTeammatesNames()
        {
            string json = await GetLobbyData();

            PlayersResponse? players = JsonSerializer.Deserialize<PlayersResponse>(json);
            if (players == null)
                throw new ArgumentNullException(nameof(players));
            
            if (players.Participants == null)
                throw new ArgumentNullException(nameof(players.Participants));

            StringBuilder sb = new StringBuilder();
            foreach (Player player in players.Participants)
            {
                sb.Append(player.Name);
                sb.Append("#");
                sb.Append(player.GameTag);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private async Task<string> GetLobbyData()
        {
            Dictionary<string, string> paramsCmd = ParamsCmdArgs();
            string token;
            if (paramsCmd.TryGetValue(portKey, out string? port) && paramsCmd.TryGetValue(authKey, out string? toka))
                token = Convert.ToBase64String(Encoding.GetEncoding(encoding).GetBytes($"riot:{toka}"));
            else
                throw new ArgumentNullException();

            try
            {
                using HttpClientHandler httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;

                using HttpClient httpClient = new HttpClient(httpClientHandler)
                {
                    BaseAddress = new Uri("https://127.0.0.1:" + port)
                };

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);

                HttpResponseMessage response = await httpClient.GetAsync(ChampSelectEndpoint);

                return response.IsSuccessStatusCode
                    ? await response.Content.ReadAsStringAsync()
                    : response.StatusCode.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private static string GetLeagueClientCmd()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");

#pragma warning disable CA1416
            ManagementClass managementClass = new ManagementClass("Win32_Process");
            foreach (ManagementBaseObject managementObject in managementClass.GetInstances())
            {
                if (managementObject["Name"].Equals(GameName))
                {
                    sb.Append(managementObject["CommandLine"]);
                    break;
                }
            }
#pragma warning restore CA1416

            if (sb.Length == 1)
                throw new InvalidOperationException("Expected to find the LeagueClientUx from the instances");

            sb.Append("]");

            return sb.ToString();
        }

        private static Dictionary<string, string> ParamsCmdArgs()
        {
            string cmd = GetLeagueClientCmd();
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            string[] parts = cmd.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                string stCur = part.Trim('"');
                if (stCur.StartsWith("--", StringComparison.Ordinal))
                {
                    int equalsIndex = part.IndexOf('=');
                    if (equalsIndex != -1)
                    {
                        string paramName = stCur.Substring(0, equalsIndex);
                        string paramValue = stCur.Substring(equalsIndex);
                        parameters[paramName] = paramValue;
                    }
                }
            }

            return parameters;
        }
    }
}
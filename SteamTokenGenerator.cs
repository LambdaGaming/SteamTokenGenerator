using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SteamTokenGenerator
{
	class SteamTokenGenerator
	{
		public static string GetKey()
		{
			string Path = @"\\SERVER-PC\Users\starw\Documents\key.txt";
			string ReadKey = File.ReadAllText( Path );
			Console.WriteLine( "\nReading user key..." );
			return ReadKey;
		}

		public static string GetJSON()
		{
			string URL = "https://api.steampowered.com/IGameServersService/GetAccountList/v1/?key=" + GetKey();
			string response;
			Console.WriteLine( "\nGrabbing JSON..." );
			WebRequest tokenRequest = WebRequest.Create( URL );
			WebResponse getToken = tokenRequest.GetResponse();
			using ( Stream dataStream = getToken.GetResponseStream() )
			{
				StreamReader reader = new StreamReader( dataStream );
				response = reader.ReadToEnd();
			}
			getToken.Close();
			Console.WriteLine( "\nReading JSON..." );
			return response;
		}

		public static void ParseJSON( string json )
		{
			Console.WriteLine( "\nParsing JSON..." );
			dynamic newjson = JObject.Parse( json );
			dynamic servers = newjson.response.servers;
			bool FoundUsedTokens = false;
			string FoundMessage;
			string Token = servers[0].login_token;
			Console.WriteLine( "\nSearching for used tokens..." );
			foreach ( dynamic token in servers )
			{
				if ( token.rt_last_logon != 0 )
				{
					string URL = "https://api.steampowered.com/IGameServersService/DeleteAccount/v1/?key=" + GetKey() + "&steamid=" + token.steamid;
					WebRequest tokenRequest = WebRequest.Create( URL );
					tokenRequest.Method = "POST";
					tokenRequest.ContentType = "application/x-www-form-urlencoded";
					tokenRequest.GetResponse();
					FoundUsedTokens = true;
				}
			}
			FoundMessage = FoundUsedTokens ? "Found used tokens. Deleting..." : "No used tokens found.";
			Console.WriteLine( FoundMessage );
			WriteKey( Token );
		}

		public static void WriteKey( string token )
		{
			string[] Paths = {
				@"\\SERVER-PC\lambda_cityrp",
				@"\\SERVER-PC\gmodserver",
				@"\\SERVER-PC\lambdarp",
				@"\\SERVER-PC\lambda_various"
			};
			Console.WriteLine( "\nWriting tokens to file..." );
			foreach ( string path in Paths )
			{
				File.WriteAllText( path + @"\token.txt", token );
			}
		}

		public static void CreateJSON()
		{
			Console.WriteLine( "\nCreating new token..." );
			Random rand = new Random();
			string URL = "https://api.steampowered.com/IGameServersService/CreateAccount/v1/?key=" + GetKey() + "&appid=4000&memo=AutoToken" + rand.Next( 1, 1001 );
			WebRequest tokenRequest = WebRequest.Create( URL );
			tokenRequest.Method = "POST";
			tokenRequest.ContentType = "application/x-www-form-urlencoded";
			tokenRequest.GetResponse();
			Console.WriteLine( "\nGiving Steam a few seconds to sync..." );
			Task.Delay( 3000 ).Wait();
			ParseJSON( GetJSON() );
		}

		static void Main( string[] args )
		{
			Console.WriteLine( "\nInitializing..." );
			CreateJSON();
			Console.WriteLine( "\nProcess finished. Press any key to continue..." );
			Console.ReadKey();
		}
	}
}

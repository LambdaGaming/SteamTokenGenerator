using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;

namespace SteamTokenGenerator
{
	class SteamTokenGenerator
	{
		static readonly string MainPath = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ) + @"\SteamTokenGenerator";
		static string Key;
		static string AppID;

		static string GetJSON()
		{
			string URL = $"https://api.steampowered.com/IGameServersService/GetAccountList/v1/?key={Key}";
			string response;
			Console.WriteLine( "\nGrabbing JSON..." );
			WebRequest tokenRequest = WebRequest.Create( URL );
			WebResponse getToken = tokenRequest.GetResponse();
			using ( Stream dataStream = getToken.GetResponseStream() )
			{
				StreamReader reader = new( dataStream );
				response = reader.ReadToEnd();
			}
			getToken.Close();
			Console.WriteLine( "\nReading JSON..." );
			return response;
		}

		static void WriteKey( string name, string token )
		{
			Console.WriteLine( "\nWriting tokens to file..." );
			File.WriteAllText( MainPath + @$"\{name}.txt", token );
		}

		static void CheckToken()
		{
			dynamic jsonlist = JObject.Parse( GetJSON() );
			dynamic servers = jsonlist.response.servers;
			bool FoundUsedTokens = false;
			Console.WriteLine( "\nSearching for old tokens..." );
			int count = 0;
			foreach ( dynamic token in servers )
			{
				if ( token.is_expired == true )
				{
					string deleteurl = $"https://api.steampowered.com/IGameServersService/DeleteAccount/v1/?key={Key}&steamid={token.steamid}";
					WebRequest deleterequest = WebRequest.Create( deleteurl );
					deleterequest.Method = "POST";
					deleterequest.ContentType = "application/x-www-form-urlencoded";
					deleterequest.GetResponse();
					FoundUsedTokens = true;

					Console.WriteLine( "\nCreating new token..." );
					string memo = $"AutoToken{AppID}{count}";
					string createurl = $"https://api.steampowered.com/IGameServersService/CreateAccount/v1/?key={Key}&appid={AppID}&memo={memo}";
					WebRequest createrequest = WebRequest.Create( createurl );
					createrequest.Method = "POST";
					createrequest.ContentType = "application/x-www-form-urlencoded";
					
					dynamic newjson = JObject.Parse( GetJSON() );
					dynamic newservers = newjson.response.servers;
					string tokenstr = servers[count].login_token;
					WriteKey( memo, tokenstr );
				}
				count++;
			}
			Console.WriteLine( FoundUsedTokens ? "\nRefreshing old tokens..." : "\nNo old tokens found." );
		}

		static void Main( string[] args )
		{
			if ( args.Length < 2 )
			{
				Console.WriteLine( "Arguments not provided. Aborting." );
				return;
			}

			AppID = args[0];
			Key = args[1];
			Console.WriteLine( "\nInitializing..." );
			CheckToken();
			Console.WriteLine( "\nProcess finished." );
		}
	}
}

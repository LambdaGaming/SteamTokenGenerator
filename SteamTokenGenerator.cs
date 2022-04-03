using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;

namespace SteamTokenGenerator
{
	class SteamTokenGenerator
	{
		static readonly string MainPath = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ) + @"\SteamTokenGenerator";

		static string GetKey()
		{
			string readkey = File.ReadAllText( MainPath + @"\key.txt" );
			Console.WriteLine( "\nReading user key..." );
			return readkey;
		}

		static string GetJSON()
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

		static void WriteKey( string token )
		{
			Console.WriteLine( "\nWriting tokens to file..." );
			File.WriteAllText( MainPath + @"\token.txt", token );
		}

		static void CheckToken()
		{
			Console.WriteLine( "\nParsing JSON..." );
			dynamic jsonlist = JObject.Parse( GetJSON() );
			dynamic servers = jsonlist.response.servers;
			bool FoundUsedTokens = false;
			Console.WriteLine( "\nSearching for old tokens..." );
			int count = 0;
			foreach ( dynamic token in servers )
			{
				if ( token.is_expired == true )
				{
					string deleteurl = "https://api.steampowered.com/IGameServersService/DeleteAccount/v1/?key=" + GetKey() + "&steamid=" + token.steamid;
					WebRequest deleterequest = WebRequest.Create( deleteurl );
					deleterequest.Method = "POST";
					deleterequest.ContentType = "application/x-www-form-urlencoded";
					deleterequest.GetResponse();
					FoundUsedTokens = true;

					Console.WriteLine( "\nCreating new token..." );
					string createurl = "https://api.steampowered.com/IGameServersService/CreateAccount/v1/?key=" + GetKey() + "&appid=4000&memo=AutoToken" + count;
					WebRequest createrequest = WebRequest.Create( createurl );
					createrequest.Method = "POST";
					createrequest.ContentType = "application/x-www-form-urlencoded";
					
					dynamic newjson = JObject.Parse( GetJSON() );
					dynamic newservers = newjson.response.servers;
					string tokenstr = servers[0].login_token;
					WriteKey( tokenstr );
				}
				count++;
			}
			Console.WriteLine( FoundUsedTokens ? "\nRefreshing old tokens..." : "\nNo old tokens found." );
		}

		static void Main( string[] args )
		{
			Console.WriteLine( "\nInitializing..." );
			CheckToken();
			Console.WriteLine( "\nProcess finished." );
		}
	}
}

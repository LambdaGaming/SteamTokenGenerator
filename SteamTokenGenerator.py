import json
import os
import requests
import sys

MainPath = os.path.expanduser( "~/Documents/SteamTokens" )

def GetJSON():
	print( "\nGrabbing JSON..." )
	jsonurl = requests.get( f"https://api.steampowered.com/IGameServersService/GetAccountList/v1/?key={Key}" )
	print( "\nReading JSON..." )
	return json.loads( jsonurl.text )

def WriteKey( name, token ):
	print( "\nWriting tokens to file..." )
	f = open( MainPath + f"/{name}.txt", "w+" )
	f.write( token )
	f.close()

def CheckToken():
	jsonfile = GetJSON()
	servers = jsonfile["response"]["servers"] if "servers" in jsonfile["response"] else None
	foundusedtokens = False
	print( "Searching for old tokens..." )
	count = 0

	if servers is None:
		print( f"No tokens found. Creating a new one for appid {AppID}..." )
		memo = f"AutoToken{AppID}{count}"
		requests.post( f"https://api.steampowered.com/IGameServersService/CreateAccount/v1/?key={Key}&appid={AppID}&memo={memo}" )
		newjson = GetJSON()
		newservers = newjson["response"]["servers"]
		tokenstr = newservers[0]["login_token"]
		WriteKey( memo, tokenstr )
		return

	for token in servers:
		if token["is_expired"]:
			requests.post( f"https://api.steampowered.com/IGameServersService/DeleteAccount/v1/?key={Key}&steamid={token['steamid']}" )
			foundusedtokens = True
			print( "\nCreating new token..." )
			memo = f"AutoToken{AppID}{count}"
			requests.post( f"https://api.steampowered.com/IGameServersService/CreateAccount/v1/?key={Key}&appid={AppID}&memo={memo}" )
			newjson = GetJSON()
			newservers = newjson["response"]["servers"]
			tokenstr = newservers[count]["login_token"]
			WriteKey( memo, tokenstr )
		count += 1
	print( foundusedtokens and "\nRefreshing old tokens..." or "\nNo old tokens found." )

if __name__ == "__main__":
	if not os.path.exists( MainPath ):
		os.makedirs( MainPath )

	if len( sys.argv ) < 2:
		print( "Arguments not provided. Aborting." )
		exit()
	
	global Key, AppID
	AppID = sys.argv[1]
	Key = sys.argv[2]
	print( "Initializing..." )
	CheckToken()
	print( "Process finished." )

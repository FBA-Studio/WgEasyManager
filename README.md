<h1 align="center"><img src="https://github.com/FBA-Studio/WgEasyManager/blob/main/raws/wg-easy-manager-logo.svg" height="33" align="center">WgEasyManager</h1>
<a href="#"><img alt="GitHub repo size" src="https://img.shields.io/github/repo-size/FBA-Studio/WgEasyManager"></a>
<a href="https://github.com/FBA-Studio/WgEasyManager/blob/main/LICENSE"><img alt="GitHub" src="https://img.shields.io/github/license/FBA-Studio/WgEasyManager"></a>
<a href="https://github.com/FBA-Studio/WgEasyManager/releases"><img alt="GitHub release (latest by date including pre-releases)" src="https://img.shields.io/github/v/release/FBA-Studio/WgEasyManager?include_prereleases"></a>
<a href="https://github.com/FBA-Studio/WgEasyManager/commits/main"><img alt="GitHub last commit" src="https://img.shields.io/github/last-commit/FBA-Studio/WgEasyManager"></a>
<a href="https://www.nuget.org/packages/WgEasyManager/"><img alt="Nuget" src="https://img.shields.io/nuget/dt/WgEasyManager"></a>
<a href="https://github.com/FBA-Studio/WgEasyManager/issues"><img alt="GitHub issues" src="https://img.shields.io/github/issues/FBA-Studio/WgEasyManager"></a>
<a href="https://twitter.com/FBA_Studio"><img alt="Twitter Follow" src="https://img.shields.io/twitter/follow/FBA_Studio?style=social"></a>

## .NET Library for working with WireGuard keys by [wg-easy](https://github.com/WeeJeWel/wg-easy) API
![This Library is helpful for Telegram bots💪🏻](https://github.com/FBA-Studio/WgEasyManager/blob/main/raws/wg-easy-banner.png)
## Installiation
Type this command in project's folder:
```
dotnet add package WgEasyManager
```
## Quick start
1. Get URL of your server with port
2. Get password for access
3. Initialize WgEasyClient:
```csharp
private static WgEasyClient client = new("sup3rSecr3tPassw0rd", "0.0.0.0:12345", true);
//if your server hasn't SSL - set false
```
4. Login to server:
```csharp
static async Task Main(){
    await client.LoginToServerIfNeeded();
    //some code...
}
```
After Initialize in compiled folder you can see "session.wgmanager". It's cookies for access to your server.

## Editing Keys
### 1. Get Keys
For getting keys use method `.GetKeys()`
```csharp
var keys = client.GetKeys();
//returns List<WireGuardKey>
```
### 2. Create new key
Use method `.CreateKey()` for key creating
#### Parameters:
- name - name of key
```csharp
client.CreateKey("Lance's key");
//return key info in object 'WireGuardKey'
```
<i>Also you can delete key with method `DeleteKey()` with parameter <b>clientId</b></i>
### 3. Block Key
Use method `.BlockKey()` for key baning
#### Parameters:
- clientId - Client ID in wg-easy
```csharp
client.DeleteKey("xxxx-xxxx-xxxx-xxxx");
// Key id you can get in object "WireGuardKey"
```
<i>Also you can unblock key with method `UnbanKey()` with parameter <b>clientId</b></i>

### 4. Rename key
Use method `.RenameKey()` for updating key's name
#### Parameters:
- clientId - client Id
- name - new name for this key
```csharp
client.RenameKey("xxxx-xxxx-xxxx-xxxx", "Lance's key");
```

### 5. Download .config file
Use method `.DownloadConfig()` for downloading .config file
#### Parameters:
- clientId - Client Id for downloading
- path - path for saving .config file
```
client.DownloadConfig("xxxx-xxxx-xxxx-xxxx", "path/to/download");
```
<i>Also you can download QR-Code with method `DownloadQrCode()` with parameters <b>clientId</b> and <b>path</b></i>

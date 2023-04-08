<h1><img src="https://github.com/FBA-Studio/WgEasyManager/blob/main/raws/wg-easy-manager-logo.svg" height="38" align="center">WgEasyManager</h1>
<a href="#"><img alt="GitHub repo size" src="https://img.shields.io/github/repo-size/FBA-Studio/WgEasyManager"></a>
<a href="#"><img alt="GitHub" src="https://img.shields.io/github/license/FBA-Studio/WgEasyManager"></a>
<a href="#"><img alt="GitHub tag (latest by date)" src="https://img.shields.io/github/v/tag/FBA-Studio/WgEasyManager"></a>

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
private static WgEasyClient client = new("sup3rSecr3tPassw0rd", "0.0.0.0:12345");
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
#### Parameters
- name - name of key
```csharp
client.CreateKey("Lance's key");
```

# WgEasyManager
## .NET Library for working with WireGuard keys by [wg-easy](https://github.com/WeeJeWel/wg-easy) API
![This Library is helpful for Telegram bots💪🏻](https://github.com/FBA-Studio/WgEasyManager/blob/main/raws/Wireguard.jpg)
## Quick start
1. Get URL of your server with port
2. Get password for access
3. Initialize WgEasyClient:
```csharp
WgEasyClient client = new("sup3rSecr3tPassw0rd", "0.0.0.0:12345");
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

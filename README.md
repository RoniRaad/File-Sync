# File-Sync
A solution including an API, WPF App, and Worker Windows Service. This solution uses a relative path system to allow folders with different names to have their contents synced between computers. It utilizes Azure Storage Accounts, AD, and App Services.

Requires an appsettings with the following values set for both UI and WindowService layers:
```
  "AzureAdConfig": {
    "AADInstance": "",
    "Tenant": "",
    "ClientId": "",
    "FileSyncScope": "",
    "FileSyncBaseAddress": ""
  }
  ```

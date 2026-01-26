This is the Aspire enabled version of the tutorial. It orchestrates only the _Server_ and the _Blazor_Client_ projects.

**Aspire Enabled Blazor WebAssembly Tutorial**
1. Install .NET 10.0 SDK from https://dotnet.microsoft.com/en-us/
2. Install Aspire from https://aspire.dev/get-started/install-cli/
3. Clone this repository
4. In the _AgUiAspire/Server_ folder copy file named `appsettings.json.dev` to `appsettings.json` and update the _GitHub:Token_ value with your personal access token.
5. Run the following command at a terminal window inside the _AgUiAspire_ folder.
   ```bash
   aspire run
   ```
6. The `Dashboard URL` will be displayed in the terminal once the projects are running.
7. Point your browser to the dashboard URL to access the Aspire Dashboard.
8. Click on the Blazor_Client service to try the blazor client application.
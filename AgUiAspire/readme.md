This is the Aspire enabled version of the tutorial. It orchestrates only the _Server_ and the _Blazor_Client_ projects.

**Aspire Enabled Blazor WebAssembly Tutorial**
1. Install .NET 10.0 SDK from https://dotnet.microsoft.com/en-us/
2. Clone this repository
3. In the _AgUiAspire/Server_ folder copy file named `appsettings.json.dev` to `appsettings.json` and update the _GitHub:Token_ value with your personal access token.

4. Use Aspire

<details><summary>Without Aspire Tool</summary>
Change directory into the `AgUiAspire.AppHost` folder and load Aspire dashboard with these terminal window commands:

```bash
cd AgUiAspire.AppHost
dotnet watch
````
</details>

<details><summary>Using Aspire Tool</summary>

Install Aspire from https://aspire.dev/get-started/install-cli/. 

Then, run the following command at a terminal window inside the `AgUiAspire` folder.

```bash
aspire run
```
</details>

5. The `Dashboard URL` will be displayed in the terminal once the projects are running.
6. Point your browser to the dashboard URL to access the Aspire Dashboard.
7. Click on the Blazor_Client service to try the blazor client application.

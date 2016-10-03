#Cloud Foundry Visual Studio Extension

The Cloud Foundry Visual Studio Extension enables Cloud Foundry users to publish applications from Visual Studio directly to a Cloud Foundry deployment.

##Features

- Supports multiple Cloud Foundry targets
- User interface for pushing applications using the Cloud Foundry MSBuild Tasks
- View organizations, spaces, apps, services
- Manage apps, services and bindings

##Installing
- Download and install CloudFoundry.VisualStudio.vsix from the Visual Studio Gallery
- Restart Visual Studio


##Building

###Prerequisites
- Visual Studio 2013 or Visual Studio 2015
- Visual Studio SDK - [https://www.microsoft.com/en-us/download/details.aspx?id=40758](https://www.microsoft.com/en-us/download/details.aspx?id=40758)
- Git client

###Build:

- Clone this repository
- Open cf-vs-extension.sln in Visual Studio
- Build CloudFoundry.VisualStudio

For information about the Cloud Foundry MSBuild Tasks, visit: [https://github.com/hpcloud/cf-msbuild-tasks](https://github.com/hpcloud/cf-msbuild-tasks)

###Debugging CloudFoundry.VisualStudio
- Set your debug start action to `Start external program` and use the following path: `C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.exe` 
- Set the `Command line arguments` to `/rootsuffix Exp`

## Using the Extension

###Cloud Foundry Explorer

The Explorer is a TreeView that enables you to browse your organizations, view application states and delete apps and services.
You can view the Cloud Foundry Eplorer window by opening it selecting View -> Other Windows -> Cloud Foundry Explorer or using the keyboard shortcut: Ctrl+K, Shift+C

####TreeView Nodes

- `CloudFoundryTarget`
 - actions: refresh, remove
 - details: version, url, username, ignore ssl errors
- `Organization`
 - actions: refresh
 - details: current roles in org, org name, when it was created
- `Space`
 - actions: refresh
 - details: current roles in space, space name, when it was created
- `App`
 - actions: browse, start, restart, stop, delete
 - details:  when it was created, name, buildpack, max memory, instance count
- `Service`
 - actions: delete
 - details: current roles in org, org name, when it was created
- `Route`
 - actions: delete
 - details: domain for route, apps bound to route

###Publish app to Cloud Foundry
You can push a Web Application to Cloud Foundry using the cf-msbuild-tasks - [https://github.com/hpcloud/cf-msbuild-tasks](https://github.com/hpcloud/cf-msbuild-tasks)

- Add nuget package `cf-msbuild-tasks` in your project
- Right click on your web project -> Publish to Cloud Foundry
- Fill out the publish form and click Publish or save your publish profile for later.

##Contributing
We are looking forward to contributions from the Cloud Foundry community.
Feel free to open a pull request or an issue if you find one.

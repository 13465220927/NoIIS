# NoIIS
NoIIS is a lightweight C# web server for the `IHttpHandlerFactory` and `IHttpHandler` to avoid the huge and heavy IIS web server for some cases. Projects for NoIIS are compatible with the IIS if you utilise the build-in interfaces `IHttpHandlerBaseFactory` and `IHttpHandlerBase`. You can study examples to get the main points: [Example002](https://github.com/SommerEngineering/Example002) about how to use the NoIIS as stand-alone server and [example004](https://github.com/SommerEngineering/Example004) about how to set-up and run the NoIIS server directly from your code. Further, [example006](https://github.com/SommerEngineering/Example006) shows how to consume and provide a JSON-RPC service. NoIIS works with .NET on Microsoft Windows and also with Mono on Unix, Linux and Mac OS X.

## Get NoIIS
The fasted way is to use NoIIS from NuGet: https://www.nuget.org/packages/NoIIS/.

## Limits
NoIIS is not yet a full replacement for an ISS. The following properties of the `HttpRequestBase` class are not yet implemented:
* AnonymousID
* ApplicationPath
* AppRelativeCurrentExecutionFilePath
* Browser
* ClientCertificate
* CurrentExecutionFilePath
* CurrentExecutionFilePathExtension
* Filter
* HttpChannelBinding
* LogonUserIdentity
* PhysicalApplicationPath
* PhysicalPath
* ReadEntityBodyMode
* RequestContext
* ServerVariables
* TimedOutToken
* Unvalidated

Further, the following methods of the `HttpRequestBase` class are not yet implemented:
* BinaryRead()
* InsertEntityBody()
* MapImageCoordinates()
* MapPath()
* MapRawImageCoordinates()
* SaveAs()
* ToString()
* ValidateInput()

For many projects, these missing methods and properties are not relevant. If you have an issue, please consider if your code tries to use any of these.

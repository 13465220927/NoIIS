# NoIIS
NoIIS is a lightweight C# web server for the `IHttpHandlerFactory` and `IHttpHandler` to avoid the huge and heavy IIS web server for some cases. Projects for NoIIS are compatible with the IIS if you utilise the build-in interfaces `IHttpHandlerBaseFactory` and `IHttpHandlerBase`. You can study examples to get the main points: [Example002](https://github.com/SommerEngineering/Example002) about how to use the NoIIS as stand-alone server and [example004](https://github.com/SommerEngineering/Example004) about how to set-up and run the NoIIS server directly from your code. Further, [example006](https://github.com/SommerEngineering/Example006) shows how to consume and provide a JSON-RPC service. NoIIS works with .NET on Microsoft Windows and also with Mono on Unix, Linux and Mac OS X.

## Get NoIIS
The fasted way is to use NoIIS from NuGet: https://www.nuget.org/packages/NoIIS/.

## Configuration
The correct configuration is important in order to guarantee a successful performance.

1. The temporary folder is important because NoIIS stores the requests there. These could contain attached files or form data.
2. The `maxRequestSizeBytes` parameter sets the maximal length of the request. This includes any uploaded files and form data. Any request which is larger gets rejected.
3. The `visitsMinimum` parameter defines how many requests a client must perform within `entryTimeSeconds` in order to get not blocked. Set the `visitsMinimum` to `0` to disable this function.
4. The `visitsMaximum` parameter defines how many requests a client can perform within `keepAliveTimeSeconds` in order to get now blocked. Set the `visitsMaximum` to `0` to disable this function.
5. The `blockTimeSeconds` parameter defines how long a client gets blocked.
6. The `clientLifeTimeSeconds` parameter defines how long NoIIS keeps track of a client's profile and status. It gets measured after the last client's visit.

In order to work properly, the following rule should applied:
- `entryTimeSeconds` < `keepAliveTimeSeconds` in order to allow NoIIS to keep trach of all entry requests.
- `blockTimeSeconds` < `clientLifeTimeSeconds` in order to guarantee the desired blocking time.
- `entryTimeSeconds` < `clientLifeTimeSeconds` in order to allow the desired blocking algorithm to work.
- `clientLifeTimeSeconds` < `24h` because many customers's IP address gets changed after one day. Thus, a further control is not possible.
- `visitsMinimum` < `visitsMaximum` otherwise each client gets blocked.

One example regarding the blocking function:
- `visitsMinimum=6`
- `entryTimeSeconds=30` i.e. 1/2 minute
- `visitsMaximum=200`
- `keepAliveTimeSeconds=120` i.e. 2 minutes
- `blockTimeSeconds=300` i.e. 5 minutes
- `clientLifeTimeSeconds=900` i.e. 15 minutes

Now, any client must perform at least `6` request at the first `1/2` minute. Otherwise, the client get blocked. After successfully do so, the client cannot perform more than `200` requests at any time over the last `2` minutes. Otherwise, it gets blocked. A blocked client gets ignored for `5` minutes. If a client goes offline, NoIIS keep track of the last status for `15` minutes.

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
* InsertEntityBody()
* MapImageCoordinates()
* MapPath()
* MapRawImageCoordinates()
* SaveAs()
* ToString()
* ValidateInput()

For many projects, these missing methods and properties are not relevant. If you have an issue, please consider if your code tries to use any of these.

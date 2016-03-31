A plugin for [ServiceStack](https://servicestack.net/) that allows requests to be tracked across multiple services. This is done via an http header. 
If no request identifier is found one is created and appended to incoming request and then added to outgoing response object for every service.
If a request id already exists then this is appended to the outgoing response object.

# Quick Start

The plugin is added like any other. By default it has no external dependencies that need to be provided.
```csharp
Plugins.Add(new RequestCorrelationFeature());
```
Both the http header name (default: "x-mac-requestid") and request id generation method (default: [RustFlakes](https://github.com/peschkaj/rustflakes)) can be customised:
```csharp
Plugins.Add(new RequestCorrelationFeature
{
    HeaderName = "x-my-custom-header",
    IdentityGenerator = new MyIdentityGenerator()
});

// public class MyIdentityGenerator : IIdentityGenerator {}
```
Where IIdentityGenerator has a single method that needs to be implemented. This method is called each time a request id is created:
```csharp
string GenerateIdentity();
```

# Demo
There is a demo project "DemoService" within the solution. This is a console app that starts up a self-hosted app host on http://127.0.0.1:8090 with the request correlation plugin setup. 

It has a single service with default routing. When calling http://127.0.0.1:8090/json/reply/demorequest with Postman/Fiddler etc it will return a response which contains the generated request id. It will also return the request id in the header.

# Why?
When used with alongside a logging strategy it will enable easier tracing of requests across multiple services.
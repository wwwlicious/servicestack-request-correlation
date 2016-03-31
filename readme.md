A plugin for [ServiceStack](https://servicestack.net/) that allows requests to be tracked across multiple services. This is done via an http header. If no request identifier is found one is created and appended to incoming request and then added to outgoing response object. 
If a request id already exists then this is appended to the outgoing response object.

# Quick Start

The plugin is added like any other. By default it has no external dependencies that need to be provided.
```csharp
Plugins.Add(new RequestCorrelationFeature());
```
Both the http header name (default: "x-mac-requestid") and request id generation method (default: [RustFlakes](https://github.com/peschkaj/rustflakes)) can be customised if required:
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
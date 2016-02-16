# Introduction

WebTraceListener is a .NET assembly that exposes trace logs via a web page and updates them in real time using web sockets.

I initially implemented this to provide a simple way to see real time trace logs for an ASP.NET web site.  However
it works just as well for a desktop application where you want a local or remote view into what's happening.

![](http://i.imgur.com/LVOJlbX.png)

## Installation

Include the assembly in your app.

The WebTraceListener starts an http listener (on port 8080 by default) and serves a single web page with the real time trace logs.

If your app is not running elevated, you need to grant permissions for the the app to listen on port 8080 with:

```
netsh http add urlacl url=http://+:8080/ user=.\Users
```

To open the firewall to allow port 8080 connections, you need to add firewall rules:

```
netsh advfirewall firewall add rule name="WebTraceListener HTTP Port 8080" dir=in action=allow protocol=TCP localport=8080
netsh advfirewall firewall add rule name="WebTraceListener HTTP Port 8080" dir=out action=allow protocol=TCP localport=8080
```

## Customizing look and feel

Look and feel is defined in embedded CSS in an embedded HTML file (TraceLog.html)

![](http://i.imgur.com/51D8sQ1.png)

Modify to your heart's content.
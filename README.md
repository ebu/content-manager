EBU Content Manager
===================

The Content Manager is a visual production tool which is able to generate on the fly 
visualisation for DAB slideshow and RadioVIS. 
.NET framework based, it is developped in C# and is distributed under LGPL licence.

The Content Manager can run in a standalone fashion or in an advanced DataGateway environment
allowing external data providers such as Swisstiming to provide valuable data to generate
slideshows for live sports events.

The configuration and operations documentation can be found in the 
[Content Manager README](io.ebu.eis.contentmanager/README.md).

## Dependencies

Beside the Nuget Package Manager dependencies which are resolved automatically by Visual Studio, the Content Manager
does not require any other depencies to be installed on the development machine.

### Nuget Dependencies

The following third-party libraries use in the solutions:

* Apache.NMS
* Apache.NMS.Stomp
* RabbitMQ.Client
* AWSSDK
* PhantomJS

## License

The Content Manager and its libraries are distributed under LGPL licence.

Multiple external libraries are used and referenced using NuGet Package Manager.
According licenses are available on the repository sites. 
Other libraries not included using NuGet are listed explicitely below.

### Licenses of third party components

#### Apache.NMS
License under Apache 2.0  
http://activemq.apache.org/nms/

#### AMQP.Client
.NET/C# AMQP client library and WCF binding  
The library is open-source, and is dual-licensed under the Apache License v2 and the Mozilla Public License v1.1.  
http://www.rabbitmq.com/dotnet.html

#### AWS SDK for .NET
Licensed under Apache 2.0  
https://github.com/aws/aws-sdk-net/blob/master/License.txt

#### PhantomJS
PhantomJS Nuget Packaget https://www.nuget.org/packages/PhantomJS/,   
Licensed under BSD  
http://phantomjs.org, https://github.com/ariya/phantomjs/blob/master/LICENSE.BSD



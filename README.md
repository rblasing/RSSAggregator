# RSS Aggregator

This application provides consumers with a consolidated view of a set of RSS news feeds, those consumers being either end users who make use of its web site, or external systems who make use of one of its web services. It is intended to serve as a "portfolio" for potential employers, demonstrating usage of the following languages, technologies, and concepts. It is free of any former employer's intellectual property. It may be deployed on a dedicated server, or in Azure.

| <!-- --> | <!-- -->  | <!-- -->  | <!-- --> | <!-- -->  | <!-- --> 
| ---- | ---- | ---- | ---- | ---- | ---- 
| C# | JavaScript | Regular expressions | SQL | LINQ | HTML 
| CSS  | Responsive web design | RSS | PowerShell | ASP.NET MVC | WCF 
| WebAPI | SOAP | REST | Windows services | SVG | log4net
| XML | XSLT | XPath | [WebSQL](https://www.w3.org/TR/webdatabase/) | [IndexedDB](https://www.w3.org/TR/IndexedDB-2/) | [Web storage](https://www.w3.org/TR/webstorage/) 
| [Server-Sent Events](https://html.spec.whatwg.org/multipage/server-sent-events.html) | [OpenXML](https://docs.microsoft.com/en-us/office/open-xml/about-the-open-xml-sdk) | SQL/XML | AJAX | | |  

<br /><br />
<!-- A live demo is available at [zoot.azurewebsites.net/news](https://zoot.azurewebsites.net/news) -->
<br /><br />

## Features
* Regularly queries a configured set of RSS feeds, using a long running process that can be executed as either a console app, Windows service, or Azure WebJob 
* Stores all new feed items in the application's database
* Publishes a static RSS XML file containing the latest 100 aggregated feed items, which is rendered as HTML in a browser via a referenced XSLT
<br />

* Server worker pushes new items to browser clients via Server-Sent Events
![Web site home page](/img/homepage.webp)
<br/>

* Automatically recovers from client browser disconnects by pushing catchup item messages, using the SSE *Last-Event-ID* header (unsupported by Firefox)
* Retrieves a set of catchup items on initial page launch (only if on LAN or WAN, as detected by the Network Connection API)
* Filters images and feed 'flare' (social media links) from new items before pushing to clients
* Caches latest 1000 items in browser's local database, using WebSQL or IndexedDB, depending on browser support
* Enables offline infinite scrolling of past 1000 items stored in browser's local database
* Allows management of feeds queried by the aggregator process via web site
* Is responsive to device screen size
* Allows user to set font size for main page independently of browser settings, saving preference in browser's local storage
* Highlights items which contain user-defined keywords (which are also saved in local storage)
* Allows searching by keyword, and downloading of results as XSLT using OpenXML
* Demonstrates usage of the SQL *xml* data type by executing queries against columns defined as such
* Logs system warnings and errors to file and application database using log4net
* Exposes a WCF service which responds to both SOAP and JSON/HTTP requests
* Exposes a WebAPI service which responds to RESTful requests
<br />

* Displays a live heat map of news activity by state, using a clickable SVG image (also driven by Server-Sent Events)
![Web site heat map](/img/heatmap.webp)

# Copyright, 2020, Richard Blasingame

function WriteXmlToScreen([xml]$xml)
{
   $sw = new-object System.IO.StringWriter
   $xw = new-object System.Xml.XmlTextWriter $sw
   $xw.Formatting = "indented"
   $xml.WriteTo($xw)
   $xw.Flush()
   $sw.Flush()
   write-output $sw.ToString()
}

cls


# test WCF service

$site = "https://zoot.azurewebsites.net/rsswcf"

"JSON getTopItems"
$cl = new-object System.Net.WebClient
$cl.Headers.Add("Accept", "application/json; charset=utf-8");
$cl.Headers.Add("Content-Type", "application/json; charset=utf-8");
$out = $null
$out = $cl.UploadData($site + "/RSSService.svc/json/getTopItems", [System.Text.Encoding]::ASCII.GetBytes('{"itemCount":5}'))
[System.Text.Encoding]::ASCII.GetString($out) | ConvertFrom-Json | ConvertTo-Json

""
""
"JSON getItemsByRange"
$cl = new-object System.Net.WebClient
$cl.Headers.Add("Content-Type", "application/json");

$out = $cl.UploadData($site + "/RSSService.svc/json/getItemsByRange",
   [System.Text.Encoding]::ASCII.GetBytes('{"minDateTime":"\/Date(1604886000000)\/","maxDateTime":"\/Date(1604972400000)\/"}'))

[System.Text.Encoding]::ASCII.GetString($out) | ConvertFrom-Json | ConvertTo-Json

""
""
"JSON getItemsByKeyword"
$cl = new-object System.Net.WebClient
$cl.Headers.Add("Content-Type", "application/json");
$out = $cl.UploadData($site + "/RSSService.svc/json/getItemsByKeyword", [System.Text.Encoding]::ASCII.GetBytes('{"keyword":"asteroid"}'))
[System.Text.Encoding]::ASCII.GetString($out) | ConvertFrom-Json | ConvertTo-Json



""
""
"SOAP getTopItems"
$cl = new-object System.Net.WebClient
$cl.Headers.Add("Content-Type", "text/xml;charset=UTF-8");
$cl.Headers.Add("SOAPAction", "http://blasingame/getTopItems");

$sendBytes = [System.Text.UTF8Encoding]::UTF8.GetBytes("<s:Envelope xmlns:s='http://schemas.xmlsoap.org/soap/envelope/'>
<s:Header />
<s:Body>
   <getTopItemsRequest xmlns='http://blasingame/RSS.xsd'>
      <itemCount>3</itemCount>
   </getTopItemsRequest>
</s:Body>
</s:Envelope>")

$out = $cl.UploadData($site + "/RSSService.svc", $sendBytes)
WriteXmlToScreen([System.Text.Encoding]::ASCII.GetString($out))

""
""
"SOAP getItemsByRange"
$cl = new-object System.Net.WebClient
$cl.Headers.Add("Content-Type", "text/xml");
$cl.Headers.Add("SOAPAction", "http://blasingame/getItemsByRange");

$sendBytes = [System.Text.UTF8Encoding]::UTF8.GetBytes("<s:Envelope xmlns:s='http://schemas.xmlsoap.org/soap/envelope/'>
      <s:Body>
         <getItemsByRangeRequest xmlns='http://blasingame/RSS.xsd'>
            <minDateTime>2020-08-05T00:00:00Z</minDateTime>
            <maxDateTime>2020-08-06T00:00:00Z</maxDateTime>
         </getItemsByRangeRequest>
      </s:Body>
   </s:Envelope>")

$out = $cl.UploadData($site + "/RSSService.svc", $sendBytes)
WriteXmlToScreen([System.Text.Encoding]::ASCII.GetString($out))

""
""
"SOAP getItemsByKeyword"
$cl = new-object System.Net.WebClient
$cl.Headers.Add("Content-Type", "text/xml");
$cl.Headers.Add("SOAPAction", "http://blasingame/getItemsByKeyword");

$sendBytes = [System.Text.UTF8Encoding]::UTF8.GetBytes("<s:Envelope xmlns:s='http://schemas.xmlsoap.org/soap/envelope/'>
      <s:Body>
         <getItemsByKeywordRequest xmlns='http://blasingame/RSS.xsd'>
            <keyword>asteroid</keyword>
         </getItemsByKeywordRequest>
      </s:Body>
   </s:Envelope>")

$out = $cl.UploadData($site + "/RSSService.svc", $sendBytes)
WriteXmlToScreen([System.Text.Encoding]::ASCII.GetString($out))



# test WebAPI service

$site = "https://zoot.azurewebsites.net/rsswebapi"

""
""
"REST getTopItems"
$cl = new-object System.Net.WebClient
$cl.Headers.Add("Content-Type", "application/json");
$out = $cl.DownloadData($site + "/api/rss/top/3")
[System.Text.Encoding]::ASCII.GetString($out) | ConvertFrom-Json | ConvertTo-Json

""
""
"REST getItemsByRange"
$cl = new-object System.Net.WebClient
$cl.Headers.Add("Content-Type", "application/json");
$out = $cl.DownloadData($site + "/api/rss/search/2020-10-05T00:00:00Z/2020-10-06T00:00:00Z")
[System.Text.Encoding]::ASCII.GetString($out) | ConvertFrom-Json | ConvertTo-Json

""
""
"REST getItemsByKeyword"
$cl = new-object System.Net.WebClient
$cl.Headers.Add("Content-Type", "application/json");
$out = $cl.DownloadData($site + "/api/rss/search/asteroid")
[System.Text.Encoding]::ASCII.GetString($out) | ConvertFrom-Json | ConvertTo-Json

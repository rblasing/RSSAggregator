// Copyright 2020 Richard Blasingame. All rights reserved.

using System;


namespace RSSTests
{
   public class TestBase
   {
      protected static string testItemNode =
@"<item xmlns:ns1=""http://namespace1"">
<title>Test title</title>
<ns1:description>Test description</ns1:description>
<pubDate>Thu, 29 Oct 2020 12:37:04 Z</pubDate>
<ns1:link>https://www.anywhere.com/news/some-article/</ns1:link>
<ttl>30</ttl>
<pi>3.14</pi>
</item>";

      protected static string testChannelNode = string.Format(
@"<channel xml:base=""http://base1"" xmlns:ns1=""http://namespace1"">
<title>Channel title</title>
<description>Channel description</description>
<lastBuildDate>Wed, 28 Oct 2020 01:38:00 Z</lastBuildDate>
<pubDate>Fri, 30 Oct 2020 01:38:00 Z</pubDate>
<ns1:link>http://sitename/newsFeed.rss</ns1:link>
<ttl>30</ttl>{0}</channel>", testItemNode);

      protected static string testRssNode = string.Format(
         @"<rss xmlns:ns1=""http://namespace1"" version='2.0'>{0}</rss>",
         testChannelNode);
   }
}

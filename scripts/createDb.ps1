﻿# Copyright, 2020, Richard Blasingame

$azure = "Server=tcp:____,1433;Initial Catalog=RSS;Persist Security Info=False;User ID=____;Password=____;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30"
$local = "Data Source=____;Initial Catalog=RSS;timeout=1200;Integrated Security=true"

$db = new-object System.Data.SqlClient.SqlConnection
$db.ConnectionString = $azure
$db.Open()

$cmd = new-object System.Data.SqlClient.SqlCommand

# drop existing tables
$cmd.Connection = $db
$cmd.CommandText = "DROP TABLE rss_item"
$cmd.ExecuteNonQuery()

$cmd.Connection = $db
$cmd.CommandText = "DROP TABLE rss_feed"
$cmd.ExecuteNonQuery()

$cmd.Connection = $db
$cmd.CommandText = "DROP TABLE state"
$cmd.ExecuteNonQuery()

$cmd.Connection = $db
$cmd.CommandText = "DROP TABLE log4net"
$cmd.ExecuteNonQuery()

# create cables
$cmd.CommandText = "CREATE TABLE rss_feed (feed_id INT PRIMARY KEY IDENTITY(1, 1), title NVARCHAR(MAX) NOT NULL, url NVARCHAR(MAX) NOT NULL, active BIT NOT NULL, regional BIT NOT NULL)"
$cmd.Connection = $db
$cmd.ExecuteNonQuery()

$cmd.CommandText = "CREATE TABLE rss_item (feed_id INT FOREIGN KEY REFERENCES rss_feed(feed_id), ins_date DATETIME2 NOT NULL DEFAULT (GETUTCDATE()), pub_date DATETIME2 NOT NULL, title NVARCHAR(MAX) NOT NULL, description NVARCHAR(MAX), url NVARCHAR(400) NOT NULL, xml XML, CONSTRAINT PK_rss_item PRIMARY KEY (url))"
$cmd.Connection = $db
$cmd.ExecuteNonQuery()

$cmd.CommandText = "CREATE TABLE state (name NVARCHAR(25) NOT NULL, abbr NCHAR(2) NOT NULL, ref_count INT NOT NULL DEFAULT 0)"
$cmd.Connection = $db
$cmd.ExecuteNonQuery()

$cmd.CommandText = "CREATE TABLE log4net (id INT IDENTITY (1, 1) NOT NULL, date DATETIME NOT NULL, thread VARCHAR(255) NOT NULL, level VARCHAR(50) NOT NULL, logger VARCHAR(255) NOT NULL, message VARCHAR(4000) NOT NULL, exception VARCHAR(2000) NULL)"
$cmd.Connection = $db
$cmd.ExecuteNonQuery()

# seed with data

$feeds = "INSERT INTO rss_feed (title, url, active, regional) VALUES ('CNN', 'http://rss.cnn.com/rss/cnn_topstories.rss', 1, 0)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('The New York Times', 'http://www.nytimes.com/services/xml/rss/nyt/HomePage.xml', 1, 0)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('FOX News', 'http://feeds.foxnews.com/foxnews/latest', 1, 0)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('USA Today', 'http://rssfeeds.usatoday.com/UsatodaycomNation-TopStories', 1, 0)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('NPR', 'http://www.npr.org/rss/rss.php?id=1001', 1, 0)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('LA Times', 'https://www.latimes.com/local/rss2.0.xml', 1, 1)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('NBC News', 'http://www.nbcnews.com/id/3032091/device/rss/rss.xml', 1, 0)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('ABC News', 'http://feeds.abcnews.com/abcnews/topstories', 1, 0)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('NPR All Things Considered', 'https://feeds.npr.org/2/rss.xml', 1, 0)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('Savannah Now', 'https://www.savannahnow.com/news?template=rss&mime=xml', 1, 1)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('The Valdosta Daily Times', 'https://www.valdostadailytimes.com/search/?f=rss&t=article&c=news&l=50&s=start_time&sd=desc', 1, 1)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('Slashdot', 'http://rss.slashdot.org/Slashdot/slashdot', 1, 0)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('Space', 'https://www.space.com/feeds/all', 1, 0)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('Scientific American', 'http://rss.sciam.com/ScientificAmerican-Global', 1, 0)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('Wired', 'https://www.wired.com/feed/', 1, 0)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('TechCrunch', 'http://feeds.feedburner.com/Techcrunch', 1, 0)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('AJC Music', 'https://www.ajc.com/news/app-atlanta-music-scene-list/?outputType=rss', 1, 1)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('AJC Cobb County', 'https://www.ajc.com/neighborhoods/cobb-county-list/?outputType=rss', 1, 1)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('AJC Georgia', 'https://www.ajc.com/news/georgia-news/app/?outputType=rss', 1, 1)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('AJC Atlanta', 'https://www.ajc.com/neighborhoods/intown-atlanta-list/?outputType=rss', 1, 1)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('AJC Top Stories', 'https://www.ajc.com/news/breaking-news-headlines', 1, 1)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('Tallahassee Democrat', 'http://rssfeeds.tallahassee.com/tallahassee/home', 1, 1)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('WTXL ABC 27', 'https://www.wtxl.com/news/local-news.rss', 1, 1)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('First Coast News', 'http://rssfeeds.firstcoastnews.com/wtlv/firstcoastnews-topstories', 1, 1)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('News4JAX', 'https://www.news4jax.com/arcio/rss/category/news/?size=10', 1, 1)",
   "INSERT INTO rss_feed (title, url, active, regional) VALUES ('Marietta Daily Journal', 'https://www.mdjonline.com/search/?f=rss&t=article&c=neighbor_newspapers&l=50&s=start_time&sd=desc', 1, 1)"

$feeds | foreach {
   $cmd.CommandText = $_
   $cmd.Connection = $db
   $cmd.ExecuteNonQuery()
}

$states = "INSERT INTO state (name, abbr) VALUES ('ALABAMA', 'AL')",
   "INSERT INTO state (name, abbr) VALUES ('ALASKA', 'AK')",
   "INSERT INTO state (name, abbr) VALUES ('ARIZONA', 'AZ')",
   "INSERT INTO state (name, abbr) VALUES ('ARKANSAS', 'AR')",
   "INSERT INTO state (name, abbr) VALUES ('CALIFORNIA', 'CA')",
   "INSERT INTO state (name, abbr) VALUES ('COLORADO', 'CO')",
   "INSERT INTO state (name, abbr) VALUES ('CONNECTICUT', 'CT')",
   "INSERT INTO state (name, abbr) VALUES ('DISTRICT OF COLUMBIA', 'DC')",
   "INSERT INTO state (name, abbr) VALUES ('DELAWARE', 'DE')",
   "INSERT INTO state (name, abbr) VALUES ('FLORIDA', 'FL')",
   "INSERT INTO state (name, abbr) VALUES ('GEORGIA', 'GA')",
   "INSERT INTO state (name, abbr) VALUES ('HAWAII', 'HI')",
   "INSERT INTO state (name, abbr) VALUES ('IDAHO', 'ID')",
   "INSERT INTO state (name, abbr) VALUES ('ILLINOIS', 'IL')",
   "INSERT INTO state (name, abbr) VALUES ('INDIANA', 'IN')",
   "INSERT INTO state (name, abbr) VALUES ('IOWA', 'IA')",
   "INSERT INTO state (name, abbr) VALUES ('KANSAS', 'KS')",
   "INSERT INTO state (name, abbr) VALUES ('KENTUCKY', 'KY')",
   "INSERT INTO state (name, abbr) VALUES ('LOUISIANA', 'LA')",
   "INSERT INTO state (name, abbr) VALUES ('MAINE', 'ME')",
   "INSERT INTO state (name, abbr) VALUES ('MARYLAND', 'MD')",
   "INSERT INTO state (name, abbr) VALUES ('MASSACHUSETTS', 'MA')",
   "INSERT INTO state (name, abbr) VALUES ('MICHIGAN', 'MI')",
   "INSERT INTO state (name, abbr) VALUES ('MINNESOTA', 'MN')",
   "INSERT INTO state (name, abbr) VALUES ('MISSISSIPPI', 'MS')",
   "INSERT INTO state (name, abbr) VALUES ('MISSOURI', 'MO')",
   "INSERT INTO state (name, abbr) VALUES ('MONTANA', 'MT')",
   "INSERT INTO state (name, abbr) VALUES ('NEBRASKA', 'NE')",
   "INSERT INTO state (name, abbr) VALUES ('NEVADA', 'NV')",
   "INSERT INTO state (name, abbr) VALUES ('NEW HAMPSHIRE', 'NH')",
   "INSERT INTO state (name, abbr) VALUES ('NEW JERSEY', 'NJ')",
   "INSERT INTO state (name, abbr) VALUES ('NEW MEXICO', 'NM')",
   "INSERT INTO state (name, abbr) VALUES ('NEW YORK', 'NY')",
   "INSERT INTO state (name, abbr) VALUES ('NORTH CAROLINA', 'NC')",
   "INSERT INTO state (name, abbr) VALUES ('N CAROLINA', 'NC')",
   "INSERT INTO state (name, abbr) VALUES ('N. CAROLINA', 'NC')",
   "INSERT INTO state (name, abbr) VALUES ('NORTH DAKOTA', 'ND')",
   "INSERT INTO state (name, abbr) VALUES ('N DAKOTA', 'ND')",
   "INSERT INTO state (name, abbr) VALUES ('N. DAKOTA', 'ND')",
   "INSERT INTO state (name, abbr) VALUES ('OHIO', 'OH')",
   "INSERT INTO state (name, abbr) VALUES ('OKLAHOMA', 'OK')",
   "INSERT INTO state (name, abbr) VALUES ('OREGON', 'OR')",
   "INSERT INTO state (name, abbr) VALUES ('PENNSYLVANIA', 'PA')",
   "INSERT INTO state (name, abbr) VALUES ('RHODE ISLAND', 'RI')",
   "INSERT INTO state (name, abbr) VALUES ('SOUTH CAROLINA', 'SC')",
   "INSERT INTO state (name, abbr) VALUES ('S CAROLINA', 'SC')",
   "INSERT INTO state (name, abbr) VALUES ('S. CAROLINA', 'SC')",
   "INSERT INTO state (name, abbr) VALUES ('SOUTH DAKOTA', 'SD')",
   "INSERT INTO state (name, abbr) VALUES ('S DAKOTA', 'SD')",
   "INSERT INTO state (name, abbr) VALUES ('S. DAKOTA', 'SD')",
   "INSERT INTO state (name, abbr) VALUES ('TENNESSEE', 'TN')",
   "INSERT INTO state (name, abbr) VALUES ('TEXAS', 'TX')",
   "INSERT INTO state (name, abbr) VALUES ('UTAH', 'UT')",
   "INSERT INTO state (name, abbr) VALUES ('VERMONT', 'VT')",
   "INSERT INTO state (name, abbr) VALUES ('VIRGINIA', 'VA')",
   "INSERT INTO state (name, abbr) VALUES ('WASHINGTON', 'WA')",
   "INSERT INTO state (name, abbr) VALUES ('WEST VIRGINIA', 'WV')",
   "INSERT INTO state (name, abbr) VALUES ('W VIRGINIA', 'WV')",
   "INSERT INTO state (name, abbr) VALUES ('W. VIRGINIA', 'WV')",
   "INSERT INTO state (name, abbr) VALUES ('WASHINGTON, D.C.', 'DC')",
   "INSERT INTO state (name, abbr) VALUES ('WASHINGTON D.C.', 'DC')",
   "INSERT INTO state (name, abbr) VALUES ('WASHINGTON, DC', 'DC')",
   "INSERT INTO state (name, abbr) VALUES ('WASHINGTON DC', 'DC')",
   "INSERT INTO state (name, abbr) VALUES ('WISCONSIN', 'WI')",
   "INSERT INTO state (name, abbr) VALUES ('WYOMING', 'WY')"

$states | foreach {
   $cmd.CommandText = $_
   $cmd.Connection = $db
   $cmd.ExecuteNonQuery()
}

$db.Close()
cls
$azure = "Server=tcp:___.database.windows.net,1433;Initial Catalog=DTS;Persist Security Info=False;User ID=___;Password=___;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30"
$local = "Data Source=___;Initial Catalog=RSS;timeout=1200;Integrated Security=true"

$dest = new-object System.Data.SqlClient.SqlConnection
$dest.ConnectionString = $azure
$dest.Open()

$src = new-object System.Data.SqlClient.SqlConnection
$src.ConnectionString = $local
$src.Open()

$cmd = new-object System.Data.SqlClient.SqlCommand

# empty existing tables
$cmd.Connection = $dest
$cmd.CommandText = "DELETE FROM rss_item"
$cmd.CommandTimeout = 100000
$cmd.ExecuteNonQuery()
"rss items deleted"

$cmd.Connection = $dest
$cmd.CommandText = "DELETE FROM rss_feed"
$cmd.ExecuteNonQuery()
"rss feeds deleted"

$cmd.Connection = $dest
$cmd.CommandText = "DELETE FROM state"
$cmd.ExecuteNonQuery()
"state info deleted"

$cmd.Connection = $dest
$cmd.CommandText = "DELETE FROM ignore_word"
$cmd.ExecuteNonQuery()
"ignore words deleted"

$cmd.Connection = $dest
$cmd.CommandText = "DELETE FROM profanity"
$cmd.ExecuteNonQuery()
"profanity words deleted"

$bulk = new-object System.Data.SqlClient.SqlBulkCopy($azure, [System.Data.SqlClient.SqlBulkCopyOptions]::KeepIdentity);
$bulk.BatchSize = 1000
$bulk.NotifyAfter = 2000
$bulk.BulkCopyTimeout = 200000

# copy feeds
$cmd.CommandText = "SELECT * FROM rss_feed WITH (NOLOCK)"
$cmd.Connection = $src
$dr = $cmd.ExecuteReader()
$bulk.DestinationTableName = "rss_feed"
$bulk.WriteToServer($dr)
$dr.Close()
"feeds copied"

# copy items
$cmd.CommandText = "SELECT * FROM rss_item WITH (NOLOCK)"
$cmd.Connection = $src
$dr = $cmd.ExecuteReader()
$bulk.DestinationTableName = "rss_item"
$bulk.WriteToServer($dr)
$dr.Close()
"items copied"

# copy state data
$cmd.CommandText = "SELECT * FROM state WITH (NOLOCK)"
$cmd.Connection = $src
$dr = $cmd.ExecuteReader()
$bulk.DestinationTableName = "state"
$bulk.WriteToServer($dr)
$dr.Close()
"states copied"

# copy ignore words
$cmd.CommandText = "SELECT * FROM ignore_word WITH (NOLOCK)"
$cmd.Connection = $src
$dr = $cmd.ExecuteReader()
$bulk.DestinationTableName = "ignore_word"
$bulk.WriteToServer($dr)
$dr.Close()
"ignore words copied"

# copy profanity words
$cmd.CommandText = "SELECT * FROM profanity WITH (NOLOCK)"
$cmd.Connection = $src
$dr = $cmd.ExecuteReader()
$bulk.DestinationTableName = "profanity"
$bulk.WriteToServer($dr)
$dr.Close()
"profanity words copied"

# reindex
$cmd.CommandText = "ALTER INDEX ALL ON rss_feed REBUILD"
$cmd.Connection = $dest
$cmd.ExecuteNonQuery()

$cmd.CommandText = "ALTER INDEX ALL ON rss_item REBUILD"
$cmd.Connection = $dest
$cmd.ExecuteNonQuery()
"indexes rebuilt"

# report
$cmd.CommandText = "SELECT COUNT(*) FROM rss_feed"
$cmd.Connection = $dest
"Dest feeds " + $cmd.ExecuteScalar()

$cmd.CommandText = "SELECT COUNT(*) FROM rss_item"
$cmd.Connection = $dest
"Dest items " + $cmd.ExecuteScalar()

$dest.Close()
$src.Close()
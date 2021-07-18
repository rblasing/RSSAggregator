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

# copy items
$cmd.CommandText = "SELECT MAX(ins_date) FROM rss_item WITH (NOLOCK)"
$cmd.Connection = $dest
$azureDate = [System.DateTime]$cmd.ExecuteScalar();

$cmd.CommandText = "SELECT * FROM rss_item WITH (NOLOCK) WHERE ins_date > @d"
$cmd.Connection = $src
$p = new-object System.Data.SqlClient.SqlParameter
$p.ParameterName = "@d"
$p.Value = $azureDate
$junk = $cmd.Parameters.Add($p)

$dr = $cmd.ExecuteReader()

$bulk = new-object System.Data.SqlClient.SqlBulkCopy($azure, [System.Data.SqlClient.SqlBulkCopyOptions]::KeepIdentity);
$bulk.BatchSize = 1000
$bulk.NotifyAfter = 2000
$bulk.BulkCopyTimeout = 200000
$bulk.DestinationTableName = "rss_item"
$bulk.WriteToServer($dr)
$dr.Close()

# reindex
$cmd.CommandText = "ALTER INDEX ALL ON rss_item REBUILD"
$cmd.CommandTimeout = 200000;
$cmd.Connection = $dest
$cmd.ExecuteNonQuery()

# report
$cmd.CommandText = "SELECT COUNT(*) FROM rss_item"
$cmd.Connection = $dest
"Dest items " + $cmd.ExecuteScalar()

$cmd.CommandText = "EXEC SelectMIMETypes"
$cmd.CommandTimeout = 200000;
$cmd.Connection = $dest
$cmd.ExecuteReader()

$cmd.CommandText = "EXEC SelectNASAArticles"
$cmd.CommandTimeout = 200000;
$cmd.Connection = $dest
$cmd.ExecuteReader()

$cmd.CommandText = "EXEC SelectDailyDistribution"
$cmd.CommandTimeout = 200000;
$cmd.Connection = $dest
$cmd.ExecuteReader()

$cmd.CommandText = "EXEC SelectNPRArticles"
$cmd.CommandTimeout = 200000;
$cmd.Connection = $dest
$cmd.ExecuteReader()

$dest.Close()
$src.Close()
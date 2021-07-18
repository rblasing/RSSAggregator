cls

$db = new-object System.Data.SqlClient.SqlConnection
$db.ConnectionString = "Data Source=___;Initial Catalog=RSS;timeout=1200;Integrated Security=true"
#$db.ConnectionString = "Server=tcp:___.database.windows.net,1433;Initial Catalog=DTS;Persist Security Info=False;User ID=___;Password=___;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30"
$db.Open()

$cmd = new-object System.Data.SqlClient.SqlCommand

$cmd.CommandText = "SELECT * FROM log4net WHERE level != 'WARN' ORDER BY date";
$cmd.Connection = $db
$dr = $cmd.ExecuteReader()

while ($dr.Read())
{
   $dr[0].ToString() + " " + $dr[1].ToString() + " " + $dr[2].ToString() + " " + $dr[3].ToString() + " " + $dr[4].ToString() + " " + $dr[5].ToString()
   ""
}


$dr.Close()

$db.Close()
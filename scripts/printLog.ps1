$db = new-object System.Data.SqlClient.SqlConnection
$db.ConnectionString = "Data Source=___;Initial Catalog=RSS;timeout=1200;Integrated Security=true"
$db.Open()

$cmd = new-object System.Data.SqlClient.SqlCommand

$cmd.CommandText = "SELECT * FROM log4net";
$cmd.Connection = $db
$dr = $cmd.ExecuteReader()

while ($dr.Read())
{
   $dr[0].ToString() + " " + $dr[1].ToString() + " " + $dr[2].ToString() + " " + $dr[3].ToString() + " " + $dr[4].ToString() + " " + $dr[5].ToString()
}


$dr.Close()

$db.Close()
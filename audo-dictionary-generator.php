<html>
 <head>
  <title>Audio Dictionary Generator</title>
 </head>
 <body>

<form method="post" action="hello.php">
  <textarea rows = "5" cols = "60" name="user_input"></textarea>
  <br/>
  <input type="submit" class='button' value="Create" />
</form>

<?php
  $test = $_POST['user_input'];
  echo $test;

  if ($test == "")
    return;

 $myfile = fopen("/tmp/words-list-77.txt", "w") or die("Unable to open file!");
 $txt = "\n";
 fwrite($myfile, $test);
 fclose($myfile);

// Execute program interactively

while (@ ob_end_flush()); // end all output buffers if any

$proc = popen('/home/alex/audio-dictionary/AudioDictionary /tmp/words-list-77.txt', 'r');
echo '<pre>';
while (!feof($proc))
{
    echo fread($proc, 4096);
    @ flush();
}
echo '</pre>';

echo '<br/>';

echo '<a href="result">link</a>';

?>
 </body>
</html>


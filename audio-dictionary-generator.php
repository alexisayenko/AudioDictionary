<html>

<head>
    <title>Audio Dictionary Generator</title>
    <link rel="stylesheet" href="https://unpkg.com/purecss@2.0.5/build/pure-min.css" integrity="sha384-LTIDeidl25h2dPxrB2Ekgc9c7sEC3CWGM6HeFmuDNUjX76Ert4Z4IY714dhZHPLd" crossorigin="anonymous">
    <link rel="stylesheet" href="https://unpkg.com/purecss@2.0.5/build/grids-responsive-min.css">
    <link rel="stylesheet" href="https://unpkg.com/purecss@1.0.1/build/base-min.css">
    <meta name="viewport" content="width=device-width, initial-scale=1">
</head>

<body>

    <form class="pure-form-stacked" method="post" action="audio-dictionary-generator.php">
        <fieldset>
            <textarea style="resize:none;" rows="20" cols="70" name="user_input"></textarea>
            <input class="pure-button pure-button-primary" type="submit" class='button' value="Create" />
            <br /><br />

            <?php
            $words_list = $_POST['user_input'];

            if ($words_list == "")
                return;

            // Save user input
            $words_list_file = fopen("/tmp/audio-dictionary-words-list.txt", "w") or die("Unable to open file!");
            $txt = "\n";
            fwrite($words_list_file, $words_list);
            fclose($words_list_file);


            // Execute AudioDictionary with live console output

            while (@ob_end_flush()); // end all output buffers if any

            $proc = popen('/opt/audio-dictionary/AudioDictionary /tmp/audio-dictionary-words-list.txt', 'r');
            
            echo '<textarea readonly rows=5 cols=70 style="resize:none;">';
            while (!feof($proc)) {
                echo fread($proc, 4096);
                @flush();
            }

            echo '</textarea>';

            // Give a link to the result MP3
            echo '<a class="button-success pure-button" href="result">Download MP3</a>';

            ?>
        </fieldset>
    </form>

</body>

</html>
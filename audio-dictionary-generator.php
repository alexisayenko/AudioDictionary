<html>

<head>
    <title>Audio Dictionary Generator</title>
    <link rel="stylesheet" href="https://unpkg.com/purecss@2.0.5/build/pure-min.css" integrity="sha384-LTIDeidl25h2dPxrB2Ekgc9c7sEC3CWGM6HeFmuDNUjX76Ert4Z4IY714dhZHPLd" crossorigin="anonymous">
    <link rel="stylesheet" href="https://unpkg.com/purecss@2.0.5/build/grids-responsive-min.css">
    <link rel="stylesheet" href="https://unpkg.com/purecss@1.0.1/build/base-min.css">
    <meta name="viewport" content="width=device-width, initial-scale=1">
</head>

<body>

    <?php
    function GUID()
    {
        if (function_exists('com_create_guid') === true) {
            return trim(com_create_guid(), '{}');
        }

        return sprintf('%04X%04X-%04X-%04X-%04X-%04X%04X%04X', mt_rand(0, 65535), mt_rand(0, 65535), mt_rand(0, 65535), mt_rand(16384, 20479), mt_rand(32768, 49151), mt_rand(0, 65535), mt_rand(0, 65535), mt_rand(0, 65535));
    } ?>

    <p>Release date time: datetimeplaceholder</p>

    <form class="pure-form-stacked" method="post" action="audio-dictionary-generator.php">
        <fieldset>
            <textarea maxlength="1000" style="resize:none;" rows="15" cols="70" name="user_input"></textarea>
            <input class="pure-button pure-button-primary" type="submit" class='button' value="Create" />
            <br /><br />

            <?php
            $user_input = $_POST['user_input'];

            if ($user_input == "")
                return;

            // Save user input
            $guid = GUID();
            $words_file_name = "/tmp/{$guid}.txt";
            $output_audio_file_name = "{$guid}.mp3";

            $words_list_resource = fopen($words_file_name, "w") or die("Unable to open file!");
            $txt = "\n";
            fwrite($words_list_resource, $user_input);
            fclose($words_list_resource);

            // Execute AudioDictionary with live console output

            while (@ob_end_flush()); // end all output buffers if any

            $proc = popen("/opt/audio-dictionary/AudioDictionary {$words_file_name} {$output_audio_file_name}", 'r');

            echo '<div style="white-space: pre-line;overflow-y:scroll;height:250px;width:600px">';
            while (!feof($proc)) {
                echo fread($proc, 256);
                @flush();
            }
            echo '</div>';

            shell_exec("ln -s /srv/audio-dictionary/{$output_audio_file_name} ./results/{$output_audio_file_name}");

            // Give a link to the result MP3
            echo "<a class='button-success pure-button' href='results/{$output_audio_file_name}'>Download MP3</a>";

            ?>
        </fieldset>
    </form>

</body>

</html>
#!/bin/bash

show-message(){
  echo -e "* \e[34m$1\e[39m *"
}

show-message "publishing"
cd /mnt/c/Alex/AudioDictionary/
dotnet publish -c release -r debian.10-x64 --self-contained

show-message "copy files"
# cp ./bin/Release/netcoreapp3.1/debian.10-x64/publish/* ~/audio-dictionary/
# chmod a+x ~/audio-dictionary/AudioDictionary
cd ./bin/Release/netcoreapp3.1/debian.10-x64/publish/
tar -czf audio-dictionary.tg . 
scp audio-dictionary.tg alex-ocean:/tmp/

show-message "clean up remote directory and unarchive binaries"
ssh alex-ocean rm /opt/audio-dictionary/*
ssh alex-ocean tar -xzf /tmp/audio-dictionary.tg -C /opt/audio-dictionary/
ssh alex-ocean chmod a+x /opt/audio-dictionary/AudioDictionary

scp /mnt/c/Alex/AudioDictionary/audio-dictionary-generator.php alex-ocean:/var/www/html/

show-message "done"

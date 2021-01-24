#!/bin/bash

show-message(){
  echo -e "* \e[34m$1\e[39m *"
}

show-message "publishing"
cd /mnt/c/Alex/AudioDictionary/
dotnet publish -c release -r debian.10-x64 --self-contained
show-message "clean up ~/audio-dictionary"
rm ~/audio-dictionary/*

show-message "copy files"
# cp ./bin/Release/netcoreapp3.1/debian.10-x64/publish/* ~/audio-dictionary/
# chmod a+x ~/audio-dictionary/AudioDictionary
cd ./bin/Release/netcoreapp3.1/debian.10-x64/publish/
tar -czf audio-dictionary.tg . 
scp -r audio-dictionary.tg alex-ocean:/tmp/
ssh alex-ocean tar -xzf /tmp/audio-dictionary.tg /opt/audio-dictionary/
ssh alex-ocean chmod a+x /opt/audio-dictionary/AudioDictionary

show-message "done"

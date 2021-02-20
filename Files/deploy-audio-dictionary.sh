#!/bin/bash

### Variables
CONFIGURATION=Release
RUNTIME="ubuntu.20.04-x64" #"debian.10-x64"
FRAMEWORK=net5.0

### Functions
show-message(){
  echo -e "* \e[34m$1\e[39m *"
}

copy-full-package(){
  show-message "archiving files"
  cd ./bin/$CONFIGURATION/$FRAMEWORK/$RUNTIME/publish/
  tar -cJf audio-dictionary.tar.xz .

  show-message "copy files"
  scp audio-dictionary.tar.xz alex-ocean:/tmp/

  show-message "clean up remote directory and unarchive binaries"
  ssh alex-ocean rm -r /opt/audio-dictionary/*
  ssh alex-ocean tar -xJf /tmp/audio-dictionary.tar.xz -C /opt/audio-dictionary/
  ssh alex-ocean chmod a+x /opt/audio-dictionary/AudioDictionary
}

copy-minimal(){
  show-message "copy files"
  cd ./bin/$CONFIGURATION/$FRAMEWORK/$RUNTIME/publish/
  scp AudioDictionary* alex-ocean:/opt/audio-dictionary/
  ssh alex-ocean chmod a+x /opt/audio-dictionary/AudioDictionary*
}


### Main Script
show-message "publishing"
cd /mnt/c/Alex/AudioDictionary/
show-message "dotnet publish -c $CONFIGURATION -f $FRAMEWORK -r $RUNTIME --self-contained"
dotnet publish -c $CONFIGURATION -f $FRAMEWORK -r $RUNTIME --self-contained

# copy-full-package
copy-minimal

scp /mnt/c/Alex/AudioDictionary/Files/audio-dictionary-generator.php alex-ocean:/var/www/html/

ssh alex-ocean "sed -i \"s/datetimeplaceholder/`date +'%H:%M %d.%m.%Y'`/g\" /var/www/html/audio-dictionary-generator.php"
ssh alex-ocean cp /opt/audio-dictionary/Files/silence-0.5s.mp3 /srv/audio-dictionary

show-message "done"

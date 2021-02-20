#!/bin/bash

### Variables
CONFIGURATION=Release
RUNTIME="ubuntu.20.04-x64" #"debian.10-x64"
FRAMEWORK=net5.0

LOCAL_PROJECT_DIR=/mnt/c/Alex/AudioDictionary/
REMOTE_BIN_DIR=/opt/audio-dictionary/

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
  ssh alex-ocean rm -r $REMOTE_BIN_DIR/*
  ssh alex-ocean tar -xJf /tmp/audio-dictionary.tar.xz -C $REMOTE_BIN_DIR/
  ssh alex-ocean chmod a+x $REMOTE_BIN_DIR/AudioDictionary
}

copy-minimal(){
  show-message "copy files"
  cd ./bin/$CONFIGURATION/$FRAMEWORK/$RUNTIME/publish/

  scp AudioDictionary alex-ocean:$REMOTE_BIN_DIR/
  scp AudioDictionary.dll alex-ocean:$REMOTE_BIN_DIR/

  ssh alex-ocean chmod a+x $REMOTE_BIN_DIR/AudioDictionary*
}


### Main Script
show-message "publishing"
cd $LOCAL_PROJECT_DIR
show-message "dotnet publish -c $CONFIGURATION -f $FRAMEWORK -r $RUNTIME --self-contained"
dotnet publish -c $CONFIGURATION -f $FRAMEWORK -r $RUNTIME --self-contained

# copy-full-package
copy-minimal

scp $LOCAL_PROJECT_DIR/Files/audio-dictionary-generator.php alex-ocean:/var/www/html/

ssh alex-ocean "sed -i \"s/datetimeplaceholder/`date +'%H:%M %d.%m.%Y'`/g\" /var/www/html/audio-dictionary-generator.php"
# ssh alex-ocean cp $REMOTE_BIN_DIR/Files/silence-0.5s.mp3 /srv/audio-dictionary

show-message "done"

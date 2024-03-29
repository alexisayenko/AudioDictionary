﻿# AudioDictionary

Used components
---------------
- https://purecss.io/ - A set of small, responsive CSS modules
- php 8.1
- apache2
- ffmpeg
- .NET 7
- NAudio
- C# 8


Release/Deploy
-------
Simply run the script 'deploy-audio-dictionary.sh' to automatically build and deploy the self-contained binaries to alex-ocean server.

For more info refer to https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish
Supported RID https://docs.microsoft.com/en-us/dotnet/core/rid-catalog

Setup Linux Environment
-----------------------
/opt
└── [drwxrwxr-x aisayenko aisayenko]  audio-dictionary
/srv
└── [drwxrwxrwx www-data www-data]  audio-dictionary
/var/www/
└── [drwxr-xr-x www-data www-data]  html
    └── [drwxrwxrwx www-data www-data]  results


TODO:
-----
+ css styles for the website
+ guids for generated mp3 dicitonaries
- if _us_1 does not exist, try _us_2 autuomatically
+ restrict max lines
+ amend deploy script to deploy also the website
+ add support german russian dicitonary
+ add support for german articles in front
+ add support for german single-plural dictionary
+ display data to diffirentiate betweem releases. E.g. auto increment release/build number and display somewhere in php page.
- add support for multiple meanings (объект, предмет, вещь = der Gegenstand)
+ add backend support for specifying langugaes: EnRu, RuDe, etc
+ add frontend support for specifying langugaes: EnRu, RuDe, etc
- add images, styles of page
+ add audiopattern to generate vocabulary: Ru.Pause.Pause.En.En.En.En.En.Pause.Pause.Pause
- add logging

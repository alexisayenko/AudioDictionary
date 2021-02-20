# AudioDictionary

Used components
---------------
- https://purecss.io/ - A set of small, responsive CSS modules
- php 7.3
- ffmpeg
- .NET Core 3.1
- NAudio
- C# 8


Release/Deploy
-------
Simply run the script 'deploy-audio-dictionary.sh' to automatically build and deploy the self-contained binaries to alex-ocean server.


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

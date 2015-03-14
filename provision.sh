#!/bin/bash

#test if button is down

wget https://github.com/mikaelnet/DeployMachine/archive/master.zip && unzip -o master.zip && rm master.zip
xbuild DeployMachine.sln

sudo mono DeployMachine/bin/Debug/DeployMachine.exe

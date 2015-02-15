#!/bin/bash

git pull
xbuild DeployMachine.sln
sudo mono DeployMachine/bin/Debug/DeployMachine.exe


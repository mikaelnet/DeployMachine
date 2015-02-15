#!/bin/bash

git clone https://github.com/mikaelnet/DeployMachine
cd DeployMachine
xbuild DeployMachine.sln
sudo mono DeployMachine/bin/Debug/DeployMachine.exe


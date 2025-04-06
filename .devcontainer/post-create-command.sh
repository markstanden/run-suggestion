#!/bin/bash

# Install Azure Functions Core Tools
curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg
sudo mv microsoft.gpg /etc/apt/trusted.gpg.d/microsoft.gpg
sudo sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/debian/$(lsb_release -rs | cut -d"." -f 1)/prod $(lsb_release -cs) main" > /etc/apt/sources.list.d/dotnetdev.list'
sudo apt-get update
sudo apt-get install -y azure-functions-core-tools-4

# Install tree utility (for cleantree function)
sudo apt-get install -y tree

# Add cleantree function to shell profile, used for readme trees
echo 'function cleantree() { pwd && tree -a -d -I "obj|bin|devportal|images|nodejs|build|standard-logic-apps"; }' >> ~/.zshrc
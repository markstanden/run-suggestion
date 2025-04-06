#!/bin/bash

# Restore NuGet packages for the solution
echo "post-create-command: Restoring NuGet packages..."
dotnet restore /workspaces/run-suggestion/RunSuggestion.sln --verbosity minimal

# Install Linux CLI tools
sudo apt-get update && sudo apt-get install -y \
    fzf \
    silversearcher-ag \
    tree

# Define cleantree function for shell profiles
CLEANTREE_FUNC='function cleantree() { pwd && tree -d -I "obj|bin|images|nodejs|build"; }'

# Add to bash profile if it exists
if [ -f ~/.bashrc ]; then
    echo "$CLEANTREE_FUNC" >> ~/.bashrc
fi

# Add to zsh profile if it exists
if [ -f ~/.zshrc ]; then
    echo "$CLEANTREE_FUNC" >> ~/.zshrc
fi
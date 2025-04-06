#!/bin/bash
echo "Running post-create-command.sh script"

echo "Restoring NuGet packages..."
dotnet restore /workspaces/run-suggestion/RunSuggestion.sln --verbosity minimal

echo "Install Linux CLI tools"
sudo apt-get update && sudo apt-get install -y \
    fzf \
    silversearcher-ag \
    tree

echo "Define cleantree function to produce dir trees for readme"
CLEANTREE_FUNC='function cleantree() { pwd && tree -d -I "obj|bin|images|nodejs|build"; }'

if [ -f ~/.bashrc ]; then
    echo "Adding cleantree function to .bashrc"
    echo "$CLEANTREE_FUNC" >> ~/.bashrc
fi

if [ -f ~/.zshrc ]; then
    echo "Adding cleantree function to .zshrc"
    echo "$CLEANTREE_FUNC" >> ~/.zshrc
fi

echo "Setting up Terraform tools..."
if [ -f /workspaces/run-suggestion/tools/terraform/install.sh ]; then
    chmod +x /workspaces/run-suggestion/tools/terraform/install.sh
    /workspaces/run-suggestion/tools/terraform/install.sh
fi

echo "Setting up coding standards and Git hooks..."
if [ -f /workspaces/run-suggestion/.coding-standards/dotnet/setup.sh ]; then
    chmod +x /workspaces/run-suggestion/.coding-standards/dotnet/setup.sh
    /workspaces/run-suggestion/.coding-standards/dotnet/setup.sh
fi

echo "Source the updated shell profile"
if [[ "$SHELL" == *"zsh"* ]]; then
    source ~/.zshrc
elif [[ "$SHELL" == *"bash"* ]]; then
    source ~/.bashrc
fi

echo "post-create-command.sh script complete"
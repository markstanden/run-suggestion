#!/bin/bash

echo ""
echo "Updating .NET workloads..."
sudo sudo dotnet workload update

echo ""
echo "Install Linux CLI tools"
sudo apt-get update && sudo apt-get install -y \
    fzf \
    silversearcher-ag \
    tree

echo ""
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

echo ""
echo "Setting up Terraform tools..."
if [ -f /workspaces/run-suggestion/tools/terraform/install.sh ]; then
    chmod +x /workspaces/run-suggestion/tools/terraform/install.sh
    /workspaces/run-suggestion/tools/terraform/install.sh
fi

echo ""
echo "Setting up coding standards and Git hooks..."
if [ -f /workspaces/run-suggestion/.coding-standards/dotnet/setup.sh ]; then
    chmod +x /workspaces/run-suggestion/.coding-standards/dotnet/setup.sh
    /workspaces/run-suggestion/.coding-standards/dotnet/setup.sh
fi

echo "Setting/Fixing devcontainer git hooks directory..."
git config --local core.hooksPath .git/hooks

echo "Adding custom git aliases..."
echo "'git amend' to add current stage to previous commit"
git config --global alias.amend "commit --amend"
echo "'git uncommit' to soft undo previous commit (return to staging)"
git config --global alias.uncommit 'reset --soft HEAD~1'

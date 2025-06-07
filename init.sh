echo ""
echo "Setting up Terraform tools..."
if [ -f ./tools/terraform/install.sh ]; then
    chmod +x ./tools/terraform/install.sh
    ./tools/terraform/install.sh
fi

echo ""
echo "Setting up coding standards and Git hooks..."
if [ -f ./.coding-standards/dotnet/setup.sh ]; then
    chmod +x ./.coding-standards/dotnet/setup.sh
    ./.coding-standards/dotnet/setup.sh
fi

echo "Setting/Fixing devcontainer git hooks directory..."
git config --local core.hooksPath .git/hooks
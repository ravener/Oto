#!/bin/bash
set -eu

APP_ID="io.github.ravener.Oto"
PREFIX="/app"

# Get the machine architecture
ARCH=$(uname -m)

# Check the architecture and set the ARCH variable
if [ "$ARCH" == "x86_64" ]; then
    ARCH="x64"
elif [ "$ARCH" == "aarch64" ]; then
    ARCH="arm64"
else
    echo "Unsupported architecture"
    exit 1
fi


if [ -d "nuget-sources" ]
then
    mv flatpak/NuGet.config ./
fi


cd Oto/
echo "Publishing linux-$ARCH"
mkdir -p "$PREFIX/lib/$APP_ID"
dotnet build -c Release -r "linux-$ARCH" -o "$PREFIX/lib/$APP_ID" --self-contained

mkdir -p "$PREFIX/bin"
mkdir -p "$PREFIX/share"
mkdir -p "$PREFIX/share/icons/hicolor/scalable/apps"
cp ../data/io.github.ravener.Oto.svg "$PREFIX/share/icons/hicolor/scalable/apps"
cp ../data/io.github.ravener.Oto.desktop "$PREFIX/share/applications"


cat <<EOF > "$PREFIX/bin/Oto"
#!/bin/sh
exec "$PREFIX/lib/$APP_ID/Oto"
EOF

chmod +x "$PREFIX/bin/Oto"

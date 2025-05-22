#!/bin/bash

# setup NuGet.config file
echo Create a NuGet.config file
cat > ../LibCZI_Net.UnitTests/NuGet.config<< EOF
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="Local" value=".\local_packages" />
    <add key="Nuget.Org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
EOF

# generate a Directory.Packages.props file
echo Create a Directory.Packages.props file
# shellcheck disable=SC2154
cat > ../LibCZI_Net.UnitTests/Directory.Packages.props<<EOF
<Project>
  <ItemGroup>
    <PackageVersion Include="LibCZI_Net" Version="${version}" />
  </ItemGroup>
</Project>
EOF

# prepare the local package directoy
echo Create directory for local package source
mkdir ../LibCZI_Net.UnitTests/local_packages

# replace project with package reference
echo Replace project reference with package reference
sed -i 's|<ProjectReference Include="..\\LibCZI_Net\\LibCZI_Net.csproj" />|<PackageReference Include="LibCZI_API" />|' ../LibCZI_Net.UnitTests/LibCZI_Net.UnitTests.csproj

printf "\n***You must now copy a nuget package to the generated local source!***\n\n"

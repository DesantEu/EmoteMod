#!/bin/bash


if [[ $1 == '-r' ]]; then
    modpath='bin/Release/net452/*'
    dotnet build -c Release
else
    modpath='bin/Debug/net452/*'
    dotnet build
fi

rm -rf zips
mkdir zips

cp -r $modpath zips/
cp -r "Dialog" zips/
cp -r "Graphics" zips/
cp -r "everest_.yaml" zips/everest.yaml

cd zips; zip -r EmoteMod.zip *; cp EmoteMod.zip ../../

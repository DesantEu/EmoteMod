#!/bin/bash

set -e # stop on error

if [[ $1 == '-r' ]]; then
    # modpath='bin/Release/net452/*'
    modpath='bin/Release/net8.0/*'
    dotnet build -c Release
else
    # modpath='bin/Debug/net452/*'
    modpath='bin/Debug/net8.0/*'
    dotnet build -v quiet
fi

rm -rf zips
mkdir zips

cp -r $modpath zips/
cp -r "Dialog" zips/
cp -r "Graphics" zips/
cp -r "everest_.yaml" zips/everest.yaml

cd zips; zip -rq EmoteMod.zip *; cp EmoteMod.zip ../../


# start/restart celeste and wait
if pgrep -x "Celeste" > /dev/null
then
    echo "Celeste is running."
    echo "Restarting... $(curl -sf localhost:32270/hotswap)"
    sleep 3
else
    echo "Celeste is not running"
    echo "Starting via Steam..."
    xdg-open "steam://run/504230"
    current_pid=1
fi

echo "Waiting for Celeste..."
while true; do
    if curl --silent --max-time 1 "localhost:32270" > /dev/null
    then
        echo "Celeste detected."
        sleep 5
        echo "Debugger should attach now."
        break
    else
        sleep 1
    fi
done

exit 0

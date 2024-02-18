#!/bin/bash

if ps aux | grep -v grep | grep '^LorePage' > /dev/null; then
    pkill -f '^LorePage'
else
    exit 0
fi

#!/usr/bin/env python3
import sys, base64

if len(sys.argv) < 2:
    print("", end="")
    sys.exit(0)

path = sys.argv[1]
with open(path, "rb") as f:
    data = f.read()

print(base64.b64encode(data).decode("ascii"))

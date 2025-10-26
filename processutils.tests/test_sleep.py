#!/usr/bin/env python3
"""Sleep for given seconds, then print 'done'"""
import sys
import time

seconds = int(sys.argv[1]) if len(sys.argv) > 1 else 5
time.sleep(seconds)
print("done")


# !/usr/bin/env python3

# Use this to serve the built Elements.Playground locally. 

# Adapted from Francesco Pira's python http server with cors: https://fpira.com/blog/2020/05/python-http-server-with-cors

from http.server import HTTPServer, SimpleHTTPRequestHandler
import sys
import os

# app client URL
client = 'http://localhost:8080'

# move to the framework directory
os.chdir('bin/release/net6.0/publish/wwwroot/_framework')


class CORSRequestHandler(SimpleHTTPRequestHandler):
    def end_headers(self):
        self.send_header('Access-Control-Allow-Origin', client)
        self.send_header('Access-Control-Allow-Methods', client)
        self.send_header('Access-Control-Allow-Headers', client)
        self.send_header('Access-Control-Allow-Credentials', 'true')
        self.send_header('Cache-Control', 'no-store, no-cache, must-revalidate')
        return super(CORSRequestHandler, self).end_headers()

    def do_OPTIONS(self):
        self.send_response(200)
        self.end_headers()


host = sys.argv[1] if len(sys.argv) > 2 else '0.0.0.0'
port = int(sys.argv[len(sys.argv) - 1]) if len(sys.argv) > 1 else 5001

print("Listening on {}:{}".format(host, port))
httpd = HTTPServer((host, port), CORSRequestHandler)
httpd.serve_forever()

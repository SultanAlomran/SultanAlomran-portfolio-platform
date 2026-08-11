#!/bin/sh
set -eu
: "${PREVIEW_API_URL:?PREVIEW_API_URL is required}"
find /usr/share/nginx/html -type f -name '*.js' -exec sed -i "s|PORTFOLIO_PREVIEW_API_URL|${PREVIEW_API_URL}|g" {} +
exec nginx -g 'daemon off;'

#!/bin/sh
set -eu
: "${PREVIEW_API_URL:?PREVIEW_API_URL is required}"
find /usr/share/nginx/html -type f -name '*.js' -exec sed -i "s|PORTFOLIO_PREVIEW_API_URL|${PREVIEW_API_URL}|g" {} +
if [ -n "${PREVIEW_WEB_URL:-}" ]; then
  find /usr/share/nginx/html -type f -name '*.js' -exec sed -i "s|PORTFOLIO_PREVIEW_WEB_URL|${PREVIEW_WEB_URL}|g" {} +
fi
exec nginx -g 'daemon off;'

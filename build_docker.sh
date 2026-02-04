#!/bin/bash
NAMESPACE="${1:-codebase_b1378_app}"
docker build -t "$NAMESPACE" .
#!/bin/env bash

# Get the the paths of the plants and images
# Check exactly two arguments are provided
if [ "$#" -ne 2 ]; then
    echo "Usage: $0 <plants_path> <images_path>"
    exit 1
fi 

# Get the important paths 
plants_path=$(realpath "$1")
images_path=$(realpath "$2")

echo "Watching for changes in $plants_path..."
echo "Outputting images to $images_path..."

while true; do
    # Wait for a change in the plants directory
    modified_file="$(inotifywait ${plants_path} -e close_write -q --format %w%f)"

    # Rebuild the image
    plantuml -tpdf "$modified_file" -o "$images_path"
done

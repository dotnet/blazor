#!/usr/bin/env bash

input_dir="obj/data/input"
output_dir="obj/data/output"

if [[ -d "obj/data" ]]
then
    rm -rf "obj/data"
fi

mkdir -p "$input_dir"
mkdir "$output_dir"

curl -L https://data.iana.org/time-zones/tzdata-latest.tar.gz -o "$input_dir/tzdata.tar.gz"
tar xvzf "$input_dir/tzdata.tar.gz" -C "$input_dir"

files=("africa"  "antarctica"  "asia"  "australasia"  "etcetera"  "europe"  "northamerica"  "southamerica")

for file in "${files[@]}"
do
    zic -d "$output_dir" "$input_dir/$file"
done

dotnet run
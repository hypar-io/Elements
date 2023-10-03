DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
rm -r "$DIR/Generated"
source ~/.bash_profile

types=(
"https://schemas.hypar.io/DrainableRoofSection.json"
"https://schemas.hypar.io/Section.json"
"https://schemas.hypar.io/RoofDrain.json"
"https://schemas.hypar.io/RainLoad.json"
"https://schemas.hypar.io/DrainableRoofCharacteristics.json"
"https://schemas.hypar.io/FittingTree.json"
"https://schemas.hypar.io/StraightSegment.json"
"https://schemas.hypar.io/Elbow.json"
"https://schemas.hypar.io/Assembly.json"
"https://schemas.hypar.io/Wye.json"
"https://schemas.hypar.io/Reducer.json"
"https://schemas.hypar.io/Terminal.json"
"https://schemas.hypar.io/Coupler.json"
"https://schemas.hypar.io/Cross.json"
"https://schemas.hypar.io/EquipmentBase.json"
"https://schemas.hypar.io/Manifold.json"
)
for t in ${types[@]}; do
    hypar generate-types -u $t -o $DIR/Generated
done;

rm -r "$DIR/Generated/Tree.g.cs"
sed -i 's/using System;/using System;\nusing Elements.Flow;/g' "$DIR/Generated/Terminal.g.cs"
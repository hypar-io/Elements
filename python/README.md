# Hypar
Hypar is a generative design platform for AEC. This package contains modules for use in creating a custom function to execute on the Hypar platform.

## Modules
### glTF
The glTF module contains classes and methods for working with [glTF](https://www.khronos.org/gltf/) models. Hypar stores geometry in glTF format.

## Prerequisites
- For testing, and deployment you'll need to install a number of packages.  
    - `python3 -m pip install --user --upgrade setuptools wheel twine`

## Packaging
```bash
python3 setup.py sdist bdist_wheel
```

## Uploading to PyPi
```bash
twine upload dist/*
```
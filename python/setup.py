import setuptools
from os import path
this_directory = path.abspath(path.dirname(__file__))
with open(path.join(this_directory, 'README.md'), encoding='utf-8') as f:
    long_description = f.read()

setuptools.setup(name='hypar',
                 version='0.0.3',
                 description='Generative design platform for AEC.',
                 long_description=long_description,
                 long_description_content_type='text/markdown',
                 url='https://github.com/hypar-io',
                 author='Hypar',
                 author_email='ian@hypar.io',
                 license='MIT',
                 packages=setuptools.find_packages(),
                 zip_safe=False)

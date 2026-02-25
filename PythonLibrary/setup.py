from setuptools import setup, find_packages

setup(
    name="python-lib",
    version="1.0.0",
    author="Gaurav Aggarwal",
    author_email="gauravaggarwalin@yahoo.com",
    description="Reusable common utility library",
    long_description=open("README.md").read(),
    long_description_content_type="text/markdown",
    packages=find_packages(),
    python_requires=">=3.8",
    install_requires=[
        # "requests>=2.31.0",  # Example dependency
    ],
    classifiers=[
        "Programming Language :: Python :: 3",
        "Operating System :: OS Independent",
    ],
)
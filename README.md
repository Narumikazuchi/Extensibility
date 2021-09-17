![Logo](../master/logo.png)

# Utility Library
This library originally contained all classes that I used in my projects more than once. I made them universal instead of being hardcoded for the project in question and added them to the library in order to reuse them whenever needed. With time the library grew and not all aspects were needed in every project. That's when I decided to split them into organized individual pieces and also publish them on github as well as nuget.org.

# Extensibility
The spiritual successor of the former Narumikazuchi.Plugins library. I myself love extentable applications, because there are always people thinking of possibilites that I am not. But not everyone is sincere so in order to protect the integrity of the code, the library now offers support for the isolation of an extension from the main application. This even prevents a hard crash of the whole application, when a single extension encounters an exception. Due to the fact that methods and properties are gonna be invoked through reflection as well as having to serialize the parameters and return values into bytes to send them over a pipe connection to the isolated process slows down the application as well as limits the kind of methods and properties, that can be exposed to the main application.
  
## Installation
[![NuGet](https://img.shields.io/nuget/v/Narumikazuchi.Extensibility.svg)](https://www.nuget.org/packages/Narumikazuchi.Extensibility)  
The installation can be simply done via installing the nuget package or by downloading the latest release here from github and referencing it in your project.

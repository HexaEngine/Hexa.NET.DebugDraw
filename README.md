# DebugDraw Library for HexaEngine

The DebugDraw library is a versatile and straightforward 3D debug drawing library designed for the HexaEngine in Immediate-mode. It provides the capability to draw various shapes and text on the screen, making it an essential tool for debugging and prototyping in 3D applications. Although it is a submodule of the HexaEngine, it can also be used as a standalone library.

## Features

### Shapes
- **3D Line**
- **Quad**
- **Ring**
- **Text** (Coming soon)
- **Images**
- **Boxes**
- **Spheres**
- **Planes**
- **Triangles**
- **Cylinders**
- **Cones**
- **Capsules**
- **Frustums**
- **Grids**
- **Rays**
- **Quad Billboard**
- **Custom Shapes** (Supports any primitive topology type, including single texturing)

> Note: The library is actively being developed, and additional shapes will be included in future updates. Ensure that the primitive topology types are supported by your graphics API for correct display.

## Supported Platforms
The DebugDraw library is compatible with all major graphics APIs, including:
- **OpenGL**
- **Vulkan**
- **DirectX**
- **Metal**

This wide range of support ensures that the library can be used across different platforms seamlessly.

## Installation and Usage
To integrate DebugDraw into your project, follow these steps:

### Using NuGet Package
The easiest way to add the DebugDraw library to your project is via the NuGet package.

1. **Installation**: Use the NuGet Package Manager to install the `Hexa.NET.DebugDraw` package.
   ```sh
   dotnet add package Hexa.NET.DebugDraw
   ```
2. Initialization: Initialize the library in your application.
   See examples [Example](https://github.com/HexaEngine/Hexa.NET.DebugDraw/Example).
4. Drawing: Use the provided functions to draw shapes and text for debugging purposes.
   See examples [Example](https://github.com/HexaEngine/Hexa.NET.DebugDraw/Example)

## Contributing

Contributions to the DebugDraw library are welcome. If you have any suggestions, bug reports, or feature requests, please submit them via the project's issue tracker or create a pull request.

## License

The DebugDraw library is released under the MIT License. See the [LICENSE](https://github.com/HexaEngine/Hexa.NET.DebugDraw/blob/master/LICENSE.txt) file for more details.

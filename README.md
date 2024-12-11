# URP Wireframe Shader System for Mobile, PC & Consoles

![Clip-1](https://github.com/user-attachments/assets/da07a42a-0091-47e0-b43f-728d8f0af1af)

A high-performance wireframe rendering solution specifically designed for Unity's Universal Render Pipeline (URP), optimized for mobile devices. This system uses barycentric coordinates to achieve efficient wireframe rendering without geometry shaders, making it ideal for mobile platforms.


## Key Features

```
Mobile-optimized wireframe rendering
Barycentric coordinate-based edge detection
Compatible with Unity URP
Editor tooling for mesh preprocessing
Automatic mesh data generation
Texture-based edge detection support
Color correction and lighting support
Performance-focused implementation
```

![Clip-2](https://github.com/user-attachments/assets/d0d77da9-a8cd-4cd1-966f-17ce3a0cb87f)


## Usage
```

Mesh Preprocessing (Editor):

Processes original meshes to store barycentric coordinates
Generates optimized mesh data for runtime use
Handles UV, normal, and color data preservation
Creates serialized mesh data assets

![Clip-3](https://github.com/user-attachments/assets/5253deb6-57a1-4929-8a8d-f2f851ad1bac)

Runtime Rendering:

Uses preprocessed mesh data for efficient wireframe rendering
Applies barycentric coordinate-based edge detection
Supports texture-based edge detection
Implements proper transparency handling
```

![Clip-4](https://github.com/user-attachments/assets/5937ef04-2488-46b1-b378-c5eb55e61deb)


## Setting Up the Wireframe
```

Open the Mesh Preprocessor tool:
Tools > Wireframe Shader Tool > Mesh Preprocessor

![Screenshot 2024-12-11 at 2 44 51 AM](https://github.com/user-attachments/assets/04f0423d-4c2c-488a-a29e-2a24328e71a4)


In the Mesh Preprocessor window:

Select your source parent object containing meshes
Choose an output path for processed mesh data
Enable "Auto-Add Components" if desired
Click "Process Meshes"

![Screenshot 2024-12-11 at 2 31 17 AM](https://github.com/user-attachments/assets/008a96fc-fa52-46ee-a039-0d11efaf6147)

Apply the wireframe shader:

Select your processed object
Assign the "Custom/URPWireframeWithEdges_Mobile_V2" shader
Adjust shader properties as needed

```

![Clip-5 (1)](https://github.com/user-attachments/assets/b51d8f07-c040-4e1b-a003-08ea41aef839)


## Main Properties

```
Main Texture: Base texture for the object
Main Color: Primary color of the mesh
Edge Color: Color of the wireframe edges
Edge Width: Width of wireframe lines
Edge Threshold: Threshold for edge detection

```

![Clip-6](https://github.com/user-attachments/assets/d6678d67-d4d9-4ec7-be68-4e9a0bbf94a7)


## Edge Detection

```
Texture Edge Threshold: Sensitivity for texture-based edges
Texture Edge Sharpness: Sharpness of texture edges
Texture Edge Color: Color for texture-based edges

```

## Visual Adjustments

```
Brightness: Overall brightness adjustment
Contrast: Contrast adjustment
Saturation: Color saturation control
Hue: Hue shift control
Ambient Strength: Ambient lighting intensity

```

## Notes

Tested on Android, IOS,
Unity 2021.2 or higher
Universal Render Pipeline (URP)
Target platform supporting Shader Model 3.0


## Acknowledgements

 - [Awesome Catlike Coding](https://catlikecoding.com/unity/tutorials/advanced-rendering/flat-and-wireframe-shading/)
 - [Awesome README](https://en.wikipedia.org/wiki/Barycentric_coordinate_system)


## License

[MIT](https://choosealicense.com/licenses/mit/)

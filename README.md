# MAUI Code Obfuscator

**MAUI Code Obfuscator** is a tool designed to approach code obfuscation from a different perspective than most existing obfuscators. The primary goal is to obfuscate the **source code** directly during the compilation process, without relying on the final `.dll` assembly generated by MSBuild.

## Project Scope

This project is initially focused on obfuscating the source code for **.NET MAUI** applications, ensuring that the final package is protected at the source level. As development progresses, I may consider extending support to other C# frameworks based on the effectiveness of the approach and feedback from contributors.

## Obfuscation Approach

Unlike traditional obfuscators that work on the compiled assembly, this tool aims to obfuscate the **source code** itself during the build process. The obfuscator will create a temporary copy of the project, obfuscate the source files, and proceed with the compilation. The original source code remains intact, allowing for regular development workflows without interference.

At this stage, the exact execution process for source code obfuscation is not yet fully defined. I will refine this approach as the project evolves, using test projects to validate the methodology.

## Contribution

If you have a clear idea of how to contribute to this project, feel free to initiate a discussion and coordinate if your suggestion aligns with the project's direction. I am open to viable recommendations that can enhance the development of this tool.

## Future Development

The primary focus of this obfuscator is currently **.NET MAUI** applications. However, depending on the progress and potential improvements, I may consider adding support for other C# frameworks in the future.

## License

This project is licensed under the [MIT License](LICENSE).